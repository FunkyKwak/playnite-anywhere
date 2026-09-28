using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlayniteAnywhere.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddWebPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Preferences",
                columns: table => new
                {
                    Id = table.Column<int>(
                        type: "integer",
                        nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
                                .NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),

                    GroupBy = table.Column<string>(
                        type: "text",
                        nullable: false),

                    CollapsedGroups = table.Column<string>(
                        type: "jsonb",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Preferences", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Preferences");
        }
    }
}
