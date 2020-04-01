using Microsoft.EntityFrameworkCore.Migrations;

namespace Markt2Go.Migrations
{
    public partial class AddedProtfolio : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Portfolio",
                table: "MarketSellers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Portfolio",
                table: "MarketSellers");
        }
    }
}
