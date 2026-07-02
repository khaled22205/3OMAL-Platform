using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AiConversation_GuestAndMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Make UserId nullable
            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "AiConversations",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            // Add SessionId
            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "AiConversations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            // Add UserRole
            migrationBuilder.AddColumn<string>(
                name: "UserRole",
                table: "AiConversations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Guest");

            // Add IsArchived
            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "AiConversations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Add IsHidden
            migrationBuilder.AddColumn<bool>(
                name: "IsHidden",
                table: "AiConversations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Add index on SessionId for fast guest session lookups
            migrationBuilder.CreateIndex(
                name: "IX_AiConversations_SessionId",
                table: "AiConversations",
                column: "SessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AiConversations_SessionId",
                table: "AiConversations");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "AiConversations");

            migrationBuilder.DropColumn(
                name: "UserRole",
                table: "AiConversations");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "AiConversations");

            migrationBuilder.DropColumn(
                name: "IsHidden",
                table: "AiConversations");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "AiConversations",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
