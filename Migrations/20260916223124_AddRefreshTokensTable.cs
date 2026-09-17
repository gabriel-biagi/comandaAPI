using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace comandaAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokensTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokenEntity_AspNetUsers_UserId",
                table: "RefreshTokenEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RefreshTokenEntity",
                table: "RefreshTokenEntity");

            migrationBuilder.RenameTable(
                name: "RefreshTokenEntity",
                newName: "RefreshTokens");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshTokenEntity_UserId",
                table: "RefreshTokens",
                newName: "IX_RefreshTokens_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RefreshTokens",
                table: "RefreshTokens",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_AspNetUsers_UserId",
                table: "RefreshTokens",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_AspNetUsers_UserId",
                table: "RefreshTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RefreshTokens",
                table: "RefreshTokens");

            migrationBuilder.RenameTable(
                name: "RefreshTokens",
                newName: "RefreshTokenEntity");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokenEntity",
                newName: "IX_RefreshTokenEntity_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RefreshTokenEntity",
                table: "RefreshTokenEntity",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokenEntity_AspNetUsers_UserId",
                table: "RefreshTokenEntity",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
