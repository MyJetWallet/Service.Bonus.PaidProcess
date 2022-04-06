using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service.BonusRewards.Postgres.Migrations
{
    public partial class Referrals : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReferralClientId",
                schema: "bonusrewards",
                table: "rewards",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReferralClientId",
                schema: "bonusrewards",
                table: "rewards");
        }
    }
}
