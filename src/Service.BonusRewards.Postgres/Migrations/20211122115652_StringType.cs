using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service.BonusRewards.Postgres.Migrations
{
    public partial class StringType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RewardType",
                schema: "bonusrewards",
                table: "rewards",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "RewardType",
                schema: "bonusrewards",
                table: "rewards",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
