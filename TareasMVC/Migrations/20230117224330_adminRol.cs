using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TareasMVC.Migrations
{
    /// <inheritdoc />
    public partial class adminRol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT ID FROM AspNetRoles WHERE ID = '65a70ca6-f6ae-49bb-aea5-0cae1afff950')
                                BEGIN 
	                                INSERT AspNetRoles (Id, [Name], [NormalizedName])
	                                VALUES('65a70ca6-f6ae-49bb-aea5-0cae1afff950', 'admin', 'ADMIN')
                                END");
        }
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE AspNetRoles WHERE ID = '65a70ca6-f6ae-49bb-aea5-0cae1afff950'");
        }
    }
}
