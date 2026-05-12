using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ClinicaMedicala.Migrations
{
    /// <inheritdoc />
    public partial class AddReguliConsultatie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReguliConsultatii",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipProgramare = table.Column<int>(type: "int", nullable: false),
                    NecesitaAsistent = table.Column<bool>(type: "bit", nullable: false),
                    Descriere = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReguliConsultatii", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ReguliConsultatii",
                columns: new[] { "Id", "Descriere", "NecesitaAsistent", "TipProgramare" },
                values: new object[,]
                {
                    { 1, "Consultație simplă — fără cerințe speciale.", false, 0 },
                    { 2, "Re-evaluare pacient — de obicei fără asistent.", false, 1 },
                    { 3, "Intervenție clinică — asistent obligatoriu.", true, 2 },
                    { 4, "Stabilizare pacient — asistent obligatoriu.", true, 3 },
                    { 5, "Consultație remote — fără asistent fizic.", false, 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReguliConsultatii_TipProgramare",
                table: "ReguliConsultatii",
                column: "TipProgramare",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReguliConsultatii");
        }
    }
}
