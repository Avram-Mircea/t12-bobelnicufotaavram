using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicaMedicala.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Utilizatori",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nume = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Prenume = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ParolaHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Adresa = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Rol = table.Column<int>(type: "int", nullable: false),
                    StatusCont = table.Column<bool>(type: "bit", nullable: false),
                    DataCreareCont = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Discriminator = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    Departament = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Tura = table.Column<int>(type: "int", nullable: true),
                    Specializare = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CodParafa = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: true),
                    GradProfesional = table.Column<int>(type: "int", nullable: true),
                    CostConsultatie = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CNP = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true),
                    DataNastere = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AsiguratCNAS = table.Column<bool>(type: "bit", nullable: true),
                    GrupaSanguina = table.Column<int>(type: "int", nullable: true),
                    AlergiiCunoscute = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ContactUrgentaNume = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ContactUrgentaTelefon = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilizatori", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumenteMedicale",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipDocument = table.Column<int>(type: "int", nullable: false),
                    CaleFisier = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DataIncarcare = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Observatii = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PacientId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumenteMedicale", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumenteMedicale_Utilizatori_PacientId",
                        column: x => x.PacientId,
                        principalTable: "Utilizatori",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FiseMedicale",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IstoricBoliCronice = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AntecedenteFamiliale = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    GrupaDeRisc = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PacientId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FiseMedicale", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FiseMedicale_Utilizatori_PacientId",
                        column: x => x.PacientId,
                        principalTable: "Utilizatori",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Resurse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Denumire = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Tip = table.Column<int>(type: "int", nullable: false),
                    Stare = table.Column<int>(type: "int", nullable: false),
                    NumarInventar = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DataUltimaRevizie = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataScadentaRevizie = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdministratorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resurse", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Resurse_Utilizatori_AdministratorId",
                        column: x => x.AdministratorId,
                        principalTable: "Utilizatori",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Consultatii",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Data = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SimptomePrezentate = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DiagnosticICD10 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TratamentRecomandat = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ObservatiiMedic = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FisaMedicalaId = table.Column<int>(type: "int", nullable: false),
                    MedicId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consultatii", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Consultatii_FiseMedicale_FisaMedicalaId",
                        column: x => x.FisaMedicalaId,
                        principalTable: "FiseMedicale",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Consultatii_Utilizatori_MedicId",
                        column: x => x.MedicId,
                        principalTable: "Utilizatori",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Programari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MotivVizita = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TipProgramare = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PacientId = table.Column<int>(type: "int", nullable: false),
                    MedicId = table.Column<int>(type: "int", nullable: false),
                    ResursaId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Programari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Programari_Resurse_ResursaId",
                        column: x => x.ResursaId,
                        principalTable: "Resurse",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Programari_Utilizatori_MedicId",
                        column: x => x.MedicId,
                        principalTable: "Utilizatori",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Programari_Utilizatori_PacientId",
                        column: x => x.PacientId,
                        principalTable: "Utilizatori",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Consultatii_FisaMedicalaId",
                table: "Consultatii",
                column: "FisaMedicalaId");

            migrationBuilder.CreateIndex(
                name: "IX_Consultatii_MedicId",
                table: "Consultatii",
                column: "MedicId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumenteMedicale_PacientId",
                table: "DocumenteMedicale",
                column: "PacientId");

            migrationBuilder.CreateIndex(
                name: "IX_FiseMedicale_PacientId",
                table: "FiseMedicale",
                column: "PacientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Programari_MedicId",
                table: "Programari",
                column: "MedicId");

            migrationBuilder.CreateIndex(
                name: "IX_Programari_PacientId",
                table: "Programari",
                column: "PacientId");

            migrationBuilder.CreateIndex(
                name: "IX_Programari_ResursaId",
                table: "Programari",
                column: "ResursaId");

            migrationBuilder.CreateIndex(
                name: "IX_Resurse_AdministratorId",
                table: "Resurse",
                column: "AdministratorId");

            migrationBuilder.CreateIndex(
                name: "IX_Resurse_NumarInventar",
                table: "Resurse",
                column: "NumarInventar",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Utilizatori_CNP",
                table: "Utilizatori",
                column: "CNP",
                unique: true,
                filter: "[CNP] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Utilizatori_CodParafa",
                table: "Utilizatori",
                column: "CodParafa",
                unique: true,
                filter: "[CodParafa] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Utilizatori_Email",
                table: "Utilizatori",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Consultatii");

            migrationBuilder.DropTable(
                name: "DocumenteMedicale");

            migrationBuilder.DropTable(
                name: "Programari");

            migrationBuilder.DropTable(
                name: "FiseMedicale");

            migrationBuilder.DropTable(
                name: "Resurse");

            migrationBuilder.DropTable(
                name: "Utilizatori");
        }
    }
}
