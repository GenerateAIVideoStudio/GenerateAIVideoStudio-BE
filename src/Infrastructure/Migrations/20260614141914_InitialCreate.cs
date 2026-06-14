using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hook_templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    category = table.Column<string>(type: "text", nullable: true),
                    hook_text = table.Column<string>(type: "text", nullable: false),
                    language = table.Column<string>(type: "text", nullable: false),
                    performance_score = table.Column<decimal>(type: "numeric", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hook_templates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "model_images",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    object_key = table.Column<string>(type: "text", nullable: false),
                    preview_url = table.Column<string>(type: "text", nullable: true),
                    gender = table.Column<string>(type: "text", nullable: true),
                    style = table.Column<string>(type: "text", nullable: true),
                    ethnicity = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_model_images", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "video_jobs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_url = table.Column<string>(type: "text", nullable: true),
                    product_info = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    video_type = table.Column<string>(type: "text", nullable: true),
                    target_audience = table.Column<string>(type: "text", nullable: true),
                    customer_email = table.Column<string>(type: "text", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    flow_type = table.Column<string>(type: "text", nullable: false),
                    script = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    output_url = table.Column<string>(type: "text", nullable: true),
                    output_format = table.Column<string>(type: "text", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_video_jobs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "api_costs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service = table.Column<string>(type: "text", nullable: false),
                    action = table.Column<string>(type: "text", nullable: true),
                    cost_usd = table.Column<decimal>(type: "numeric(10,6)", precision: 10, scale: 6, nullable: false),
                    metadata = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_api_costs", x => x.id);
                    table.ForeignKey(
                        name: "fk_api_costs_video_jobs_job_id",
                        column: x => x.job_id,
                        principalTable: "video_jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "job_variations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_id = table.Column<Guid>(type: "uuid", nullable: false),
                    variation_index = table.Column<int>(type: "integer", nullable: false),
                    hook_text = table.Column<string>(type: "text", nullable: true),
                    voice_id = table.Column<string>(type: "text", nullable: true),
                    audio_url = table.Column<string>(type: "text", nullable: true),
                    avatar_id = table.Column<string>(type: "text", nullable: true),
                    avatar_video_url = table.Column<string>(type: "text", nullable: true),
                    model_image_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tryon_images = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    kling_task_id = table.Column<string>(type: "text", nullable: true),
                    product_video_url = table.Column<string>(type: "text", nullable: true),
                    output_format = table.Column<string>(type: "text", nullable: false),
                    final_url = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_job_variations", x => x.id);
                    table.ForeignKey(
                        name: "fk_job_variations_model_images_model_image_id",
                        column: x => x.model_image_id,
                        principalTable: "model_images",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_job_variations_video_jobs_job_id",
                        column: x => x.job_id,
                        principalTable: "video_jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_api_costs_created_at",
                table: "api_costs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_api_costs_job_id",
                table: "api_costs",
                column: "job_id");

            migrationBuilder.CreateIndex(
                name: "ix_api_costs_service",
                table: "api_costs",
                column: "service");

            migrationBuilder.CreateIndex(
                name: "ix_hook_templates_category",
                table: "hook_templates",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "ix_hook_templates_language",
                table: "hook_templates",
                column: "language");

            migrationBuilder.CreateIndex(
                name: "ix_job_variations_job_id",
                table: "job_variations",
                column: "job_id");

            migrationBuilder.CreateIndex(
                name: "ix_job_variations_model_image_id",
                table: "job_variations",
                column: "model_image_id");

            migrationBuilder.CreateIndex(
                name: "ix_model_images_gender",
                table: "model_images",
                column: "gender");

            migrationBuilder.CreateIndex(
                name: "ix_model_images_is_active",
                table: "model_images",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_video_jobs_created_at",
                table: "video_jobs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_video_jobs_flow_type",
                table: "video_jobs",
                column: "flow_type");

            migrationBuilder.CreateIndex(
                name: "ix_video_jobs_status",
                table: "video_jobs",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "api_costs");

            migrationBuilder.DropTable(
                name: "hook_templates");

            migrationBuilder.DropTable(
                name: "job_variations");

            migrationBuilder.DropTable(
                name: "model_images");

            migrationBuilder.DropTable(
                name: "video_jobs");
        }
    }
}
