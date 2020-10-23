using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace QueueDataAccess.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CounterTypes",
                columns: table => new
                {
                    CounterTypeId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CounterName = table.Column<string>(nullable: true),
                    CounterTypeNo = table.Column<int>(nullable: false),
                    CounterShortName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CounterTypes", x => x.CounterTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    DeviceId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DeviceSerialNo = table.Column<string>(nullable: true),
                    IsDesktop = table.Column<bool>(nullable: false),
                    ConnectionSerial = table.Column<string>(nullable: true),
                    CounterTypeNo = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.DeviceId);
                });

            migrationBuilder.CreateTable(
                name: "Counters",
                columns: table => new
                {
                    CounterId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    COunterNo = table.Column<int>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    RowVersion2 = table.Column<byte[]>(type: "Timestamp", nullable: false),
                    CounterTypeId = table.Column<int>(nullable: true),
                    TransactionId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Counters", x => x.CounterId);
                    table.ForeignKey(
                        name: "FK_Counter_CounterTypeId",
                        column: x => x.CounterTypeId,
                        principalTable: "CounterTypes",
                        principalColumn: "CounterTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransControls",
                columns: table => new
                {
                    TransControlId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsSpecial = table.Column<bool>(nullable: true),
                    CounterTypeId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransControls", x => x.TransControlId);
                    table.ForeignKey(
                        name: "FK_TransControls_CounterTypes_CounterTypeId",
                        column: x => x.CounterTypeId,
                        principalTable: "CounterTypes",
                        principalColumn: "CounterTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransPools",
                columns: table => new
                {
                    TransPoolId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TransactionDate = table.Column<DateTime>(nullable: false),
                    First = table.Column<int>(nullable: false),
                    Last = table.Column<int>(nullable: false),
                    IsSpecial = table.Column<bool>(nullable: false),
                    TransControlId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransPools", x => x.TransPoolId);
                    table.ForeignKey(
                        name: "FK_TransPools_TransControls_TransControlId",
                        column: x => x.TransControlId,
                        principalTable: "TransControls",
                        principalColumn: "TransControlId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    TransactionId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PrioNo = table.Column<int>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    IsServed = table.Column<bool>(nullable: false),
                    RowVersion2 = table.Column<byte[]>(type: "Timestamp", nullable: false),
                    PrevId = table.Column<int>(nullable: true),
                    NextId = table.Column<int>(nullable: true),
                    TransPoolId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_Transaction_NextId",
                        column: x => x.NextId,
                        principalTable: "Transactions",
                        principalColumn: "TransactionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transaction_PrevId",
                        column: x => x.PrevId,
                        principalTable: "Transactions",
                        principalColumn: "TransactionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transactions_TransPools_TransPoolId",
                        column: x => x.TransPoolId,
                        principalTable: "TransPools",
                        principalColumn: "TransPoolId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Counters_CounterTypeId",
                table: "Counters",
                column: "CounterTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_NextId",
                table: "Transactions",
                column: "NextId",
                unique: true,
                filter: "[NextId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_PrevId",
                table: "Transactions",
                column: "PrevId",
                unique: true,
                filter: "[PrevId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TransPoolId",
                table: "Transactions",
                column: "TransPoolId");

            migrationBuilder.CreateIndex(
                name: "IX_TransControls_CounterTypeId",
                table: "TransControls",
                column: "CounterTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TransPools_TransControlId",
                table: "TransPools",
                column: "TransControlId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Counters");

            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "TransPools");

            migrationBuilder.DropTable(
                name: "TransControls");

            migrationBuilder.DropTable(
                name: "CounterTypes");
        }
    }
}
