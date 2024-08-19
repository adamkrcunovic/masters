using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlightSearch.Migrations
{
    /// <inheritdoc />
    public partial class Migration13 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "18a042c3-75f2-4379-b958-cf17f6595536");

            migrationBuilder.AddColumn<double>(
                name: "CurrentPrice",
                table: "Itinenaries",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "d70b2699-e7d7-4003-b661-da5b901dbdf6", null, "User", "USER" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d70b2699-e7d7-4003-b661-da5b901dbdf6");

            migrationBuilder.DropColumn(
                name: "CurrentPrice",
                table: "Itinenaries");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "18a042c3-75f2-4379-b958-cf17f6595536", null, "User", "USER" });
        }
    }
}
