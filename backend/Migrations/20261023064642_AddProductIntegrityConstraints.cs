using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddProductIntegrityConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_categories_CategoryId",
                table: "Products");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Products_Name_NotEmpty",
                table: "Products",
                sql: "length(trim(\"Name\")) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Products_Quantity_NonNegative",
                table: "Products",
                sql: "\"Quantity\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Products_SKU_NotEmpty",
                table: "Products",
                sql: "length(trim(\"SKU\")) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Products_UnitPrice_NonNegative",
                table: "Products",
                sql: "\"UnitPrice\" >= 0");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_categories_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_categories_CategoryId",
                table: "Products");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Products_Name_NotEmpty",
                table: "Products");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Products_Quantity_NonNegative",
                table: "Products");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Products_SKU_NotEmpty",
                table: "Products");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Products_UnitPrice_NonNegative",
                table: "Products");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_categories_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "categories",
                principalColumn: "Id");
        }
    }
}
