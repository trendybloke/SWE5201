using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPI.Migrations
{
    public partial class editedbooking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventBooking_AspNetUsers_UserId",
                table: "EventBooking");

            migrationBuilder.CreateTable(
                name: "UserSummaryViewModel",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Firstname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Lastname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSummaryViewModel", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_EventBooking_UserSummaryViewModel_UserId",
                table: "EventBooking",
                column: "UserId",
                principalTable: "UserSummaryViewModel",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventBooking_UserSummaryViewModel_UserId",
                table: "EventBooking");

            migrationBuilder.DropTable(
                name: "UserSummaryViewModel");

            migrationBuilder.AddForeignKey(
                name: "FK_EventBooking_AspNetUsers_UserId",
                table: "EventBooking",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
