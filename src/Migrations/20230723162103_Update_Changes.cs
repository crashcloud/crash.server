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

            migrationBuilder.AddPrimaryKey(
                name: "PK_Changes",
                table: "Changes",
                column: "UniqueId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Changes",
                table: "Changes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Changes",
                table: "Changes",
                column: "Id");
        }
    }
}
