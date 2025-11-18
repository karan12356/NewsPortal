using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ass1Karan.Migrations
{
    /// <inheritdoc />
    public partial class pps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PpsResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Diagnosis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ambulation = table.Column<int>(type: "int", nullable: false),
                    Activity = table.Column<int>(type: "int", nullable: false),
                    Evidence = table.Column<int>(type: "int", nullable: false),
                    SelfCare = table.Column<int>(type: "int", nullable: false),
                    Intake = table.Column<int>(type: "int", nullable: false),
                    Consciousness = table.Column<int>(type: "int", nullable: false),
                    FinalScore = table.Column<int>(type: "int", nullable: false),
                    DateSaved = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserEmail = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PpsResults", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PpsResults");
        }
    }
}
