using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicaMedicala.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexResursaDenumire : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Resurse_Denumire",
                table: "Resurse",
                column: "Denumire",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Resurse_Denumire",
                table: "Resurse");
        }
    }
}
