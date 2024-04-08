using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NCloud.Migrations
{
    public partial class newnamessqlite : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PublicShared",
                table: "SharedFolders",
                newName: "ConnectedToWeb");

            migrationBuilder.RenameColumn(
                name: "InnerShared",
                table: "SharedFolders",
                newName: "ConnectedToApp");

            migrationBuilder.RenameColumn(
                name: "PublicShared",
                table: "SharedFiles",
                newName: "ConnectedToWeb");

            migrationBuilder.RenameColumn(
                name: "InnerShared",
                table: "SharedFiles",
                newName: "ConnectedToApp");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ConnectedToWeb",
                table: "SharedFolders",
                newName: "PublicShared");

            migrationBuilder.RenameColumn(
                name: "ConnectedToApp",
                table: "SharedFolders",
                newName: "InnerShared");

            migrationBuilder.RenameColumn(
                name: "ConnectedToWeb",
                table: "SharedFiles",
                newName: "PublicShared");

            migrationBuilder.RenameColumn(
                name: "ConnectedToApp",
                table: "SharedFiles",
                newName: "InnerShared");
        }
    }
}
