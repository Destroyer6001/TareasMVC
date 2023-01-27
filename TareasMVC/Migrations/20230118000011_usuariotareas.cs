using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TareasMVC.Migrations
{
    /// <inheritdoc />
    public partial class usuariotareas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Titulo",
                table: "Tareas",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.AddColumn<string>(
                name: "UsarioCreacionId",
                table: "Tareas",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsuarioCreacionId",
                table: "Tareas",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Tareas_UsarioCreacionId",
                table: "Tareas",
                column: "UsarioCreacionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tareas_AspNetUsers_UsarioCreacionId",
                table: "Tareas",
                column: "UsarioCreacionId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tareas_AspNetUsers_UsarioCreacionId",
                table: "Tareas");

            migrationBuilder.DropIndex(
                name: "IX_Tareas_UsarioCreacionId",
                table: "Tareas");

            migrationBuilder.DropColumn(
                name: "UsarioCreacionId",
                table: "Tareas");

            migrationBuilder.DropColumn(
                name: "UsuarioCreacionId",
                table: "Tareas");

            migrationBuilder.AlterColumn<string>(
                name: "Titulo",
                table: "Tareas",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
