using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shop.Infrastructure.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "coffees",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    name = table.Column<string>(type: "text", nullable: false),
                    origin = table.Column<string>(type: "text", nullable: false),
                    roast_style = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false, defaultValueSql: "'Draft'"),
                    stock_mode = table.Column<string>(type: "text", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("coffees_pkey", x => x.id);
                    table.CheckConstraint("coffees_roast_style_check", "roast_style IN ('Filter', 'Espresso')");
                    table.CheckConstraint("coffees_status_check", "status IN ('Draft', 'Published', 'Retired')");
                    table.CheckConstraint("coffees_stock_mode_check", "stock_mode IN ('Shelf', 'RoastToOrder')");
                });

            migrationBuilder.CreateTable(
                name: "coffee_prices",
                columns: table => new
                {
                    bag_size = table.Column<string>(type: "text", nullable: false),
                    coffee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("coffee_prices_pkey", x => new { x.coffee_id, x.bag_size });
                    table.CheckConstraint("coffee_prices_amount_check", "amount > 0");
                    table.CheckConstraint("coffee_prices_bag_size_check", "bag_size IN ('Grams250', 'Kilogram1')");
                    table.CheckConstraint("coffee_prices_currency_check", "currency = 'EUR'");
                    table.ForeignKey(
                        name: "coffee_prices_coffee_id_fkey",
                        column: x => x.coffee_id,
                        principalTable: "coffees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_coffees_published",
                table: "coffees",
                column: "status",
                filter: "status = 'Published'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "coffee_prices");

            migrationBuilder.DropTable(
                name: "coffees");
        }
    }
}
