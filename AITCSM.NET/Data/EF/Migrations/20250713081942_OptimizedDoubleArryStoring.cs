using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AITCSM.NET.Data.EF.Migrations
{
    /// <inheritdoc />
    public partial class OptimizedDoubleArryStoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MoneyDistributionData",
                table: "DistributionOfMoneyStepResults");

            migrationBuilder.AddColumn<byte[]>(
                name: "MoneyDistributionBytes",
                table: "DistributionOfMoneyStepResults",
                type: "BLOB",
                nullable: false,
                defaultValue: Array.Empty<byte>());
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MoneyDistributionBytes",
                table: "DistributionOfMoneyStepResults");

            migrationBuilder.AddColumn<string>(
                name: "MoneyDistributionData",
                table: "DistributionOfMoneyStepResults",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}