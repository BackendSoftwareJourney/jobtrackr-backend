using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobTrackr.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskDueDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DueDateUtc",
                table: "Tasks",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DueDateUtc",
                table: "Tasks");
        }
    }
}
