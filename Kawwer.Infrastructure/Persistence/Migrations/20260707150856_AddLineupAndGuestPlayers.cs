using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kawwer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLineupAndGuestPlayers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "OrganizerPositionX",
                table: "matches",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "OrganizerPositionY",
                table: "matches",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrganizerTeam",
                table: "matches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "PositionX",
                table: "match_participants",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PositionY",
                table: "match_participants",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Team",
                table: "match_participants",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "match_guest_players",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    AddedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SkillLevel = table.Column<int>(type: "integer", nullable: true),
                    Team = table.Column<int>(type: "integer", nullable: false),
                    PositionX = table.Column<double>(type: "double precision", nullable: true),
                    PositionY = table.Column<double>(type: "double precision", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_match_guest_players", x => x.Id);
                    table.ForeignKey(
                        name: "FK_match_guest_players_matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_match_guest_players_MatchId",
                table: "match_guest_players",
                column: "MatchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "match_guest_players");

            migrationBuilder.DropColumn(
                name: "OrganizerPositionX",
                table: "matches");

            migrationBuilder.DropColumn(
                name: "OrganizerPositionY",
                table: "matches");

            migrationBuilder.DropColumn(
                name: "OrganizerTeam",
                table: "matches");

            migrationBuilder.DropColumn(
                name: "PositionX",
                table: "match_participants");

            migrationBuilder.DropColumn(
                name: "PositionY",
                table: "match_participants");

            migrationBuilder.DropColumn(
                name: "Team",
                table: "match_participants");
        }
    }
}
