using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chat.Migrations
{
    /// <inheritdoc />
    public partial class EmailVerifiedTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmailVerifiedToken_Users_UserId",
                table: "EmailVerifiedToken");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmailVerifiedToken",
                table: "EmailVerifiedToken");

            migrationBuilder.RenameTable(
                name: "EmailVerifiedToken",
                newName: "EmailVerifiedTokens");

            migrationBuilder.RenameIndex(
                name: "IX_EmailVerifiedToken_UserId",
                table: "EmailVerifiedTokens",
                newName: "IX_EmailVerifiedTokens_UserId");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "EmailVerifiedTokens",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmailVerifiedTokens",
                table: "EmailVerifiedTokens",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailVerifiedTokens_Users_UserId",
                table: "EmailVerifiedTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmailVerifiedTokens_Users_UserId",
                table: "EmailVerifiedTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmailVerifiedTokens",
                table: "EmailVerifiedTokens");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "EmailVerifiedTokens");

            migrationBuilder.RenameTable(
                name: "EmailVerifiedTokens",
                newName: "EmailVerifiedToken");

            migrationBuilder.RenameIndex(
                name: "IX_EmailVerifiedTokens_UserId",
                table: "EmailVerifiedToken",
                newName: "IX_EmailVerifiedToken_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmailVerifiedToken",
                table: "EmailVerifiedToken",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailVerifiedToken_Users_UserId",
                table: "EmailVerifiedToken",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
