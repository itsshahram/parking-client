using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Mig_8_AddUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "ParkingTickets",
                type: "uniqueidentifier",
                nullable: true);

            //migrationBuilder.CreateTable(
            //    name: "ApplicationUserRole",
            //    columns: table => new
            //    {
            //        UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //    });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropTable(
            //    name: "ApplicationUserRole");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ParkingTickets");
        }
    }
}
