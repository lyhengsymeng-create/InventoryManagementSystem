
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using backend.Data;
using backend.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace backend.Tests;

public class ProductsApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ProductsApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }


    [Fact]
    public void ShowTestDatabaseConnection()
    {
        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var connection = db.Database.GetDbConnection();

        connection.Open();

        try
        {
            using var command = connection.CreateCommand();

            command.CommandText = """
            SELECT
                current_database(),
                inet_server_addr()::text,
                inet_server_port(),
                current_schema(),
                to_regclass('public."Products"')::text,
                to_regclass('public.categories')::text,
                to_regclass('public."__EFMigrationsHistory"')::text;
            """;

            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                Console.WriteLine($"Database: {reader.GetValue(0)}");
                Console.WriteLine($"Server IP: {reader.GetValue(1)}");
                Console.WriteLine($"Server port: {reader.GetValue(2)}");
                Console.WriteLine($"Schema: {reader.GetValue(3)}");
                Console.WriteLine($"Products: {reader.GetValue(4)}");
                Console.WriteLine($"Categories: {reader.GetValue(5)}");
                Console.WriteLine($"Migration history: {reader.GetValue(6)}");
            }
        }
        finally
        {
            connection.Close();
        }
    }


    [Fact]
    public async Task GetProducts_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/products?page=1&pageSize=10");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_ValidData_ReturnsCreated()
    {
        // Arrange: prepare a new product
        var product = new
        {
            name = "Integration Test Product",
            sku = $"TEST-{Guid.NewGuid():N}",
            unitPrice = 12.50m,
            quantity = 20,
            categoryId = (int?)null
        };

        // Act: send POST request
        var response = await _client.PostAsJsonAsync( "/api/products",  product);

        // Assert: expect 201 Created
        Assert.Equal( HttpStatusCode.Created,  response.StatusCode);

        // Verify response body
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal( product.name,body.GetProperty("name").GetString());

        Assert.Equal(product.sku,body.GetProperty("sku").GetString());

        Assert.True(body.GetProperty("id").GetInt32() > 0);
    }

    [Fact]
    public async Task CreateProduct_DuplicateSku_ReturnsConflict()
    {
        // Arrange: use a unique SKU for this test
       // Generate a unique value to minimize conflicts with existing SKUs in the test database.
        var sku = $"DUP-{Guid.NewGuid():N}";

        var product = new
        {
            name = "Duplicate SKU Test Product",
            sku = sku,
            unitPrice = 15.00m,
            quantity = 5,
            categoryId = (int?)null
        };

        // Create the first product
        var firstResponse = await _client.PostAsJsonAsync(
            "/api/products",
            product);

        Assert.Equal(
            HttpStatusCode.Created,
            firstResponse.StatusCode);

        // Act: attempt to create another product with the same SKU
        var duplicateProduct = new
        {
            name = "Another Product",
            sku = sku,
            unitPrice = 20.00m,
            quantity = 10,
            categoryId = (int?)null
        };

        var duplicateResponse = await _client.PostAsJsonAsync(
            "/api/products",
            duplicateProduct);

        // Assert: expect 409 Conflict
        Assert.Equal(
            HttpStatusCode.Conflict,
            duplicateResponse.StatusCode);
    }
}