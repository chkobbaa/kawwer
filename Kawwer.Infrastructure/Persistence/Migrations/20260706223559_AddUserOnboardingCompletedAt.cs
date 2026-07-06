using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kawwer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserOnboardingCompletedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "OnboardingCompletedAt",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            // Grandfather existing accounts: they already have profiles, so treat them as onboarded
            // (stamped with their join date) instead of forcing them back through the first-run flow.
            // New accounts are created with a null value and therefore see onboarding.
            migrationBuilder.Sql(
                "UPDATE \"users\" SET \"OnboardingCompletedAt\" = \"CreatedAt\" WHERE \"OnboardingCompletedAt\" IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OnboardingCompletedAt",
                table: "users");
        }
    }
}
