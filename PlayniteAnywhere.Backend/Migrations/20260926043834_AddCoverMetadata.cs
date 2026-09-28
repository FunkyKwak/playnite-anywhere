using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlayniteAnywhere.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddCoverMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CoverLastWriteTimeUtcTicks",
                table: "Games",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CoverSize",
                table: "Games",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoverLastWriteTimeUtcTicks",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "CoverSize",
                table: "Games");
        }
    }
}
