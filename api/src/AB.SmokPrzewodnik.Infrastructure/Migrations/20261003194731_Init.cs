using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace AB.SmokPrzewodnik.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "accessibility_profiles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    settings = table.Column<string>(type: "jsonb", nullable: false),
                    journey_history_sync_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    server_version = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accessibility_profiles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cities",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    name = table.Column<string>(type: "jsonb", nullable: false),
                    default_locale = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    time_zone = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cities", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "data_sources",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    display_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    source_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    website = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    license = table.Column<string>(type: "jsonb", nullable: false),
                    default_confidence_weight = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_data_sources", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "itineraries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    localized_title = table.Column<string>(type: "jsonb", nullable: false),
                    starts_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ends_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    time_zone = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    route_leg_references = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_itineraries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "spatial_entities",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    city_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    geometry = table.Column<Geometry>(type: "geometry(Geometry,4326)", nullable: false),
                    lifecycle_state = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    confidence_evaluated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    confidence_evidence_count = table.Column<int>(type: "integer", nullable: false),
                    confidence_score = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    confidence_state = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_spatial_entities", x => x.id);
                    table.ForeignKey(
                        name: "FK_spatial_entities_cities_city_id",
                        column: x => x.city_id,
                        principalTable: "cities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "feed_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    city_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    localized_content = table.Column<string>(type: "jsonb", nullable: false),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    valid_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    audience_tags = table.Column<string>(type: "jsonb", nullable: false),
                    provenance = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_feed_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_feed_items_cities_city_id",
                        column: x => x.city_id,
                        principalTable: "cities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_feed_items_data_sources_source_id",
                        column: x => x.source_id,
                        principalTable: "data_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "source_assertions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    assertion_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    retrieved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    valid_from = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    valid_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    transform_version = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_source_assertions", x => x.id);
                    table.ForeignKey(
                        name: "FK_source_assertions_data_sources_source_id",
                        column: x => x.source_id,
                        principalTable: "data_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "entity_source_links",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    retrieved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    license = table.Column<string>(type: "jsonb", nullable: false),
                    transform_version = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entity_source_links", x => x.id);
                    table.ForeignKey(
                        name: "FK_entity_source_links_data_sources_source_id",
                        column: x => x.source_id,
                        principalTable: "data_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_entity_source_links_spatial_entities_entity_id",
                        column: x => x.entity_id,
                        principalTable: "spatial_entities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "entity_translations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    locale = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entity_translations", x => x.id);
                    table.ForeignKey(
                        name: "FK_entity_translations_spatial_entities_entity_id",
                        column: x => x.entity_id,
                        principalTable: "spatial_entities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "itinerary_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    planned_arrival = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    planned_departure = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    note = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    itinerary_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_itinerary_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_itinerary_items_itineraries_itinerary_id",
                        column: x => x.itinerary_id,
                        principalTable: "itineraries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_itinerary_items_spatial_entities_entity_id",
                        column: x => x.entity_id,
                        principalTable: "spatial_entities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "spatial_entity_details",
                columns: table => new
                {
                    spatial_entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    detail_type = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
                    event_organizer_entity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    event_starts_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    event_ends_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    event_booking_uri = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    event_capacity = table.Column<long>(type: "bigint", nullable: true),
                    infrastructure_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    infrastructure_operational_state = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    infrastructure_maintenance_reference = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    obstacle_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    obstacle_severity = table.Column<long>(type: "bigint", nullable: true),
                    obstacle_expected_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    obstacle_affected_travel_modes = table.Column<string>(type: "jsonb", nullable: true),
                    place_category_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    place_opening_hours = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    place_contact = table.Column<string>(type: "jsonb", nullable: true),
                    place_website = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_spatial_entity_details", x => x.spatial_entity_id);
                    table.ForeignKey(
                        name: "FK_spatial_entity_details_spatial_entities_event_organizer_ent~",
                        column: x => x.event_organizer_entity_id,
                        principalTable: "spatial_entities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_spatial_entity_details_spatial_entities_spatial_entity_id",
                        column: x => x.spatial_entity_id,
                        principalTable: "spatial_entities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "suggestion_projections",
                columns: table => new
                {
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    suggested_entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reason_codes = table.Column<string>(type: "jsonb", nullable: false),
                    score = table.Column<decimal>(type: "numeric(8,6)", precision: 8, scale: 6, nullable: false),
                    generated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_suggestion_projections", x => new { x.account_id, x.suggested_entity_id });
                    table.ForeignKey(
                        name: "FK_suggestion_projections_spatial_entities_suggested_entity_id",
                        column: x => x.suggested_entity_id,
                        principalTable: "spatial_entities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "feed_item_entities",
                columns: table => new
                {
                    feed_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_feed_item_entities", x => new { x.feed_item_id, x.entity_id });
                    table.ForeignKey(
                        name: "FK_feed_item_entities_feed_items_feed_item_id",
                        column: x => x.feed_item_id,
                        principalTable: "feed_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_feed_item_entities_spatial_entities_entity_id",
                        column: x => x.entity_id,
                        principalTable: "spatial_entities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "accessibility_facts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    spatial_entity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    graph_routing_provider = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    graph_version = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    graph_element_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    graph_external_element_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    attribute_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    value_kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    boolean_value = table.Column<bool>(type: "boolean", nullable: true),
                    number_value = table.Column<decimal>(type: "numeric", nullable: true),
                    number_unit_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    code_value = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    text_value = table.Column<string>(type: "text", nullable: true),
                    evidence_kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    source_assertion_id = table.Column<Guid>(type: "uuid", nullable: true),
                    observation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    observed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    valid_from = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    valid_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    confidence_weight = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accessibility_facts", x => x.id);
                    table.ForeignKey(
                        name: "FK_accessibility_facts_source_assertions_source_assertion_id",
                        column: x => x.source_assertion_id,
                        principalTable: "source_assertions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_accessibility_facts_spatial_entities_spatial_entity_id",
                        column: x => x.spatial_entity_id,
                        principalTable: "spatial_entities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "event_categories",
                columns: table => new
                {
                    category_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    spatial_entity_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event_categories", x => new { x.spatial_entity_id, x.category_code });
                    table.ForeignKey(
                        name: "FK_event_categories_spatial_entity_details_spatial_entity_id",
                        column: x => x.spatial_entity_id,
                        principalTable: "spatial_entity_details",
                        principalColumn: "spatial_entity_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_accessibility_facts_source_assertion_id",
                table: "accessibility_facts",
                column: "source_assertion_id");

            migrationBuilder.CreateIndex(
                name: "IX_accessibility_facts_spatial_entity_id_attribute_code",
                table: "accessibility_facts",
                columns: new[] { "spatial_entity_id", "attribute_code" });

            migrationBuilder.CreateIndex(
                name: "IX_accessibility_profiles_account_id",
                table: "accessibility_profiles",
                column: "account_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cities_code",
                table: "cities",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_data_sources_provider_code",
                table: "data_sources",
                column: "provider_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_entity_source_links_entity_id",
                table: "entity_source_links",
                column: "entity_id");

            migrationBuilder.CreateIndex(
                name: "IX_entity_source_links_source_id_external_id",
                table: "entity_source_links",
                columns: new[] { "source_id", "external_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_entity_translations_entity_id_locale",
                table: "entity_translations",
                columns: new[] { "entity_id", "locale" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_feed_item_entities_entity_id",
                table: "feed_item_entities",
                column: "entity_id");

            migrationBuilder.CreateIndex(
                name: "IX_feed_items_city_id_published_at",
                table: "feed_items",
                columns: new[] { "city_id", "published_at" });

            migrationBuilder.CreateIndex(
                name: "IX_feed_items_source_id",
                table: "feed_items",
                column: "source_id");

            migrationBuilder.CreateIndex(
                name: "IX_itineraries_account_id_starts_at",
                table: "itineraries",
                columns: new[] { "account_id", "starts_at" });

            migrationBuilder.CreateIndex(
                name: "IX_itinerary_items_entity_id",
                table: "itinerary_items",
                column: "entity_id");

            migrationBuilder.CreateIndex(
                name: "IX_itinerary_items_itinerary_id_position",
                table: "itinerary_items",
                columns: new[] { "itinerary_id", "position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_source_assertions_source_id_external_id",
                table: "source_assertions",
                columns: new[] { "source_id", "external_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_spatial_entities_city_id_kind",
                table: "spatial_entities",
                columns: new[] { "city_id", "kind" });

            migrationBuilder.CreateIndex(
                name: "IX_spatial_entities_geometry",
                table: "spatial_entities",
                column: "geometry")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "IX_spatial_entities_updated_at_id",
                table: "spatial_entities",
                columns: new[] { "updated_at", "id" });

            migrationBuilder.CreateIndex(
                name: "IX_spatial_entity_details_event_organizer_entity_id",
                table: "spatial_entity_details",
                column: "event_organizer_entity_id");

            migrationBuilder.CreateIndex(
                name: "IX_spatial_entity_details_event_starts_at_event_ends_at",
                table: "spatial_entity_details",
                columns: new[] { "event_starts_at", "event_ends_at" });

            migrationBuilder.CreateIndex(
                name: "IX_suggestion_projections_account_id_score",
                table: "suggestion_projections",
                columns: new[] { "account_id", "score" });

            migrationBuilder.CreateIndex(
                name: "IX_suggestion_projections_suggested_entity_id",
                table: "suggestion_projections",
                column: "suggested_entity_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "accessibility_facts");

            migrationBuilder.DropTable(
                name: "accessibility_profiles");

            migrationBuilder.DropTable(
                name: "entity_source_links");

            migrationBuilder.DropTable(
                name: "entity_translations");

            migrationBuilder.DropTable(
                name: "event_categories");

            migrationBuilder.DropTable(
                name: "feed_item_entities");

            migrationBuilder.DropTable(
                name: "itinerary_items");

            migrationBuilder.DropTable(
                name: "suggestion_projections");

            migrationBuilder.DropTable(
                name: "source_assertions");

            migrationBuilder.DropTable(
                name: "spatial_entity_details");

            migrationBuilder.DropTable(
                name: "feed_items");

            migrationBuilder.DropTable(
                name: "itineraries");

            migrationBuilder.DropTable(
                name: "spatial_entities");

            migrationBuilder.DropTable(
                name: "data_sources");

            migrationBuilder.DropTable(
                name: "cities");
        }
    }
}
