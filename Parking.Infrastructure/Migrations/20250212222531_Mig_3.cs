using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Mig_3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DailyPriceAfterCrossingThreshold",
                table: "VehicleSegments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<byte>(
                name: "TaxPercentage",
                table: "VehicleSegments",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "ThresholdHoursPerDay",
                table: "VehicleSegments",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "ThresholdNumberOfDays",
                table: "VehicleSegments",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyPriceAfterCrossingThreshold",
                table: "VehicleSegments");

            migrationBuilder.DropColumn(
                name: "TaxPercentage",
                table: "VehicleSegments");

            migrationBuilder.DropColumn(
                name: "ThresholdHoursPerDay",
                table: "VehicleSegments");

            migrationBuilder.DropColumn(
                name: "ThresholdNumberOfDays",
                table: "VehicleSegments");
        }
    }
}
