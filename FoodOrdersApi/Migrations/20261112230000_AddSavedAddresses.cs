using FoodOrdersApi.Entities;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodOrdersApi.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261112230000_AddSavedAddresses")]
    public partial class AddSavedAddresses : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Label",
                table: "Addresses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Home");

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "Addresses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "Addresses",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_AccountId",
                table: "Addresses",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_Accounts_AccountId",
                table: "Addresses",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_Accounts_AccountId",
                table: "Addresses");

            migrationBuilder.DropIndex(
                name: "IX_Addresses_AccountId",
                table: "Addresses");

            migrationBuilder.DropColumn(name: "AccountId", table: "Addresses");
            migrationBuilder.DropColumn(name: "IsDefault", table: "Addresses");
            migrationBuilder.DropColumn(name: "Label", table: "Addresses");
        }
    }
}
