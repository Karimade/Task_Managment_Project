using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Task_Management_system.Migrations
{
    /// <inheritdoc />
    public partial class userSubTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SubTaskUser",
                columns: table => new
                {
                    SubTasksSubTaskId = table.Column<int>(type: "int", nullable: false),
                    UsersUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubTaskUser", x => new { x.SubTasksSubTaskId, x.UsersUserId });
                    table.ForeignKey(
                        name: "FK_SubTaskUser_subTasks_SubTasksSubTaskId",
                        column: x => x.SubTasksSubTaskId,
                        principalTable: "subTasks",
                        principalColumn: "SubTaskId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubTaskUser_users_UsersUserId",
                        column: x => x.UsersUserId,
                        principalTable: "users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubTaskUser_UsersUserId",
                table: "SubTaskUser",
                column: "UsersUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubTaskUser");
        }
    }
}
