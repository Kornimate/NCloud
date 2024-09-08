using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NCloud.Migrations
{
    /// <inheritdoc />
    public partial class dateaddedtorequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RequestDate",
                table: "CloudSpaceRequests",
                type: "TEXT",
                nullable: false,
                defaultValue: DateTime.UtcNow);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestDate",
                table: "CloudSpaceRequests");
        }
    }
}
