using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ClinicaMedicala.Migrations
{
    /// <inheritdoc />
    public partial class AddSpecializariSiResursaSpecializare : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SpecializarePermisa",
                table: "Resurse");

            migrationBuilder.CreateTable(
                name: "Specializari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nume = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descriere = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Activ = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specializari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ResursaSpecializare",
                columns: table => new
                {
                    ResurseId = table.Column<int>(type: "int", nullable: false),
                    SpecializariId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResursaSpecializare", x => new { x.ResurseId, x.SpecializariId });
                    table.ForeignKey(
                        name: "FK_ResursaSpecializare_Resurse_ResurseId",
                        column: x => x.ResurseId,
                        principalTable: "Resurse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResursaSpecializare_Specializari_SpecializariId",
                        column: x => x.SpecializariId,
                        principalTable: "Specializari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Specializari",
                columns: new[] { "Id", "Activ", "Descriere", "Nume" },
                values: new object[,]
                {
                    { 1, true, null, "Medicină de familie" },
                    { 2, true, null, "Medicină internă" },
                    { 3, true, null, "Cardiologie" },
                    { 4, true, null, "Pediatrie" },
                    { 5, true, null, "Chirurgie generală" },
                    { 6, true, null, "Ortopedie și traumatologie" },
                    { 7, true, null, "Obstetrică-Ginecologie" },
                    { 8, true, null, "Neurologie" },
                    { 9, true, null, "Dermatologie" },
                    { 10, true, null, "Oftalmologie" },
                    { 11, true, null, "ORL" },
                    { 12, true, null, "Stomatologie" },
                    { 13, true, null, "Endocrinologie" },
                    { 14, true, null, "Psihiatrie" },
                    { 15, true, null, "Radiologie imagistică" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResursaSpecializare_SpecializariId",
                table: "ResursaSpecializare",
                column: "SpecializariId");

            migrationBuilder.CreateIndex(
                name: "IX_Specializari_Nume",
                table: "Specializari",
                column: "Nume",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResursaSpecializare");

            migrationBuilder.DropTable(
                name: "Specializari");

            migrationBuilder.AddColumn<string>(
                name: "SpecializarePermisa",
                table: "Resurse",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
