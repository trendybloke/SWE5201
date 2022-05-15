using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPI.Migrations
{
    public partial class UpdatedEventBooking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventBooking_UserSummaryViewModel_UserId",
                table: "EventBooking");

            migrationBuilder.DropTable(
                name: "UserSummaryViewModel");

            migrationBuilder.DropIndex(
                name: "IX_EventBooking_UserId",
                table: "EventBooking");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "EventBooking",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "EventBooking",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "UserSummaryViewModel",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Firstname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Lastname = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSummaryViewModel", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventBooking_UserId",
                table: "EventBooking",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventBooking_UserSummaryViewModel_UserId",
                table: "EventBooking",
                column: "UserId",
                principalTable: "UserSummaryViewModel",
                principalColumn: "Id");
        }
    }
}
