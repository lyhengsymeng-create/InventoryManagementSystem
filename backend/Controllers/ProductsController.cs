
using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/products
    [HttpGet]
    public async Task<ActionResult<ProductListResponseDto>> GetAll(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string? name = null,
    [FromQuery] string? sku = null,
    [FromQuery] int? categoryId = null,
    [FromQuery] decimal? minPrice = null,
    [FromQuery] decimal? maxPrice = null,
    [FromQuery] string sortBy = "id",
    [FromQuery] string sortOrder = "asc")
    {
        // 1. Validate pagination parameters
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new
            {
                message = "Page must be >= 1 and pageSize must be between 1 and 100."
            });
        }

        if ((minPrice.HasValue && minPrice.Value < 0) ||
     (maxPrice.HasValue && maxPrice.Value < 0))
        {
            return BadRequest(new
            {
                message = "Prices cannot be negative."
            });
        }

        if (minPrice.HasValue &&
            maxPrice.HasValue &&
            minPrice.Value > maxPrice.Value)
        {
            return BadRequest(new
            {
                message = "minPrice cannot be greater than maxPrice."
            });
        }

        // 2. Start query; do not load all records yet
        IQueryable<Product> query = _context.Products
            .AsNoTracking();

        // 3. Apply filters
        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(p =>
                EF.Functions.ILike(p.Name, $"%{name.Trim()}%"));
        }

        if (!string.IsNullOrWhiteSpace(sku))
        {
            query = query.Where(p =>
                EF.Functions.ILike(p.SKU, $"%{sku.Trim()}%"));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p =>
                p.CategoryId == categoryId.Value);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(p =>
                p.UnitPrice >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p =>
                p.UnitPrice <= maxPrice.Value);
        }

        // 4. Count filtered records before pagination
        int totalCount = await query.CountAsync();

        // 5. Apply sorting using an allowlist

        bool descending = sortOrder.Equals("desc",  StringComparison.OrdinalIgnoreCase);

        if (!sortOrder.Equals(
        "asc",
        StringComparison.OrdinalIgnoreCase) &&
    !sortOrder.Equals(
        "desc",
        StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message = "sortOrder must be 'asc' or 'desc'."
            });
        }

        switch (sortBy.Trim().ToLowerInvariant())
        {
            case "id":
                query = descending
                    ? query.OrderByDescending(p => p.Id)
                    : query.OrderBy(p => p.Id);
                break;

            case "name":
                query = descending
                    ? query.OrderByDescending(p => p.Name).ThenBy(p => p.Id)
                    : query.OrderBy(p => p.Name).ThenBy(p => p.Id);
                break;

            case "sku":
                query = descending
                    ? query.OrderByDescending(p => p.SKU).ThenBy(p => p.Id)
                    : query.OrderBy(p => p.SKU).ThenBy(p => p.Id);
                break;

            case "unitprice":
                query = descending
                    ? query.OrderByDescending(p => p.UnitPrice).ThenBy(p => p.Id)
                    : query.OrderBy(p => p.UnitPrice).ThenBy(p => p.Id);
                break;

            case "quantity":
                query = descending
                    ? query.OrderByDescending(p => p.Quantity).ThenBy(p => p.Id)
                    : query.OrderBy(p => p.Quantity).ThenBy(p => p.Id);
                break;

            default:
                return BadRequest(new
                {
                    message = "Invalid sortBy value."
                });
        }

        // 6. Apply pagination and project to response DTO
        var products = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                SKU = p.SKU,
                UnitPrice = p.UnitPrice,
                Quantity = p.Quantity,
                CategoryId = p.CategoryId
            })
            .ToListAsync();

        // 7. Return paginated response
        return Ok(new ProductListResponseDto
        {
            Items = products,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(
                totalCount / (double)pageSize)
        });
    }

    // GET: api/products/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductResponseDto>> GetById(int id)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                SKU = p.SKU,
                UnitPrice = p.UnitPrice,
                Quantity = p.Quantity,
                CategoryId = p.CategoryId
            })
            .FirstOrDefaultAsync();

        if (product is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Product not Found",
                detail: $"Prodcut with ID {id}  was not found." );
        }

        return Ok(product);
    }

    // POST: api/products
    [HttpPost]
    public async Task<ActionResult<ProductResponseDto>> Create(
        [FromBody] ProductCreateDto dto)
    {
        bool skuExists = await _context.Products
            .AnyAsync(p => p.SKU == dto.SKU);

        if (skuExists)
        {
            return Problem(
            statusCode: StatusCodes.Status409Conflict,
            title: "Duplicate SKU",
            detail: $"A product with SKU '{dto.SKU}' already exists." );
        }

        // Check whether the category exists
        if (dto.CategoryId.HasValue)
        {
            bool categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == dto.CategoryId.Value);

            if (!categoryExists)
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid category",
                    detail: $"Category with ID {dto.CategoryId.Value} does not exist.");
            }
        }

        var product = new Product
        {
            Name = dto.Name,
            SKU = dto.SKU,
            UnitPrice = dto.UnitPrice,
            Quantity = dto.Quantity,
            CategoryId = dto.CategoryId
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var response = new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            SKU = product.SKU,
            UnitPrice = product.UnitPrice,
            Quantity = product.Quantity,
            CategoryId = product.CategoryId
        };

        return CreatedAtAction(
            nameof(GetById),
            new { id = product.Id },
            response);
    }

    // PUT: api/products/1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] ProductUpdateDto dto)
    {
        var product = await _context.Products.FindAsync(id);

        if (product is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Product not Found",
                detail: $"Prodcut with ID {id}  was not found."
                );
        }

        bool skuExists = await _context.Products.AnyAsync(
            p => p.SKU == dto.SKU && p.Id != id);

        if (skuExists)
        {
            return Problem(
              statusCode: StatusCodes.Status409Conflict,
              title: "Duplicate SKU",
              detail: $"A product with SKU '{dto.SKU}' already exists.");
        }

        if (dto.CategoryId.HasValue)
        {
            bool categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == dto.CategoryId.Value);

            if (!categoryExists)
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid category ",
                    detail : $"Category with ID {dto.CategoryId.Value} does not exist.");
            }
        }

        product.Name = dto.Name;
        product.SKU = dto.SKU;
        product.UnitPrice = dto.UnitPrice;
        product.Quantity = dto.Quantity;
        product.CategoryId = dto.CategoryId;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/products/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product is null)
        {
            return Problem(
               statusCode: StatusCodes.Status404NotFound,
               title: "Product not Found",
               detail: $"Prodcut with ID {id}  was not found." );
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
