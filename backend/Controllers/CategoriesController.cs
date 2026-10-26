using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _context;
    public CategoriesController(AppDbContext context)
    {
        _context = context;
    }
    //GET : api/categories
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Category>>> GetAll()
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .ToListAsync();

            return Ok(categories);
    }
    //GET: api/categoties/{id}
    [HttpGet("{id:int}")]
     public async Task<ActionResult<Category>> GetById(int id)
     {
    var categories = await _context.Categories
        .AsNoTracking()
        .FirstOrDefaultAsync(c => c.Id == id);
        if (categories == null)

            return NotFound();

        return Ok(categories);
     }
    // POST : api/categories
    [HttpPost]
    public async Task<ActionResult<Category>> Create([FromBody] Category category)
    {
    category.Id = 0;
    _context.Categories.Add(category);
    await _context.SaveChangesAsync();

    return CreatedAtAction(nameof(GetById),
        new { id = category.Id }, category);

    }

    // PUT: api/categories/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update( int id, [FromBody] Category category)
    {
        var existingCategory = await _context.Categories
            .FindAsync(id);

        if (existingCategory == null)
        {
            return NotFound();
        }

        existingCategory.Name = category.Name;

        await _context.SaveChangesAsync();

        return NoContent();
    }
    // DELETE : api/categories/{{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        // Find the Category
        var category = await _context.Categories.FindAsync(id);

        // Return 404 if the category does not exist
        if (category == null)
        {
            return NotFound();

        }
        // Check whether any products use this category
        bool hasProducts = await _context.Products
            .AnyAsync(p => p.CategoryId == id);
        if(hasProducts)
        {
            return Conflict("Cannot delete this cotegory because products are assigned to it.");
        }
        // Delete the category
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        // return 204 No Contend
        return NoContent();
    }


}