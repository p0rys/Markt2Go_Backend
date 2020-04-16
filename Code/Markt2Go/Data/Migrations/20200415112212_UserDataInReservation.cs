using Microsoft.EntityFrameworkCore.Migrations;

namespace Markt2Go.Migrations
{
    public partial class UserDataInReservation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsValidated",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Town",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Zip",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "Firstname",
                table: "Reservations",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Lastname",
                table: "Reservations",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Reservations",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "RulesAccepted",
                table: "Reservations",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Firstname",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "Lastname",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "RulesAccepted",
                table: "Reservations");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsValidated",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Town",
                table: "Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Zip",
                table: "Users",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true);
        }
    }
}
