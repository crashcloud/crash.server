using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crash.Server.Migrations
{
    /// <inheritdoc />
    public partial class Update_Changes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Changes",
                table: "Changes");

            migrationBuilder.AddColumn<string>(
                name: "Follows",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Changes",
                table: "Changes",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Changes",
                table: "Changes");

            migrationBuilder.DropColumn(
                name: "Follows",
                table: "Users");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Changes",
                table: "Changes",
                column: "UniqueId");
        }
    }
}
