using Microsoft.EntityFrameworkCore.Migrations;

namespace QueueDataAccess.Migrations
{
    public partial class UpdatedEntitesForNewQueueSys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CounterTypeNo",
                table: "CounterTypes");

            migrationBuilder.RenameColumn(
                name: "CounterTypeNo",
                table: "Devices",
                newName: "CouterTypeId");

            migrationBuilder.AddColumn<bool>(
                name: "IsEndpoint",
                table: "CounterTypes",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CounterName",
                table: "Counters",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEndpoint",
                table: "CounterTypes");

            migrationBuilder.DropColumn(
                name: "CounterName",
                table: "Counters");

            migrationBuilder.RenameColumn(
                name: "CouterTypeId",
                table: "Devices",
                newName: "CounterTypeNo");

            migrationBuilder.AddColumn<int>(
                name: "CounterTypeNo",
                table: "CounterTypes",
                nullable: false,
                defaultValue: 0);
        }
    }
}
