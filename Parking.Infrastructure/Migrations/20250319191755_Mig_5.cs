using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Mig_5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVariableEnable",
                table: "ParkingVehicleSegmentPrice",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ParkingVehicleSegmentVariablePrices",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    ParkingLotId = table.Column<int>(type: "int", nullable: false),
                    VehicleSegmentId = table.Column<int>(type: "int", nullable: false),
                    Number = table.Column<int>(type: "int", nullable: false),
                    Minutes = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkingVehicleSegmentVariablePrices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParkingVehicleSegmentVariablePrices_ParkingLots_ParkingLotId",
                        column: x => x.ParkingLotId,
                        principalTable: "ParkingLots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParkingVehicleSegmentVariablePrices_VehicleSegments_VehicleSegmentId",
                        column: x => x.VehicleSegmentId,
                        principalTable: "VehicleSegments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParkingVehicleSegmentVariablePrices_ParkingLotId",
                table: "ParkingVehicleSegmentVariablePrices",
                column: "ParkingLotId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingVehicleSegmentVariablePrices_VehicleSegmentId",
                table: "ParkingVehicleSegmentVariablePrices",
                column: "VehicleSegmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParkingVehicleSegmentVariablePrices");

            migrationBuilder.DropColumn(
                name: "IsVariableEnable",
                table: "ParkingVehicleSegmentPrice");
        }
    }
}
