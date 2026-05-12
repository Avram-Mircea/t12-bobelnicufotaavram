using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicaMedicala.Migrations
{
    /// <inheritdoc />
    public partial class AddActivPeResursa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Activ",
                table: "Resurse",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Activ",
                table: "Resurse");
        }
    }
}
