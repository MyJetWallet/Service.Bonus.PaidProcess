using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service.BonusRewards.Postgres.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "bonusrewards");

            migrationBuilder.CreateTable(
                name: "rewards",
                schema: "bonusrewards",
                columns: table => new
                {
                    ClientId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RewardId = table.Column<string>(type: "text", nullable: false),
                    CampaignId = table.Column<string>(type: "text", nullable: true),
                    RewardType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    FeeShareGroup = table.Column<string>(type: "text", nullable: true),
                    ReferrerClientId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Asset = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    AmountAbs = table.Column<decimal>(type: "numeric", nullable: false),
                    AmountRel = table.Column<decimal>(type: "numeric", nullable: false),
                    Percentage = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rewards", x => new { x.ClientId, x.RewardId });
                });

            migrationBuilder.CreateIndex(
                name: "IX_rewards_CampaignId",
                schema: "bonusrewards",
                table: "rewards",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_rewards_ClientId",
                schema: "bonusrewards",
                table: "rewards",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_rewards_RewardId",
                schema: "bonusrewards",
                table: "rewards",
                column: "RewardId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rewards",
                schema: "bonusrewards");
        }
    }
}
