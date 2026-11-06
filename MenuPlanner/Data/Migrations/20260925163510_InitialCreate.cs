using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MenuPlanner.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Mains",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    RecipeUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    SideCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mains", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sides",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sides", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlanDays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    MainId = table.Column<int>(type: "INTEGER", nullable: true),
                    Locked = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanDays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanDays_Mains_MainId",
                        column: x => x.MainId,
                        principalTable: "Mains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MainSide",
                columns: table => new
                {
                    MainsId = table.Column<int>(type: "INTEGER", nullable: false),
                    SidesId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MainSide", x => new { x.MainsId, x.SidesId });
                    table.ForeignKey(
                        name: "FK_MainSide_Mains_MainsId",
                        column: x => x.MainsId,
                        principalTable: "Mains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MainSide_Sides_SidesId",
                        column: x => x.SidesId,
                        principalTable: "Sides",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanDaySide",
                columns: table => new
                {
                    PlanDaysId = table.Column<int>(type: "INTEGER", nullable: false),
                    SidesId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanDaySide", x => new { x.PlanDaysId, x.SidesId });
                    table.ForeignKey(
                        name: "FK_PlanDaySide_PlanDays_PlanDaysId",
                        column: x => x.PlanDaysId,
                        principalTable: "PlanDays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlanDaySide_Sides_SidesId",
                        column: x => x.SidesId,
                        principalTable: "Sides",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Mains",
                columns: new[] { "Id", "Name", "Notes", "RecipeUrl", "SideCount" },
                values: new object[,]
                {
                    { 1, "Chicken stir-fry", null, null, 1 },
                    { 2, "Meatloaf", null, null, 2 },
                    { 3, "Spaghetti and meatballs", null, null, 2 },
                    { 4, "Tacos", null, null, 1 },
                    { 5, "Grilled burgers", null, null, 2 },
                    { 6, "Lasagna", null, null, 0 },
                    { 7, "Baked salmon", null, null, 2 }
                });

            migrationBuilder.InsertData(
                table: "Sides",
                columns: new[] { "Id", "Name", "Notes" },
                values: new object[,]
                {
                    { 1, "Rice", null },
                    { 2, "Roasted broccoli", null },
                    { 3, "Mashed potatoes", null },
                    { 4, "Green salad", null },
                    { 5, "Garlic bread", null },
                    { 6, "Corn on the cob", null },
                    { 7, "Chips and salsa", null }
                });

            migrationBuilder.InsertData(
                table: "MainSide",
                columns: new[] { "MainsId", "SidesId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 1, 2 },
                    { 2, 2 },
                    { 2, 3 },
                    { 2, 6 },
                    { 3, 4 },
                    { 3, 5 },
                    { 4, 1 },
                    { 4, 7 },
                    { 5, 4 },
                    { 5, 6 },
                    { 5, 7 },
                    { 6, 4 },
                    { 6, 5 },
                    { 7, 1 },
                    { 7, 2 },
                    { 7, 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MainSide_SidesId",
                table: "MainSide",
                column: "SidesId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanDays_Date",
                table: "PlanDays",
                column: "Date",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanDays_MainId",
                table: "PlanDays",
                column: "MainId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanDaySide_SidesId",
                table: "PlanDaySide",
                column: "SidesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MainSide");

            migrationBuilder.DropTable(
                name: "PlanDaySide");

            migrationBuilder.DropTable(
                name: "PlanDays");

            migrationBuilder.DropTable(
                name: "Sides");

            migrationBuilder.DropTable(
                name: "Mains");
        }
    }
}
