using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicaMedicala.Migrations
{
    /// <inheritdoc />
    public partial class AddAcordatDeMedicPeRating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Ratinguri_PacientId_MedicId",
                table: "Ratinguri");

            migrationBuilder.AddColumn<bool>(
                name: "AcordatDeMedic",
                table: "Ratinguri",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Ratinguri_PacientId_MedicId_AcordatDeMedic",
                table: "Ratinguri",
                columns: new[] { "PacientId", "MedicId", "AcordatDeMedic" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Ratinguri_PacientId_MedicId_AcordatDeMedic",
                table: "Ratinguri");

            migrationBuilder.DropColumn(
                name: "AcordatDeMedic",
                table: "Ratinguri");

            migrationBuilder.CreateIndex(
                name: "IX_Ratinguri_PacientId_MedicId",
                table: "Ratinguri",
                columns: new[] { "PacientId", "MedicId" },
                unique: true);
        }
    }
}
