﻿using System;

using Microsoft.EntityFrameworkCore.Migrations;

namespace Crash.Server.Migrations
{
	/// <inheritdoc />
	public partial class Init : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "Changes",
				columns: table => new
				{
					UniqueId = table.Column<Guid>(type: "TEXT", nullable: false),
					Stamp = table.Column<DateTime>(type: "TEXT", nullable: false),
					Id = table.Column<Guid>(type: "TEXT", nullable: false),
					Owner = table.Column<string>(type: "TEXT", nullable: true),
					Payload = table.Column<string>(type: "TEXT", rowVersion: true, nullable: true),
					Type = table.Column<string>(type: "TEXT", nullable: false),
					Action = table.Column<int>(type: "INTEGER", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Changes", x => x.UniqueId);
				});

			migrationBuilder.CreateTable(
				name: "LatestChanges",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "TEXT", nullable: false),
					Stamp = table.Column<DateTime>(type: "TEXT", nullable: false),
					Owner = table.Column<string>(type: "TEXT", nullable: true),
					Payload = table.Column<string>(type: "TEXT", nullable: true),
					Type = table.Column<string>(type: "TEXT", nullable: false),
					Action = table.Column<int>(type: "INTEGER", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_LatestChanges", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "ManageableUsers",
				columns: table => new
				{
					Title = table.Column<string>(type: "TEXT", nullable: false),
					EmailPattern = table.Column<string>(type: "TEXT", nullable: false),
					Status = table.Column<int>(type: "INTEGER", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_ManageableUsers", x => x.Title);
				});

			migrationBuilder.CreateTable(
				name: "Users",
				columns: table => new
				{
					Name = table.Column<string>(type: "TEXT", nullable: false),
					Id = table.Column<string>(type: "TEXT", nullable: true),
					Follows = table.Column<string>(type: "TEXT", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Users", x => x.Name);
				});
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "Changes");

			migrationBuilder.DropTable(
				name: "LatestChanges");

			migrationBuilder.DropTable(
				name: "ManageableUsers");

			migrationBuilder.DropTable(
				name: "Users");
		}
	}
}
