using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace Mermer.Data.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddStockSlips : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "currencies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    decimals = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_disabled = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_currencies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "offices",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    region = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    tags = table.Column<string[]>(type: "text[]", nullable: true),
                    is_disabled = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_offices", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "stocks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    short_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    group_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    tags = table.Column<string[]>(type: "text[]", nullable: true),
                    barcodes = table.Column<string[]>(type: "text[]", nullable: true),
                    limit_min = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    limit_max = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_disabled = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    search_vector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stocks", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    password = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_admin = table.Column<bool>(type: "boolean", nullable: false),
                    is_disabled = table.Column<bool>(type: "boolean", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "currency_rates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    currency_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valid_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    multiplier = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    divider = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_currency_rates", x => x.id);
                    table.ForeignKey(
                        name: "FK_currency_rates_currencies_currency_id",
                        column: x => x.currency_id,
                        principalTable: "currencies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "partners",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    phone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    address = table.Column<string>(type: "text", nullable: true),
                    group_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    credit_limit = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    tags = table.Column<string[]>(type: "text[]", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    rating = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    currency_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_disabled = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_partners", x => x.id);
                    table.ForeignKey(
                        name: "FK_partners_currencies_currency_id",
                        column: x => x.currency_id,
                        principalTable: "currencies",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "depositories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    office_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    tags = table.Column<string[]>(type: "text[]", nullable: true),
                    is_disabled = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_depositories", x => x.id);
                    table.ForeignKey(
                        name: "FK_depositories_offices_office_id",
                        column: x => x.office_id,
                        principalTable: "offices",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "warehouses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    office_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    tags = table.Column<string[]>(type: "text[]", nullable: true),
                    is_disabled = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_warehouses", x => x.id);
                    table.ForeignKey(
                        name: "FK_warehouses_offices_office_id",
                        column: x => x.office_id,
                        principalTable: "offices",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "stock_additional_prices",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    stock_id = table.Column<Guid>(type: "uuid", nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    currency_id = table.Column<Guid>(type: "uuid", nullable: true),
                    price_group = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    valid_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_additional_prices", x => x.id);
                    table.ForeignKey(
                        name: "FK_stock_additional_prices_currencies_currency_id",
                        column: x => x.currency_id,
                        principalTable: "currencies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_stock_additional_prices_stocks_stock_id",
                        column: x => x.stock_id,
                        principalTable: "stocks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stock_prices",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    stock_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valid_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    currency_id = table.Column<Guid>(type: "uuid", nullable: true),
                    price_group = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_prices", x => x.id);
                    table.ForeignKey(
                        name: "FK_stock_prices_currencies_currency_id",
                        column: x => x.currency_id,
                        principalTable: "currencies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_stock_prices_stocks_stock_id",
                        column: x => x.stock_id,
                        principalTable: "stocks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stock_units",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    stock_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    multiplier = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    divider = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_units", x => x.id);
                    table.ForeignKey(
                        name: "FK_stock_units_stocks_stock_id",
                        column: x => x.stock_id,
                        principalTable: "stocks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "invoices",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    due_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    invoice_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    office_id = table.Column<Guid>(type: "uuid", nullable: true),
                    warehouse_id = table.Column<Guid>(type: "uuid", nullable: true),
                    depository_id = table.Column<Guid>(type: "uuid", nullable: true),
                    partner_id = table.Column<Guid>(type: "uuid", nullable: true),
                    display_currency_id = table.Column<Guid>(type: "uuid", nullable: true),
                    stock_price_group = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    debit_credit_left_amount = table.Column<bool>(type: "boolean", nullable: false),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false),
                    is_disabled = table.Column<bool>(type: "boolean", nullable: false),
                    group_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    tags = table.Column<string[]>(type: "text[]", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoices", x => x.id);
                    table.ForeignKey(
                        name: "FK_invoices_currencies_display_currency_id",
                        column: x => x.display_currency_id,
                        principalTable: "currencies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_invoices_depositories_depository_id",
                        column: x => x.depository_id,
                        principalTable: "depositories",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_invoices_offices_office_id",
                        column: x => x.office_id,
                        principalTable: "offices",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_invoices_partners_partner_id",
                        column: x => x.partner_id,
                        principalTable: "partners",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_invoices_warehouses_warehouse_id",
                        column: x => x.warehouse_id,
                        principalTable: "warehouses",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "stock_balances",
                columns: table => new
                {
                    warehouse_id = table.Column<Guid>(type: "uuid", nullable: false),
                    stock_id = table.Column<Guid>(type: "uuid", nullable: false),
                    income = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    expense = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_balances", x => new { x.warehouse_id, x.stock_id });
                    table.ForeignKey(
                        name: "FK_stock_balances_stocks_stock_id",
                        column: x => x.stock_id,
                        principalTable: "stocks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_stock_balances_warehouses_warehouse_id",
                        column: x => x.warehouse_id,
                        principalTable: "warehouses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stock_slips",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    slip_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false),
                    is_stock_income = table.Column<bool>(type: "boolean", nullable: false),
                    display_total = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    tags = table.Column<string[]>(type: "text[]", nullable: true),
                    date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    warehouse_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_slips", x => x.id);
                    table.ForeignKey(
                        name: "FK_stock_slips_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_stock_slips_warehouses_warehouse_id",
                        column: x => x.warehouse_id,
                        principalTable: "warehouses",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "invoice_currency_convertions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    invoice_id = table.Column<Guid>(type: "uuid", nullable: false),
                    currency_id = table.Column<Guid>(type: "uuid", nullable: false),
                    multiplier = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    divider = table.Column<decimal>(type: "numeric(18,8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoice_currency_convertions", x => x.id);
                    table.ForeignKey(
                        name: "FK_invoice_currency_convertions_currencies_currency_id",
                        column: x => x.currency_id,
                        principalTable: "currencies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_invoice_currency_convertions_invoices_invoice_id",
                        column: x => x.invoice_id,
                        principalTable: "invoices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "invoice_discounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    invoice_id = table.Column<Guid>(type: "uuid", nullable: false),
                    discount_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoice_discounts", x => x.id);
                    table.ForeignKey(
                        name: "FK_invoice_discounts_invoices_invoice_id",
                        column: x => x.invoice_id,
                        principalTable: "invoices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "invoice_lines",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    invoice_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_id = table.Column<Guid>(type: "uuid", nullable: true),
                    stock_id = table.Column<Guid>(type: "uuid", nullable: true),
                    unit_id = table.Column<Guid>(type: "uuid", nullable: true),
                    quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    currency_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoice_lines", x => x.id);
                    table.ForeignKey(
                        name: "FK_invoice_lines_currencies_currency_id",
                        column: x => x.currency_id,
                        principalTable: "currencies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_invoice_lines_invoices_invoice_id",
                        column: x => x.invoice_id,
                        principalTable: "invoices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_invoice_lines_stock_units_unit_id",
                        column: x => x.unit_id,
                        principalTable: "stock_units",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_invoice_lines_stocks_stock_id",
                        column: x => x.stock_id,
                        principalTable: "stocks",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "invoice_overheads",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    invoice_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    currency_id = table.Column<Guid>(type: "uuid", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoice_overheads", x => x.id);
                    table.ForeignKey(
                        name: "FK_invoice_overheads_currencies_currency_id",
                        column: x => x.currency_id,
                        principalTable: "currencies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_invoice_overheads_invoices_invoice_id",
                        column: x => x.invoice_id,
                        principalTable: "invoices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "invoice_payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    invoice_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    currency_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoice_payments", x => x.id);
                    table.ForeignKey(
                        name: "FK_invoice_payments_currencies_currency_id",
                        column: x => x.currency_id,
                        principalTable: "currencies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_invoice_payments_invoices_invoice_id",
                        column: x => x.invoice_id,
                        principalTable: "invoices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "invoice_stock_unit_convertions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    invoice_id = table.Column<Guid>(type: "uuid", nullable: false),
                    stock_id = table.Column<Guid>(type: "uuid", nullable: false),
                    unit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    multiplier = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    divider = table.Column<decimal>(type: "numeric(18,8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoice_stock_unit_convertions", x => x.id);
                    table.ForeignKey(
                        name: "FK_invoice_stock_unit_convertions_invoices_invoice_id",
                        column: x => x.invoice_id,
                        principalTable: "invoices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_invoice_stock_unit_convertions_stock_units_unit_id",
                        column: x => x.unit_id,
                        principalTable: "stock_units",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_invoice_stock_unit_convertions_stocks_stock_id",
                        column: x => x.stock_id,
                        principalTable: "stocks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stock_slip_lines",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    stock_slip_id = table.Column<Guid>(type: "uuid", nullable: false),
                    stock_id = table.Column<Guid>(type: "uuid", nullable: true),
                    unit_id = table.Column<Guid>(type: "uuid", nullable: true),
                    quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    action_quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    action_total = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_slip_lines", x => x.id);
                    table.ForeignKey(
                        name: "FK_stock_slip_lines_stock_slips_stock_slip_id",
                        column: x => x.stock_slip_id,
                        principalTable: "stock_slips",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_stock_slip_lines_stock_units_unit_id",
                        column: x => x.unit_id,
                        principalTable: "stock_units",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_stock_slip_lines_stocks_stock_id",
                        column: x => x.stock_id,
                        principalTable: "stocks",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_currency_rates_currency_id",
                table: "currency_rates",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "IX_depositories_office_id",
                table: "depositories",
                column: "office_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoice_currency_convertions_currency_id",
                table: "invoice_currency_convertions",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoice_currency_convertions_invoice_id",
                table: "invoice_currency_convertions",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoice_discounts_invoice_id",
                table: "invoice_discounts",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoice_lines_currency_id",
                table: "invoice_lines",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoice_lines_invoice_id",
                table: "invoice_lines",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoice_lines_stock_id",
                table: "invoice_lines",
                column: "stock_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoice_lines_unit_id",
                table: "invoice_lines",
                column: "unit_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoice_overheads_currency_id",
                table: "invoice_overheads",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoice_overheads_invoice_id",
                table: "invoice_overheads",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoice_payments_currency_id",
                table: "invoice_payments",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoice_payments_invoice_id",
                table: "invoice_payments",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoice_stock_unit_convertions_invoice_id",
                table: "invoice_stock_unit_convertions",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoice_stock_unit_convertions_stock_id",
                table: "invoice_stock_unit_convertions",
                column: "stock_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoice_stock_unit_convertions_unit_id",
                table: "invoice_stock_unit_convertions",
                column: "unit_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_depository_id",
                table: "invoices",
                column: "depository_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_display_currency_id",
                table: "invoices",
                column: "display_currency_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_office_id",
                table: "invoices",
                column: "office_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_partner_id",
                table: "invoices",
                column: "partner_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_warehouse_id",
                table: "invoices",
                column: "warehouse_id");

            migrationBuilder.CreateIndex(
                name: "IX_partners_currency_id",
                table: "partners",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "IX_stock_additional_prices_currency_id",
                table: "stock_additional_prices",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "IX_stock_additional_prices_stock_id",
                table: "stock_additional_prices",
                column: "stock_id");

            migrationBuilder.CreateIndex(
                name: "IX_stock_balances_stock_id",
                table: "stock_balances",
                column: "stock_id");

            migrationBuilder.CreateIndex(
                name: "IX_stock_prices_currency_id",
                table: "stock_prices",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "IX_stock_prices_stock_id",
                table: "stock_prices",
                column: "stock_id");

            migrationBuilder.CreateIndex(
                name: "IX_stock_slip_lines_stock_id",
                table: "stock_slip_lines",
                column: "stock_id");

            migrationBuilder.CreateIndex(
                name: "IX_stock_slip_lines_stock_slip_id",
                table: "stock_slip_lines",
                column: "stock_slip_id");

            migrationBuilder.CreateIndex(
                name: "IX_stock_slip_lines_unit_id",
                table: "stock_slip_lines",
                column: "unit_id");

            migrationBuilder.CreateIndex(
                name: "IX_stock_slips_user_id",
                table: "stock_slips",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_stock_slips_warehouse_id",
                table: "stock_slips",
                column: "warehouse_id");

            migrationBuilder.CreateIndex(
                name: "IX_stock_units_stock_id",
                table: "stock_units",
                column: "stock_id");

            migrationBuilder.CreateIndex(
                name: "IX_stocks_search_vector",
                table: "stocks",
                column: "search_vector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "IX_warehouses_office_id",
                table: "warehouses",
                column: "office_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "currency_rates");

            migrationBuilder.DropTable(
                name: "invoice_currency_convertions");

            migrationBuilder.DropTable(
                name: "invoice_discounts");

            migrationBuilder.DropTable(
                name: "invoice_lines");

            migrationBuilder.DropTable(
                name: "invoice_overheads");

            migrationBuilder.DropTable(
                name: "invoice_payments");

            migrationBuilder.DropTable(
                name: "invoice_stock_unit_convertions");

            migrationBuilder.DropTable(
                name: "stock_additional_prices");

            migrationBuilder.DropTable(
                name: "stock_balances");

            migrationBuilder.DropTable(
                name: "stock_prices");

            migrationBuilder.DropTable(
                name: "stock_slip_lines");

            migrationBuilder.DropTable(
                name: "invoices");

            migrationBuilder.DropTable(
                name: "stock_slips");

            migrationBuilder.DropTable(
                name: "stock_units");

            migrationBuilder.DropTable(
                name: "depositories");

            migrationBuilder.DropTable(
                name: "partners");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "warehouses");

            migrationBuilder.DropTable(
                name: "stocks");

            migrationBuilder.DropTable(
                name: "currencies");

            migrationBuilder.DropTable(
                name: "offices");
        }
    }
}
