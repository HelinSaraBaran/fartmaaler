using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FartmaalerAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddLeaderboardSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LeaderboardSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaderboardSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SchoolLeaderboardMocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RoadType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AverageScore = table.Column<double>(type: "float", nullable: false),
                    AverageCo2 = table.Column<double>(type: "float", nullable: false),
                    MeasurementCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolLeaderboardMocks", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeaderboardSettings");

            migrationBuilder.DropTable(
                name: "SchoolLeaderboardMocks");
        }
    }
}
