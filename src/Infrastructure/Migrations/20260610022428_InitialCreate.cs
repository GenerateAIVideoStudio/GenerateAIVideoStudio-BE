using System;
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
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    avatar_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "projects",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_projects", x => x.id);
                    table.ForeignKey(
                        name: "fk_projects_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ai_generations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    project_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    entity_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    entity_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    provider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    prompt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    response = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    token_input = table.Column<int>(type: "int", nullable: true),
                    token_output = table.Column<int>(type: "int", nullable: true),
                    cost_usd = table.Column<decimal>(type: "numeric(10,6)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    error_message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    started_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    completed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ai_generations", x => x.id);
                    table.ForeignKey(
                        name: "fk_ai_generations_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "assets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    project_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    asset_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    file_name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    object_key = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    file_size_bytes = table.Column<long>(type: "bigint", nullable: true),
                    mime_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_assets", x => x.id);
                    table.ForeignKey(
                        name: "fk_assets_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    project_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    prompt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stories", x => x.id);
                    table.ForeignKey(
                        name: "fk_stories_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "characters",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    project_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    age = table.Column<int>(type: "int", nullable: true),
                    gender = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    face_asset_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_characters", x => x.id);
                    table.ForeignKey(
                        name: "fk_characters_assets_face_asset_id",
                        column: x => x.face_asset_id,
                        principalTable: "assets",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_characters_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "locations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    project_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    style_json = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    image_asset_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_locations", x => x.id);
                    table.ForeignKey(
                        name: "fk_locations_assets_image_asset_id",
                        column: x => x.image_asset_id,
                        principalTable: "assets",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_locations_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "final_videos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    story_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    video_asset_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    subtitle_asset_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    error_message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_final_videos", x => x.id);
                    table.ForeignKey(
                        name: "fk_final_videos_assets_subtitle_asset_id",
                        column: x => x.subtitle_asset_id,
                        principalTable: "assets",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_final_videos_assets_video_asset_id",
                        column: x => x.video_asset_id,
                        principalTable: "assets",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_final_videos_stories_story_id",
                        column: x => x.story_id,
                        principalTable: "stories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "character_appearances",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    character_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    hair_color = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    hair_style = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    skin_tone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    eye_color = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    height = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    body_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    typical_outfit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    accessories = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    extra_notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_character_appearances", x => x.id);
                    table.ForeignKey(
                        name: "fk_character_appearances_characters_character_id",
                        column: x => x.character_id,
                        principalTable: "characters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "character_bibles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    character_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    bible_json = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    version = table.Column<int>(type: "int", nullable: false),
                    generated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_character_bibles", x => x.id);
                    table.ForeignKey(
                        name: "fk_character_bibles_characters_character_id",
                        column: x => x.character_id,
                        principalTable: "characters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "character_memories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    character_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    related_character_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    memory_scope = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    memory_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    story_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    scene_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    memory_json = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_character_memories", x => x.id);
                    table.ForeignKey(
                        name: "fk_character_memories_characters_character_id",
                        column: x => x.character_id,
                        principalTable: "characters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_character_memories_characters_related_character_id",
                        column: x => x.related_character_id,
                        principalTable: "characters",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "character_personalities",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    character_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    traits = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    speaking_style = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    background = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    goals = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    quirks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_character_personalities", x => x.id);
                    table.ForeignKey(
                        name: "fk_character_personalities_characters_character_id",
                        column: x => x.character_id,
                        principalTable: "characters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "character_relationships",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    character_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    target_character_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    relationship_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    since_story_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_character_relationships", x => x.id);
                    table.ForeignKey(
                        name: "fk_character_relationships_characters_character_id",
                        column: x => x.character_id,
                        principalTable: "characters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_character_relationships_characters_target_character_id",
                        column: x => x.target_character_id,
                        principalTable: "characters",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "character_voices",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    character_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    provider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    voice_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    voice_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    language = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    sample_audio_asset_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_character_voices", x => x.id);
                    table.ForeignKey(
                        name: "fk_character_voices_assets_sample_audio_asset_id",
                        column: x => x.sample_audio_asset_id,
                        principalTable: "assets",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_character_voices_characters_character_id",
                        column: x => x.character_id,
                        principalTable: "characters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "scenes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    story_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    location_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    scene_number = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    dialogue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    mood = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    duration_sec = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_scenes", x => x.id);
                    table.ForeignKey(
                        name: "fk_scenes_locations_location_id",
                        column: x => x.location_id,
                        principalTable: "locations",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_scenes_stories_story_id",
                        column: x => x.story_id,
                        principalTable: "stories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "scene_characters",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    scene_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    character_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    role = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_scene_characters", x => x.id);
                    table.ForeignKey(
                        name: "fk_scene_characters_characters_character_id",
                        column: x => x.character_id,
                        principalTable: "characters",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_scene_characters_scenes_scene_id",
                        column: x => x.scene_id,
                        principalTable: "scenes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "scene_voices",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    scene_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    character_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    dialogue_text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    audio_asset_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    job_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    duration_sec = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    error_message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_scene_voices", x => x.id);
                    table.ForeignKey(
                        name: "fk_scene_voices_assets_audio_asset_id",
                        column: x => x.audio_asset_id,
                        principalTable: "assets",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_scene_voices_characters_character_id",
                        column: x => x.character_id,
                        principalTable: "characters",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_scene_voices_scenes_scene_id",
                        column: x => x.scene_id,
                        principalTable: "scenes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "storyboards",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    scene_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    prompt_used = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    image_asset_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    job_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    error_message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_storyboards", x => x.id);
                    table.ForeignKey(
                        name: "fk_storyboards_assets_image_asset_id",
                        column: x => x.image_asset_id,
                        principalTable: "assets",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_storyboards_scenes_scene_id",
                        column: x => x.scene_id,
                        principalTable: "scenes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "scene_videos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    scene_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    storyboard_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    prompt_used = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    video_asset_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    job_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    duration_sec = table.Column<int>(type: "int", nullable: true),
                    error_message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_scene_videos", x => x.id);
                    table.ForeignKey(
                        name: "fk_scene_videos_assets_video_asset_id",
                        column: x => x.video_asset_id,
                        principalTable: "assets",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_scene_videos_scenes_scene_id",
                        column: x => x.scene_id,
                        principalTable: "scenes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_scene_videos_storyboards_storyboard_id",
                        column: x => x.storyboard_id,
                        principalTable: "storyboards",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_ai_generations_entity_type_entity_id",
                table: "ai_generations",
                columns: new[] { "entity_type", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "ix_ai_generations_project_id",
                table: "ai_generations",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "ix_ai_generations_provider",
                table: "ai_generations",
                column: "provider");

            migrationBuilder.CreateIndex(
                name: "ix_assets_asset_type",
                table: "assets",
                column: "asset_type");

            migrationBuilder.CreateIndex(
                name: "ix_assets_object_key",
                table: "assets",
                column: "object_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_assets_project_id",
                table: "assets",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "ix_character_appearances_character_id",
                table: "character_appearances",
                column: "character_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_character_bibles_character_id",
                table: "character_bibles",
                column: "character_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_character_memories_character_id",
                table: "character_memories",
                column: "character_id");

            migrationBuilder.CreateIndex(
                name: "ix_character_memories_character_id_memory_scope",
                table: "character_memories",
                columns: new[] { "character_id", "memory_scope" });

            migrationBuilder.CreateIndex(
                name: "ix_character_memories_character_id_related_character_id",
                table: "character_memories",
                columns: new[] { "character_id", "related_character_id" });

            migrationBuilder.CreateIndex(
                name: "ix_character_memories_related_character_id",
                table: "character_memories",
                column: "related_character_id");

            migrationBuilder.CreateIndex(
                name: "ix_character_personalities_character_id",
                table: "character_personalities",
                column: "character_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_character_relationships_character_id",
                table: "character_relationships",
                column: "character_id");

            migrationBuilder.CreateIndex(
                name: "ix_character_relationships_character_id_target_character_id",
                table: "character_relationships",
                columns: new[] { "character_id", "target_character_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_character_relationships_target_character_id",
                table: "character_relationships",
                column: "target_character_id");

            migrationBuilder.CreateIndex(
                name: "ix_character_voices_character_id",
                table: "character_voices",
                column: "character_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_character_voices_sample_audio_asset_id",
                table: "character_voices",
                column: "sample_audio_asset_id");

            migrationBuilder.CreateIndex(
                name: "ix_characters_face_asset_id",
                table: "characters",
                column: "face_asset_id");

            migrationBuilder.CreateIndex(
                name: "ix_characters_project_id",
                table: "characters",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "ix_final_videos_story_id",
                table: "final_videos",
                column: "story_id");

            migrationBuilder.CreateIndex(
                name: "ix_final_videos_subtitle_asset_id",
                table: "final_videos",
                column: "subtitle_asset_id");

            migrationBuilder.CreateIndex(
                name: "ix_final_videos_video_asset_id",
                table: "final_videos",
                column: "video_asset_id");

            migrationBuilder.CreateIndex(
                name: "ix_locations_image_asset_id",
                table: "locations",
                column: "image_asset_id");

            migrationBuilder.CreateIndex(
                name: "ix_locations_project_id",
                table: "locations",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "ix_projects_user_id",
                table: "projects",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_scene_characters_character_id",
                table: "scene_characters",
                column: "character_id");

            migrationBuilder.CreateIndex(
                name: "ix_scene_characters_scene_id",
                table: "scene_characters",
                column: "scene_id");

            migrationBuilder.CreateIndex(
                name: "ix_scene_characters_scene_id_character_id",
                table: "scene_characters",
                columns: new[] { "scene_id", "character_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_scene_videos_scene_id",
                table: "scene_videos",
                column: "scene_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_scene_videos_storyboard_id",
                table: "scene_videos",
                column: "storyboard_id");

            migrationBuilder.CreateIndex(
                name: "ix_scene_videos_video_asset_id",
                table: "scene_videos",
                column: "video_asset_id");

            migrationBuilder.CreateIndex(
                name: "ix_scene_voices_audio_asset_id",
                table: "scene_voices",
                column: "audio_asset_id");

            migrationBuilder.CreateIndex(
                name: "ix_scene_voices_character_id",
                table: "scene_voices",
                column: "character_id");

            migrationBuilder.CreateIndex(
                name: "ix_scene_voices_scene_id",
                table: "scene_voices",
                column: "scene_id");

            migrationBuilder.CreateIndex(
                name: "ix_scenes_location_id",
                table: "scenes",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "ix_scenes_story_id_scene_number",
                table: "scenes",
                columns: new[] { "story_id", "scene_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_stories_project_id",
                table: "stories",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "ix_storyboards_image_asset_id",
                table: "storyboards",
                column: "image_asset_id");

            migrationBuilder.CreateIndex(
                name: "ix_storyboards_scene_id",
                table: "storyboards",
                column: "scene_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ai_generations");

            migrationBuilder.DropTable(
                name: "character_appearances");

            migrationBuilder.DropTable(
                name: "character_bibles");

            migrationBuilder.DropTable(
                name: "character_memories");

            migrationBuilder.DropTable(
                name: "character_personalities");

            migrationBuilder.DropTable(
                name: "character_relationships");

            migrationBuilder.DropTable(
                name: "character_voices");

            migrationBuilder.DropTable(
                name: "final_videos");

            migrationBuilder.DropTable(
                name: "scene_characters");

            migrationBuilder.DropTable(
                name: "scene_videos");

            migrationBuilder.DropTable(
                name: "scene_voices");

            migrationBuilder.DropTable(
                name: "storyboards");

            migrationBuilder.DropTable(
                name: "characters");

            migrationBuilder.DropTable(
                name: "scenes");

            migrationBuilder.DropTable(
                name: "locations");

            migrationBuilder.DropTable(
                name: "stories");

            migrationBuilder.DropTable(
                name: "assets");

            migrationBuilder.DropTable(
                name: "projects");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
