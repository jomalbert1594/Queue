using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace QueueDataAccess.Migrations
{
    public partial class DateTimeOrderedAddedinTransaction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateTimeOrdered",
                table: "Transactions",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateTimeOrdered",
                table: "Transactions");
        }
    }
}
