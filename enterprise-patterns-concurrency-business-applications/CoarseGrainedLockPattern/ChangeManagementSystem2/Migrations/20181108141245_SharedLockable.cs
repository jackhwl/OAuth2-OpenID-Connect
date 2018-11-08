using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace ChangeManagementSystem.Migrations
{
    public partial class SharedLockable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SharedVersionId",
                table: "ChangeRequestTasks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ChangeRequests",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SharedVersionId",
                table: "ChangeRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Versions",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Versions", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChangeRequestTasks_SharedVersionId",
                table: "ChangeRequestTasks",
                column: "SharedVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeRequests_SharedVersionId",
                table: "ChangeRequests",
                column: "SharedVersionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChangeRequests_Versions_SharedVersionId",
                table: "ChangeRequests",
                column: "SharedVersionId",
                principalTable: "Versions",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChangeRequestTasks_Versions_SharedVersionId",
                table: "ChangeRequestTasks",
                column: "SharedVersionId",
                principalTable: "Versions",
                principalColumn: "ID",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChangeRequests_Versions_SharedVersionId",
                table: "ChangeRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ChangeRequestTasks_Versions_SharedVersionId",
                table: "ChangeRequestTasks");

            migrationBuilder.DropTable(
                name: "Versions");

            migrationBuilder.DropIndex(
                name: "IX_ChangeRequestTasks_SharedVersionId",
                table: "ChangeRequestTasks");

            migrationBuilder.DropIndex(
                name: "IX_ChangeRequests_SharedVersionId",
                table: "ChangeRequests");

            migrationBuilder.DropColumn(
                name: "SharedVersionId",
                table: "ChangeRequestTasks");

            migrationBuilder.DropColumn(
                name: "SharedVersionId",
                table: "ChangeRequests");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ChangeRequests",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }
    }
}
