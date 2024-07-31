using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlightSearch.Migrations
{
    /// <inheritdoc />
    public partial class Migration9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "06f83e1c-4a7b-4ff2-bdd5-cdbd90889662");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Itinenaries",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "d7d863d0-b597-449e-90c8-c2d6c37b9da6", null, "User", "USER" });

            migrationBuilder.CreateIndex(
                name: "IX_Itinenaries_UserId",
                table: "Itinenaries",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Itinenaries_AspNetUsers_UserId",
                table: "Itinenaries",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Itinenaries_AspNetUsers_UserId",
                table: "Itinenaries");

            migrationBuilder.DropIndex(
                name: "IX_Itinenaries_UserId",
                table: "Itinenaries");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d7d863d0-b597-449e-90c8-c2d6c37b9da6");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Itinenaries");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "06f83e1c-4a7b-4ff2-bdd5-cdbd90889662", null, "User", "USER" });
        }
    }
}
