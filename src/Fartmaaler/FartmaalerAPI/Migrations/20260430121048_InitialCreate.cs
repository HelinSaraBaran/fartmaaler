using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FartmaalerAPI.Migrations
{
    // Denne migration opretter den første database struktur for projektet
    // Den indeholder tabeller til grupper, brugere, sessions og målinger
    public partial class InitialCreate : Migration
    {
        // Up metoden køres, når migrationen tilføjes til databasen
        // Her oprettes tabeller, kolonner, primary keys, foreign keys og indexes
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Opretter Groups tabellen
            // Tabellen bruges til hold/klasser i fartmåler systemet
            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    // Primary key med auto increment
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    // Gruppens navn
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),

                    // Skolen som gruppen tilhører
                    School = table.Column<string>(type: "nvarchar(max)", nullable: false),

                    // Angiver om gruppen er låst
                    IsLocked = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    // Sætter Id som primary key
                    table.PrimaryKey("PK_Groups", x => x.Id);
                });

            // Opretter Users tabellen
            // Tabellen bruges til login og roller i systemet
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    // Primary key med auto increment
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    // Brugerens login navn
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),

                    // Hashet password, så vi ikke gemmer password i klar tekst
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),

                    // Brugerens rolle, fx admin eller bruger
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    // Sætter Id som primary key
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            // Opretter Sessions tabellen
            // En session er et forsøg/måleforløb for en gruppe
            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    // Primary key med auto increment
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    // Foreign key til Groups tabellen
                    GroupId = table.Column<int>(type: "int", nullable: false),

                    // Biltypen der bruges i forsøget
                    CarType = table.Column<string>(type: "nvarchar(max)", nullable: false),

                    // Vejtypen der bruges i forsøget
                    RoadType = table.Column<string>(type: "nvarchar(max)", nullable: false),

                    // Hastighedsgrænsen for sessionen
                    SpeedLimit = table.Column<int>(type: "int", nullable: false),

                    // Faktor der bruges til at skalere målingen til simuleret hastighed
                    ScalingFactor = table.Column<double>(type: "float", nullable: false),

                    // Status for sessionen, fx aktiv eller afsluttet
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),

                    // Tidspunkt hvor sessionen blev oprettet
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),

                    // Tidspunkt hvor sessionen blev afsluttet
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    // Sætter Id som primary key
                    table.PrimaryKey("PK_Sessions", x => x.Id);

                    // Opretter relation mellem Sessions og Groups
                    // En gruppe kan have flere sessions
                    table.ForeignKey(
                        name: "FK_Sessions_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Opretter Measurements tabellen
            // Tabellen gemmer de konkrete målinger fra fartmåleren
            migrationBuilder.CreateTable(
                name: "Measurements",
                columns: table => new
                {
                    // Primary key med auto increment
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    // Foreign key til Sessions tabellen
                    SessionId = table.Column<int>(type: "int", nullable: false),

                    // Den målte hastighed fra sensorerne
                    MeasuredSpeed = table.Column<double>(type: "float", nullable: false),

                    // Den simulerede hastighed efter scaling factor
                    SimulatedSpeed = table.Column<double>(type: "float", nullable: false),

                    // Tiden bilen brugte mellem sensorerne
                    Time = table.Column<double>(type: "float", nullable: false),

                    // Afstanden mellem sensorerne
                    Distance = table.Column<double>(type: "float", nullable: false),

                    // Beregnet CO2 værdi
                    Co2 = table.Column<double>(type: "float", nullable: false),

                    // Beregnet CO2 besparelse
                    Co2Saved = table.Column<double>(type: "float", nullable: false),

                    // Tidspunkt hvor målingen blev oprettet
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    // Sætter Id som primary key
                    table.PrimaryKey("PK_Measurements", x => x.Id);

                    // Opretter relation mellem Measurements og Sessions
                    // En session kan have flere målinger
                    table.ForeignKey(
                        name: "FK_Measurements_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Opretter index på SessionId, så søgning på målinger pr. session bliver hurtigere
            migrationBuilder.CreateIndex(
                name: "IX_Measurements_SessionId",
                table: "Measurements",
                column: "SessionId");

            // Opretter index på GroupId, så søgning på sessions pr. gruppe bliver hurtigere
            migrationBuilder.CreateIndex(
                name: "IX_Sessions_GroupId",
                table: "Sessions",
                column: "GroupId");
        }

        // Down metoden køres, hvis migrationen skal rulles tilbage
        // Her slettes tabellerne igen i korrekt rækkefølge pga. foreign keys
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Sletter Measurements først, fordi den afhænger af Sessions
            migrationBuilder.DropTable(
                name: "Measurements");

            // Sletter Users tabellen
            migrationBuilder.DropTable(
                name: "Users");

            // Sletter Sessions efter Measurements
            migrationBuilder.DropTable(
                name: "Sessions");

            // Sletter Groups til sidst, fordi Sessions afhænger af Groups
            migrationBuilder.DropTable(
                name: "Groups");
        }
    }
}