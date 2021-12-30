using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service.BonusRewards.Postgres.Migrations
{
    public partial class IndexPrices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientWalletId",
                schema: "bonusrewards",
                table: "rewards",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "IndexPrice",
                schema: "bonusrewards",
                table: "rewards",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientWalletId",
                schema: "bonusrewards",
                table: "rewards");

            migrationBuilder.DropColumn(
                name: "IndexPrice",
                schema: "bonusrewards",
                table: "rewards");
        }
    }
}
