using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPI.Migrations
{
    public partial class initialcreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Event",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Event", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Room",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Room", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tag",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tag", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HostedEvent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    _EventId = table.Column<int>(type: "int", nullable: true),
                    EventId = table.Column<int>(type: "int", nullable: true),
                    RoomId = table.Column<int>(type: "int", nullable: true),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    DurationHours = table.Column<int>(type: "int", nullable: false),
                    DurationDays = table.Column<int>(type: "int", nullable: false),
                    EntranceFee = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HostedEvent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HostedEvent_Event_EventId",
                        column: x => x.EventId,
                        principalTable: "Event",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HostedEvent_Room_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Room",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EventTag",
                columns: table => new
                {
                    EventsId = table.Column<int>(type: "int", nullable: false),
                    TagsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTag", x => new { x.EventsId, x.TagsId });
                    table.ForeignKey(
                        name: "FK_EventTag_Event_EventsId",
                        column: x => x.EventsId,
                        principalTable: "Event",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventTag_Tag_TagsId",
                        column: x => x.TagsId,
                        principalTable: "Tag",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Event",
                columns: new[] { "Id", "Description", "Title" },
                values: new object[,]
                {
                    { 1, "Come and participate in a night of laughs, or just come along for the free pizza and drinks!", "Open Mic Comedy Night" },
                    { 2, "We're running back to back to back comedy movies all night long! Free popcorn and drinks.", "Comedy Movie Marathon" },
                    { 3, "Up for a scare? How about hours worth of spine chilling entertainment? Then come on over, the slushies are freel, but bring your own snacks!", "Horror Movie Marathon" }
                });

            migrationBuilder.InsertData(
                table: "Room",
                columns: new[] { "Id", "Capacity", "Name" },
                values: new object[,]
                {
                    { 1, 150, "F2-08" },
                    { 2, 50, "M1-11" },
                    { 3, 76, "M1-12" },
                    { 4, 76, "M1-14" },
                    { 5, 50, "M1-16" },
                    { 6, 50, "M2-11" }
                });

            migrationBuilder.InsertData(
                table: "Tag",
                columns: new[] { "Id", "Content" },
                values: new object[,]
                {
                    { 1, "Comedy" },
                    { 2, "Movie" },
                    { 3, "Live" },
                    { 4, "Horror" }
                });

            migrationBuilder.InsertData(
                table: "EventTag",
                columns: new[] { "EventsId", "TagsId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 1, 3 },
                    { 2, 1 },
                    { 2, 2 },
                    { 3, 2 },
                    { 3, 4 }
                });

            migrationBuilder.InsertData(
                table: "HostedEvent",
                columns: new[] { "Id", "DurationDays", "DurationHours", "DurationMinutes", "EntranceFee", "EventId", "RoomId", "StartTime", "_EventId" },
                values: new object[,]
                {
                    { 1, 1, 3, 0, 2f, null, 2, new DateTime(2022, 9, 12, 18, 30, 0, 0, DateTimeKind.Unspecified), 1 },
                    { 2, 2, 4, 0, 1.5f, null, 1, new DateTime(2022, 9, 12, 16, 0, 0, 0, DateTimeKind.Unspecified), 2 },
                    { 3, 1, 4, 30, 0f, null, 2, new DateTime(2022, 9, 12, 21, 45, 0, 0, DateTimeKind.Unspecified), 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventTag_TagsId",
                table: "EventTag",
                column: "TagsId");

            migrationBuilder.CreateIndex(
                name: "IX_HostedEvent_EventId",
                table: "HostedEvent",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_HostedEvent_RoomId",
                table: "HostedEvent",
                column: "RoomId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventTag");

            migrationBuilder.DropTable(
                name: "HostedEvent");

            migrationBuilder.DropTable(
                name: "Tag");

            migrationBuilder.DropTable(
                name: "Event");

            migrationBuilder.DropTable(
                name: "Room");
        }
    }
}
