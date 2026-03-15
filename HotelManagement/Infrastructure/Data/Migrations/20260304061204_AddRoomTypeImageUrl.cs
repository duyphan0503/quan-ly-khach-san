using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomTypeImageUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // No-op: cột ImageUrl đã được tạo sẵn trong InitialCreate để DB mới đầy đủ ngay từ đầu.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op.
        }
    }
}
