using System;
using FartmaalerAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace FartmaalerAPI.Migrations
{
    // Fortæller at denne snapshot fil hører til AppDbContext
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        // BuildModel beskriver hvordan databasen ser ud lige nu
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618

            // Gemmer information om EF version og SQL Server indstillinger
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.26")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            // Angiver at SQL Server bruger identity columns til auto increment Id'er
            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            // Beskriver Group tabellen
            modelBuilder.Entity("FartmaalerAPI.Models.Group", b =>
            {
                // Primary key / auto increment Id
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int");

                SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                // Angiver om gruppen er låst
                b.Property<bool>("IsLocked")
                    .HasColumnType("bit");

                // Gruppens navn
                b.Property<string>("Name")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                // Skolens navn
                b.Property<string>("School")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                // Sætter Id som primary key
                b.HasKey("Id");

                // Mapper modellen til Groups-tabellen
                b.ToTable("Groups");
            });

            // Beskriver Measurement tabellen
            modelBuilder.Entity("FartmaalerAPI.Models.Measurement", b =>
            {
                // Primary key / auto increment Id
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int");

                SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                // CO2 værdi for målingen
                b.Property<double>("Co2")
                    .HasColumnType("float");

                // Beregnet CO2 besparelse
                b.Property<double>("Co2Saved")
                    .HasColumnType("float");

                // Tidspunkt hvor målingen blev oprettet
                b.Property<DateTime>("CreatedAt")
                    .HasColumnType("datetime2");

                // Afstanden mellem sensorerne
                b.Property<double>("Distance")
                    .HasColumnType("float");

                // Den målte hastighed
                b.Property<double>("MeasuredSpeed")
                    .HasColumnType("float");

                // Foreign key til Session
                b.Property<int>("SessionId")
                    .HasColumnType("int");

                // Den simulerede hastighed efter skalering
                b.Property<double>("SimulatedSpeed")
                    .HasColumnType("float");

                // Tiden bilen brugte mellem sensorerne
                b.Property<double>("Time")
                    .HasColumnType("float");

                // Sætter Id som primary key
                b.HasKey("Id");

                // Laver index på SessionId for hurtigere opslag
                b.HasIndex("SessionId");

                // Mapper modellen til Measurements tabellen
                b.ToTable("Measurements");
            });

            // Beskriver Session tabellen
            modelBuilder.Entity("FartmaalerAPI.Models.Session", b =>
            {
                // Primary key / auto increment Id
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int");

                SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                // Biltype for forsøget
                b.Property<string>("CarType")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                // Tidspunkt hvor sessionen blev oprettet
                b.Property<DateTime>("CreatedAt")
                    .HasColumnType("datetime2");

                // Tidspunkt hvor sessionen blev afsluttet
                b.Property<DateTime?>("EndedAt")
                    .HasColumnType("datetime2");

                // Foreign key til Group
                b.Property<int>("GroupId")
                    .HasColumnType("int");

                // Vejtype for forsøget
                b.Property<string>("RoadType")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                // Faktor der bruges til simuleret hastighed
                b.Property<double>("ScalingFactor")
                    .HasColumnType("float");

                // Hastighedsgrænsen i sessionen
                b.Property<int>("SpeedLimit")
                    .HasColumnType("int");

                // Status for sessionen
                b.Property<string>("Status")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                // Sætter Id som primary key
                b.HasKey("Id");

                // Laver index på GroupId for hurtigere opslag
                b.HasIndex("GroupId");

                // Mapper modellen til Sessions tabellen
                b.ToTable("Sessions");
            });

            // Beskriver User tabellen
            modelBuilder.Entity("FartmaalerAPI.Models.User", b =>
            {
                // Primary key / auto increment Id
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int");

                SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                // Hashet password
                b.Property<string>("PasswordHash")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                // Brugerens rolle
                b.Property<string>("Role")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                // Brugernavn
                b.Property<string>("Username")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                // Sætter Id som primary key
                b.HasKey("Id");

                // Mapper modellen til Users tabellen
                b.ToTable("Users");
            });

            // Beskriver relationen mellem Measurement og Session
            modelBuilder.Entity("FartmaalerAPI.Models.Measurement", b =>
            {
                // En Measurement hører til en Session
                b.HasOne("FartmaalerAPI.Models.Session", "Session")
                    .WithMany()
                    .HasForeignKey("SessionId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                // Navigation property til Session
                b.Navigation("Session");
            });

            // Beskriver relationen mellem Session og Group
            modelBuilder.Entity("FartmaalerAPI.Models.Session", b =>
            {
                // En Session hører til én Group
                b.HasOne("FartmaalerAPI.Models.Group", "Group")
                    .WithMany()
                    .HasForeignKey("GroupId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                // Navigation property til Group
                b.Navigation("Group");
            });

#pragma warning restore 612, 618
        }
    }
}