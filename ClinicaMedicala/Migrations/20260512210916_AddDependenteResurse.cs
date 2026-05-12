using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicaMedicala.Migrations
{
    /// <inheritdoc />
    public partial class AddDependenteResurse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DependenteResurse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResursaPrincipalaId = table.Column<int>(type: "int", nullable: false),
                    ResursaCerutaId = table.Column<int>(type: "int", nullable: false),
                    Descriere = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DependenteResurse", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DependenteResurse_Resurse_ResursaCerutaId",
                        column: x => x.ResursaCerutaId,
                        principalTable: "Resurse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DependenteResurse_Resurse_ResursaPrincipalaId",
                        column: x => x.ResursaPrincipalaId,
                        principalTable: "Resurse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DependenteResurse_ResursaCerutaId",
                table: "DependenteResurse",
                column: "ResursaCerutaId");

            migrationBuilder.CreateIndex(
                name: "IX_DependenteResurse_ResursaPrincipalaId_ResursaCerutaId",
                table: "DependenteResurse",
                columns: new[] { "ResursaPrincipalaId", "ResursaCerutaId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DependenteResurse");
        }
    }
}
