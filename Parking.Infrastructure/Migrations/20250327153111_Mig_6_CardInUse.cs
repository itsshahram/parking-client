using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Mig_6_CardInUse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DriverDescription",
                table: "ParkingTickets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DriverFullName",
                table: "ParkingTickets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DriverPhoneNumber",
                table: "ParkingTickets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsInUse",
                table: "Cards",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DriverDescription",
                table: "ParkingTickets");

            migrationBuilder.DropColumn(
                name: "DriverFullName",
                table: "ParkingTickets");

            migrationBuilder.DropColumn(
                name: "DriverPhoneNumber",
                table: "ParkingTickets");

            migrationBuilder.DropColumn(
                name: "IsInUse",
                table: "Cards");
        }
    }
}
