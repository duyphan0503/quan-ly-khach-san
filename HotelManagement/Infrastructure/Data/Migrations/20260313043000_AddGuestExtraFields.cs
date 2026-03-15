using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGuestExtraFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // No-op: các cột đã được tạo sẵn trong InitialCreate để DB mới đầy đủ ngay từ đầu.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op.
        }
    }
}
