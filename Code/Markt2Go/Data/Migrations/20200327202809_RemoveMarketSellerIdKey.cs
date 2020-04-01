using Microsoft.EntityFrameworkCore.Migrations;

namespace Markt2Go.Migrations
{
    public partial class RemoveMarketSellerIdKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_MarketSellers_MarketSellerSellerId_MarketSellerMarketId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_MarketSellerSellerId_MarketSellerMarketId",
                table: "Reservations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MarketSellers",
                table: "MarketSellers");

            migrationBuilder.DropColumn(
                name: "MarketSellerMarketId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "MarketSellerSellerId",
                table: "Reservations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MarketSellers",
                table: "MarketSellers",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_MarketSellerId",
                table: "Reservations",
                column: "MarketSellerId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketSellers_SellerId_MarketId",
                table: "MarketSellers",
                columns: new[] { "SellerId", "MarketId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_MarketSellers_MarketSellerId",
                table: "Reservations",
                column: "MarketSellerId",
                principalTable: "MarketSellers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_MarketSellers_MarketSellerId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_MarketSellerId",
                table: "Reservations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MarketSellers",
                table: "MarketSellers");

            migrationBuilder.DropIndex(
                name: "IX_MarketSellers_SellerId_MarketId",
                table: "MarketSellers");

            migrationBuilder.AddColumn<long>(
                name: "MarketSellerMarketId",
                table: "Reservations",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "MarketSellerSellerId",
                table: "Reservations",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MarketSellers",
                table: "MarketSellers",
                columns: new[] { "SellerId", "MarketId" });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_MarketSellerSellerId_MarketSellerMarketId",
                table: "Reservations",
                columns: new[] { "MarketSellerSellerId", "MarketSellerMarketId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_MarketSellers_MarketSellerSellerId_MarketSellerMarketId",
                table: "Reservations",
                columns: new[] { "MarketSellerSellerId", "MarketSellerMarketId" },
                principalTable: "MarketSellers",
                principalColumns: new[] { "SellerId", "MarketId" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
