using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kawwer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSportTypeAndNotificationActions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Important",
                table: "notifications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "RelatedFriendshipId",
                table: "notifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "notifications",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Sport",
                table: "matches",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Important",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "RelatedFriendshipId",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "Sport",
                table: "matches");
        }
    }
}
