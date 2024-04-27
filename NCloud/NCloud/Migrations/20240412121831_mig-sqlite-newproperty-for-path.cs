using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NCloud.Migrations
{
    public partial class migsqlitenewpropertyforpath : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PathFromRoot",
                table: "SharedFolders",
                newName: "SharedPathFromRoot");

            migrationBuilder.RenameColumn(
                name: "PathFromRoot",
                table: "SharedFiles",
                newName: "SharedPathFromRoot");

            migrationBuilder.AddColumn<string>(
                name: "CloudPathFromRoot",
                table: "SharedFolders",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CloudPathFromRoot",
                table: "SharedFiles",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CloudPathFromRoot",
                table: "SharedFolders");

            migrationBuilder.DropColumn(
                name: "CloudPathFromRoot",
                table: "SharedFiles");

            migrationBuilder.RenameColumn(
                name: "SharedPathFromRoot",
                table: "SharedFolders",
                newName: "PathFromRoot");

            migrationBuilder.RenameColumn(
                name: "SharedPathFromRoot",
                table: "SharedFiles",
                newName: "PathFromRoot");
        }
    }
}
