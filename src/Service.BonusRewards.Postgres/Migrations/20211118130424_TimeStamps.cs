using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service.BonusRewards.Postgres.Migrations
{
    public partial class TimeStamps : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TimeStamp",
                schema: "bonusrewards",
                table: "rewards",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeStamp",
                schema: "bonusrewards",
                table: "rewards");
        }
    }
}
