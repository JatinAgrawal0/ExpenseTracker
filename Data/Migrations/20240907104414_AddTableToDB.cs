﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseTrackerApp.Data.Migrations
{
    public partial class AddTableToDB : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JatinAgrawalCategories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(5)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(10)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JatinAgrawalCategories", x => x.CategoryId);
                    table.ForeignKey(
                        name: "FK_JatinAgrawalCategories_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JatinAgrawalTransactions",
                columns: table => new
                {
                    TransactionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(75)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JatinAgrawalTransactions", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_JatinAgrawalTransactions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JatinAgrawalTransactions_JatinAgrawalCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "JatinAgrawalCategories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JatinAgrawalCategories_UserId",
                table: "JatinAgrawalCategories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_JatinAgrawalTransactions_CategoryId",
                table: "JatinAgrawalTransactions",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_JatinAgrawalTransactions_UserId",
                table: "JatinAgrawalTransactions",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JatinAgrawalTransactions");

            migrationBuilder.DropTable(
                name: "JatinAgrawalCategories");
        }
    }
}
