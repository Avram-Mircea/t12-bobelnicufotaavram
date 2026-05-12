using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicaMedicala.Migrations
{
    /// <inheritdoc />
    public partial class AddPerioadeMentenanta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PerioadeMentenanta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResursaId = table.Column<int>(type: "int", nullable: false),
                    Inceput = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Sfarsit = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Descriere = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerioadeMentenanta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerioadeMentenanta_Resurse_ResursaId",
                        column: x => x.ResursaId,
                        principalTable: "Resurse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PerioadeMentenanta_ResursaId",
                table: "PerioadeMentenanta",
                column: "ResursaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PerioadeMentenanta");
        }
    }
}
