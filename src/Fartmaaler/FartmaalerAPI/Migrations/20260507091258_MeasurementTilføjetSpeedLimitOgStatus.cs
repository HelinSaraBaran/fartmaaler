using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FartmaalerAPI.Migrations
{
    /// <inheritdoc />
    public partial class MeasurementTilføjetSpeedLimitOgStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SpeedLimit",
                table: "Measurements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Measurements",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SpeedLimit",
                table: "Measurements");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Measurements");
        }
    }
}
