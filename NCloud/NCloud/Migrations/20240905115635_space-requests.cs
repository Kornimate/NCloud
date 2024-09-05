using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NCloud.Migrations
{
    /// <inheritdoc />
    public partial class spacerequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CloudSpaceRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SpaceRequest = table.Column<long>(type: "INTEGER", nullable: false),
                    RequestJustification = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CloudSpaceRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CloudSpaceRequests_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CloudSpaceRequests_UserId",
                table: "CloudSpaceRequests",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CloudSpaceRequests");
        }
    }
}
