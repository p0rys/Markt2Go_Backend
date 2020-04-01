using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Markt2Go.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Markets",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: false),
                    DayOfWeek = table.Column<int>(nullable: false),
                    StartTime = table.Column<string>(maxLength: 5, nullable: false),
                    EndTime = table.Column<string>(maxLength: 5, nullable: false),
                    Location = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Markets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sellers",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: false),
                    Category = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sellers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MarketSellers",
                columns: table => new
                {
                    SellerId = table.Column<long>(nullable: false),
                    MarketId = table.Column<long>(nullable: false),
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketSellers", x => new { x.SellerId, x.MarketId });
                    table.ForeignKey(
                        name: "FK_MarketSellers_Markets_MarketId",
                        column: x => x.MarketId,
                        principalTable: "Markets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MarketSellers_Sellers_SellerId",
                        column: x => x.SellerId,
                        principalTable: "Sellers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    SellerId = table.Column<long>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    Mail = table.Column<string>(nullable: false),
                    Firstname = table.Column<string>(nullable: true),
                    Lastname = table.Column<string>(nullable: true),
                    Address = table.Column<string>(nullable: true),
                    Zip = table.Column<string>(nullable: true),
                    Town = table.Column<string>(nullable: true),
                    Phone = table.Column<string>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Sellers_SellerId",
                        column: x => x.SellerId,
                        principalTable: "Sellers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(nullable: false),
                    MarketSellerId = table.Column<long>(nullable: false),
                    Pickup = table.Column<DateTime>(nullable: false),
                    Price = table.Column<decimal>(type: "money", nullable: false),
                    UserComment = table.Column<string>(nullable: true),
                    SellerComment = table.Column<string>(nullable: true),
                    Accepted = table.Column<bool>(nullable: false),
                    Packed = table.Column<bool>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    MarketSellerSellerId = table.Column<long>(nullable: false),
                    MarketSellerMarketId = table.Column<long>(nullable: false),
                    UserId1 = table.Column<string>(nullable: true),
                    SellerId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservations_Sellers_SellerId",
                        column: x => x.SellerId,
                        principalTable: "Sellers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservations_Users_UserId1",
                        column: x => x.UserId1,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservations_MarketSellers_MarketSellerSellerId_MarketSellerMarketId",
                        columns: x => new { x.MarketSellerSellerId, x.MarketSellerMarketId },
                        principalTable: "MarketSellers",
                        principalColumns: new[] { "SellerId", "MarketId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReservationId = table.Column<long>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Amount = table.Column<int>(nullable: false),
                    Unit = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Items_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Items_ReservationId",
                table: "Items",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketSellers_MarketId",
                table: "MarketSellers",
                column: "MarketId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_SellerId",
                table: "Reservations",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_UserId1",
                table: "Reservations",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_MarketSellerSellerId_MarketSellerMarketId",
                table: "Reservations",
                columns: new[] { "MarketSellerSellerId", "MarketSellerMarketId" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_SellerId",
                table: "Users",
                column: "SellerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Reservations");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "MarketSellers");

            migrationBuilder.DropTable(
                name: "Markets");

            migrationBuilder.DropTable(
                name: "Sellers");
        }
    }
}
