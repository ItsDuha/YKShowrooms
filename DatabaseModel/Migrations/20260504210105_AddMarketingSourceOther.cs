using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatabaseModel.Migrations
{
    /// <inheritdoc />
    public partial class AddMarketingSourceOther : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MarketingSourceOther",
                table: "visitors",
                type: "nvarchar(max)",
                nullable: true);

          
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MarketingSourceOther",
                table: "visitors");


        }
    }
}
