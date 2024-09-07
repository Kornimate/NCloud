using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NCloud.Migrations
{
    /// <inheritdoc />
    public partial class changespacerequestlimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CloudSpaceRequests_AspNetUsers_UserId",
                table: "CloudSpaceRequests");

            migrationBuilder.DropIndex(
                name: "IX_CloudSpaceRequests_UserId",
                table: "CloudSpaceRequests");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "CloudSpaceRequests");

            migrationBuilder.AddColumn<Guid>(
                name: "CloudSpaceRequestId",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CloudSpaceRequestId",
                table: "AspNetUsers",
                column: "CloudSpaceRequestId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_CloudSpaceRequests_CloudSpaceRequestId",
                table: "AspNetUsers",
                column: "CloudSpaceRequestId",
                principalTable: "CloudSpaceRequests",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_CloudSpaceRequests_CloudSpaceRequestId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_CloudSpaceRequestId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CloudSpaceRequestId",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "CloudSpaceRequests",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_CloudSpaceRequests_UserId",
                table: "CloudSpaceRequests",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CloudSpaceRequests_AspNetUsers_UserId",
                table: "CloudSpaceRequests",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
