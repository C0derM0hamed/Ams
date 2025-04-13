using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmsApi.Migrations
{
    /// <inheritdoc />
    public partial class AddInstructorIdsToSubject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InstructorIds",
                table: "Subjects",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InstructorIds",
                table: "Subjects");
        }
    }
}
