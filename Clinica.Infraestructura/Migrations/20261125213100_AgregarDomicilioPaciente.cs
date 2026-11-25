using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinica.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDomicilioPaciente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Domicilio",
                table: "Pacientes",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Domicilio",
                table: "Pacientes");
        }
    }
}
