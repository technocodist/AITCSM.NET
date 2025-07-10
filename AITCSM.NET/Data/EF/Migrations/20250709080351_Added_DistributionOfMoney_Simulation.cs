using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AITCSM.NET.Data.EF.Migrations
{
    /// <inheritdoc />
    public partial class Added_DistributionOfMoney_Simulation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DistributionOfMoney",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NumberOfAgents = table.Column<int>(type: "INTEGER", nullable: false),
                    InitialMoney = table.Column<double>(type: "REAL", nullable: false),
                    NumberOfIterations = table.Column<int>(type: "INTEGER", nullable: false),
                    InitialRandomSeed = table.Column<int>(type: "INTEGER", nullable: false),
                    ResultPerSteps = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DistributionOfMoney", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DistributionOfMoneyStepResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DistributionOfMoneyId = table.Column<int>(type: "INTEGER", nullable: false),
                    StepNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    MoneyDistributionData = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DistributionOfMoneyStepResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DistributionOfMoneyStepResults_DistributionOfMoney_DistributionOfMoneyId",
                        column: x => x.DistributionOfMoneyId,
                        principalTable: "DistributionOfMoney",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DistributionOfMoneyStepResults_DistributionOfMoneyId",
                table: "DistributionOfMoneyStepResults",
                column: "DistributionOfMoneyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DistributionOfMoneyStepResults");

            migrationBuilder.DropTable(
                name: "DistributionOfMoney");
        }
    }
}