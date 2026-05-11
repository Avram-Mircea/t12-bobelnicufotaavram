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
                    DataCreareCont = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilizatori", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Administratori",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Administratori", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Administratori_Utilizatori_Id",
                        column: x => x.Id,
                        principalTable: "Utilizatori",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Asistenti",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Departament = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Tura = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asistenti", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asistenti_Utilizatori_Id",
                        column: x => x.Id,
                        principalTable: "Utilizatori",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Autentificari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataOra = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Succes = table.Column<bool>(type: "bit", nullable: false),
                    AdresaIp = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UtilizatorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Autentificari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Autentificari_Utilizatori_UtilizatorId",
                        column: x => x.UtilizatorId,
                        principalTable: "Utilizatori",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Medici",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Specializare = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CodParafa = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    GradProfesional = table.Column<int>(type: "int", nullable: false),
                    CostConsultatie = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NumarContractCAS = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medici", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Medici_Utilizatori_Id",
                        column: x => x.Id,
                        principalTable: "Utilizatori",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pacienti",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    CNP = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    DataNastere = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AsiguratCNAS = table.Column<bool>(type: "bit", nullable: false),
                    GrupaSanguina = table.Column<int>(type: "int", nullable: false),
                    AlergiiCunoscute = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ContactUrgentaNume = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ContactUrgentaTelefon = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pacienti", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pacienti_Utilizatori_Id",
                        column: x => x.Id,
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
                    Locatie = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SpecializarePermisa = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DataUltimaRevizie = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataScadentaRevizie = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdministratorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resurse", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Resurse_Administratori_AdministratorId",
                        column: x => x.AdministratorId,
                        principalTable: "Administratori",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MedicAsistenti",
                columns: table => new
                {
                    AsistentiId = table.Column<int>(type: "int", nullable: false),
                    MediciId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicAsistenti", x => new { x.AsistentiId, x.MediciId });
                    table.ForeignKey(
                        name: "FK_MedicAsistenti_Asistenti_AsistentiId",
                        column: x => x.AsistentiId,
                        principalTable: "Asistenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MedicAsistenti_Medici_MediciId",
                        column: x => x.MediciId,
                        principalTable: "Medici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    PacientId = table.Column<int>(type: "int", nullable: false),
                    MedicId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumenteMedicale", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumenteMedicale_Medici_MedicId",
                        column: x => x.MedicId,
                        principalTable: "Medici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DocumenteMedicale_Pacienti_PacientId",
                        column: x => x.PacientId,
                        principalTable: "Pacienti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FiseMedicale",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataCreare = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IstoricBoliCronice = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AntecedenteFamiliale = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    GrupaDeRisc = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PacientId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FiseMedicale", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FiseMedicale_Pacienti_PacientId",
                        column: x => x.PacientId,
                        principalTable: "Pacienti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicPacienti",
                columns: table => new
                {
                    MediciId = table.Column<int>(type: "int", nullable: false),
                    PacientiId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicPacienti", x => new { x.MediciId, x.PacientiId });
                    table.ForeignKey(
                        name: "FK_MedicPacienti_Medici_MediciId",
                        column: x => x.MediciId,
                        principalTable: "Medici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MedicPacienti_Pacienti_PacientiId",
                        column: x => x.PacientiId,
                        principalTable: "Pacienti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Ratinguri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Scor = table.Column<int>(type: "int", nullable: false),
                    Comentariu = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Data = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Moderat = table.Column<bool>(type: "bit", nullable: false),
                    Vizibil = table.Column<bool>(type: "bit", nullable: false),
                    PacientId = table.Column<int>(type: "int", nullable: false),
                    MedicId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ratinguri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ratinguri_Medici_MedicId",
                        column: x => x.MedicId,
                        principalTable: "Medici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ratinguri_Pacienti_PacientId",
                        column: x => x.PacientId,
                        principalTable: "Pacienti",
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
                    MotivAnulare = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NotificareTrimisa = table.Column<bool>(type: "bit", nullable: false),
                    CanalNotificare = table.Column<int>(type: "int", nullable: true),
                    DataCreare = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PacientId = table.Column<int>(type: "int", nullable: false),
                    MedicId = table.Column<int>(type: "int", nullable: false),
                    AsistentId = table.Column<int>(type: "int", nullable: true),
                    ResursaId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Programari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Programari_Asistenti_AsistentId",
                        column: x => x.AsistentId,
                        principalTable: "Asistenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Programari_Medici_MedicId",
                        column: x => x.MedicId,
                        principalTable: "Medici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Programari_Pacienti_PacientId",
                        column: x => x.PacientId,
                        principalTable: "Pacienti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Programari_Resurse_ResursaId",
                        column: x => x.ResursaId,
                        principalTable: "Resurse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
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
                    MedicId = table.Column<int>(type: "int", nullable: false),
                    ProgramareId = table.Column<int>(type: "int", nullable: true)
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
                        name: "FK_Consultatii_Medici_MedicId",
                        column: x => x.MedicId,
                        principalTable: "Medici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Consultatii_Programari_ProgramareId",
                        column: x => x.ProgramareId,
                        principalTable: "Programari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Autentificari_UtilizatorId",
                table: "Autentificari",
                column: "UtilizatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Consultatii_FisaMedicalaId",
                table: "Consultatii",
                column: "FisaMedicalaId");

            migrationBuilder.CreateIndex(
                name: "IX_Consultatii_MedicId",
                table: "Consultatii",
                column: "MedicId");

            migrationBuilder.CreateIndex(
                name: "IX_Consultatii_ProgramareId",
                table: "Consultatii",
                column: "ProgramareId",
                unique: true,
                filter: "[ProgramareId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DocumenteMedicale_MedicId",
                table: "DocumenteMedicale",
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
                name: "IX_MedicAsistenti_MediciId",
                table: "MedicAsistenti",
                column: "MediciId");

            migrationBuilder.CreateIndex(
                name: "IX_Medici_CodParafa",
                table: "Medici",
                column: "CodParafa",
                unique: true,
                filter: "[CodParafa] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MedicPacienti_PacientiId",
                table: "MedicPacienti",
                column: "PacientiId");

            migrationBuilder.CreateIndex(
                name: "IX_Pacienti_CNP",
                table: "Pacienti",
                column: "CNP",
                unique: true,
                filter: "[CNP] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Programari_AsistentId",
                table: "Programari",
                column: "AsistentId");

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
                name: "IX_Ratinguri_MedicId",
                table: "Ratinguri",
                column: "MedicId");

            migrationBuilder.CreateIndex(
                name: "IX_Ratinguri_PacientId_MedicId",
                table: "Ratinguri",
                columns: new[] { "PacientId", "MedicId" },
                unique: true);

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
                name: "IX_Utilizatori_Email",
                table: "Utilizatori",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Autentificari");

            migrationBuilder.DropTable(
                name: "Consultatii");

            migrationBuilder.DropTable(
                name: "DocumenteMedicale");

            migrationBuilder.DropTable(
                name: "MedicAsistenti");

            migrationBuilder.DropTable(
                name: "MedicPacienti");

            migrationBuilder.DropTable(
                name: "Ratinguri");

            migrationBuilder.DropTable(
                name: "FiseMedicale");

            migrationBuilder.DropTable(
                name: "Programari");

            migrationBuilder.DropTable(
                name: "Asistenti");

            migrationBuilder.DropTable(
                name: "Medici");

            migrationBuilder.DropTable(
                name: "Pacienti");

            migrationBuilder.DropTable(
                name: "Resurse");

            migrationBuilder.DropTable(
                name: "Administratori");

            migrationBuilder.DropTable(
                name: "Utilizatori");
        }
    }
}
