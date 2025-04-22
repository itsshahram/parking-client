using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Mig_4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCardMissing",
                table: "ParkingTickets",
                type: "bit",
                nullable: true, defaultValue: false);
            migrationBuilder.CreateTable(
                name: "ParkingTicketImages",
                columns: table => new
                {
                    TicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntryImageAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExitImageAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkingTicketImages", x => x.TicketId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCardMissing",
                table: "ParkingTickets");
            migrationBuilder.DropTable(
                name: "ParkingTicketImages");

        }
    }
}
