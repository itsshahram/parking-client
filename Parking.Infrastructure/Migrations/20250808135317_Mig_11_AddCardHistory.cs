using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Mig_11_AddCardHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AddCardItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerFullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeactiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnLicensePlate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PercentDiscount = table.Column<int>(type: "int", nullable: false),
                    VehicleSegmentId = table.Column<int>(type: "int", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddCardItems", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddCardItems");
        }
    }
}
