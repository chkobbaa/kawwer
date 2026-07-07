using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kawwer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameGroupsToTeamsAndAddMatchOpponent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ----- Rename "groups" -> "teams" in place, preserving every row -----
            // The default EF scaffold dropped and recreated these tables (losing data);
            // renaming keeps all existing teams and memberships intact.
            migrationBuilder.RenameTable(name: "groups", newName: "teams");
            migrationBuilder.RenameTable(name: "group_members", newName: "team_members");

            migrationBuilder.RenameColumn(name: "GroupId", table: "team_members", newName: "TeamId");

            migrationBuilder.RenameIndex(
                name: "IX_groups_OwnerId",
                table: "teams",
                newName: "IX_teams_OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_group_members_GroupId_UserId",
                table: "team_members",
                newName: "IX_team_members_TeamId_UserId");

            // Renaming a table in PostgreSQL leaves its constraint names untouched, so rename
            // the primary/foreign keys too — otherwise the schema keeps "group" names forever.
            migrationBuilder.Sql("ALTER TABLE teams RENAME CONSTRAINT \"PK_groups\" TO \"PK_teams\";");
            migrationBuilder.Sql("ALTER TABLE team_members RENAME CONSTRAINT \"PK_group_members\" TO \"PK_team_members\";");
            migrationBuilder.Sql("ALTER TABLE team_members RENAME CONSTRAINT \"FK_group_members_groups_GroupId\" TO \"FK_team_members_teams_TeamId\";");

            // ----- New match opponent columns -----
            // Existing rows are pickup matches, so backfill Format with Pickup (= 1).
            migrationBuilder.AddColumn<int>(
                name: "Format",
                table: "matches",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "OpponentName",
                table: "matches",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OpponentTeamId",
                table: "matches",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Format", table: "matches");
            migrationBuilder.DropColumn(name: "OpponentName", table: "matches");
            migrationBuilder.DropColumn(name: "OpponentTeamId", table: "matches");

            migrationBuilder.Sql("ALTER TABLE team_members RENAME CONSTRAINT \"FK_team_members_teams_TeamId\" TO \"FK_group_members_groups_GroupId\";");
            migrationBuilder.Sql("ALTER TABLE team_members RENAME CONSTRAINT \"PK_team_members\" TO \"PK_group_members\";");
            migrationBuilder.Sql("ALTER TABLE teams RENAME CONSTRAINT \"PK_teams\" TO \"PK_groups\";");

            migrationBuilder.RenameIndex(
                name: "IX_team_members_TeamId_UserId",
                table: "team_members",
                newName: "IX_group_members_GroupId_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_teams_OwnerId",
                table: "teams",
                newName: "IX_groups_OwnerId");

            migrationBuilder.RenameColumn(name: "TeamId", table: "team_members", newName: "GroupId");

            migrationBuilder.RenameTable(name: "team_members", newName: "group_members");
            migrationBuilder.RenameTable(name: "teams", newName: "groups");
        }
    }
}
