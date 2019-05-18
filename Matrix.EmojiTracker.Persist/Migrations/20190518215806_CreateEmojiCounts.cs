using Microsoft.EntityFrameworkCore.Migrations;

namespace Matrix.EmojiTracker.Persist.Migrations
{
    public partial class CreateEmojiCounts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "emoji_count",
                columns: table => new
                {
                    emoji = table.Column<string>(nullable: false),
                    source = table.Column<string>(nullable: false),
                    count = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_emoji_count", x => new { x.emoji, x.source });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "emoji_count");
        }
    }
}
