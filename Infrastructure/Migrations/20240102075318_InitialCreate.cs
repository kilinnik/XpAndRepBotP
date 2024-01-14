#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "Index_ChatId",
                table: "Chats",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_DeletableMessages_ChatId",
                table: "DeletableMessages",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "Index_Level_CurrentExperience",
                table: "Users",
                columns: new[] { "Level", "CurrentExperience" });

            migrationBuilder.CreateIndex(
                name: "Index_Reputation",
                table: "Users",
                column: "Reputation");

            migrationBuilder.CreateIndex(
                name: "Index_UserId_ChatId",
                table: "Users",
                columns: new[] { "UserId", "ChatId" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_ChatId",
                table: "Users",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "Index_UserWord_UserId_ChatId",
                table: "UserWords",
                columns: new[] { "UserId", "ChatId" });

            migrationBuilder.CreateIndex(
                name: "Index_UserWord_Word",
                table: "UserWords",
                column: "Word");

            migrationBuilder.CreateIndex(
                name: "IX_UserWords_ChatId",
                table: "UserWords",
                column: "ChatId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "Index_Level_CurrentExperience",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "Index_Reputation",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "Index_UserId_ChatId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "Index_UserWord_UserId_ChatId",
                table: "UserWords");

            migrationBuilder.DropIndex(
                name: "Index_UserWord_Word",
                table: "UserWords");
        }
    }
}
