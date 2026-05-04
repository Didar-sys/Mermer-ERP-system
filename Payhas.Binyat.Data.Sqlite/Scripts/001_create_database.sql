-- =============================================================================
-- Payhas Binyat ERP — local SQLite cache schema
-- =============================================================================
-- Mirrors Payhas.Binyat.Data.Postgres/Scripts/001_create_database.sql, with
-- the following SQLite-specific adaptations:
--   * uuid → TEXT (stored as canonical 8-4-4-4-12 lowercase)
--   * numeric(18,4) → NUMERIC (REAL affinity is rejected in code; we
--     hand decimals as TEXT via Microsoft.Data.Sqlite type mapping)
--   * tsvector / pg_trgm — replaced by FTS5 virtual tables (stocks_fts)
--   * jsonb → TEXT (store as JSON strings)
--   * GENERATED ... STORED — SQLite supports it from 3.31+ (we have 3.42 in
--     Microsoft.Data.Sqlite 8.x); but we don't need search_vector locally
-- =============================================================================

PRAGMA foreign_keys = ON;
PRAGMA journal_mode = WAL;     -- better concurrency for the offline client
PRAGMA synchronous = NORMAL;   -- WAL+NORMAL is safe and ~5x faster than FULL

-- ─────────────────────────────────────────────────────────────────────────────
-- Reference tables
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS offices (
    id           TEXT PRIMARY KEY,
    name         TEXT NOT NULL,
    region       TEXT,
    description  TEXT,
    tags         TEXT,         -- JSON array
    is_disabled  INTEGER NOT NULL DEFAULT 0,
    created_at   TEXT NOT NULL DEFAULT (datetime('now')),
    updated_at   TEXT NOT NULL DEFAULT (datetime('now')),
    -- Sync metadata
    row_version  INTEGER NOT NULL DEFAULT 1,
    sync_state   TEXT    NOT NULL DEFAULT 'synced',  -- synced | dirty | conflict
    last_synced  TEXT
);

CREATE TABLE IF NOT EXISTS warehouses (
    id           TEXT PRIMARY KEY,
    office_id    TEXT REFERENCES offices(id),
    name         TEXT NOT NULL,
    description  TEXT,
    tags         TEXT,
    is_disabled  INTEGER NOT NULL DEFAULT 0,
    created_at   TEXT NOT NULL DEFAULT (datetime('now')),
    updated_at   TEXT NOT NULL DEFAULT (datetime('now')),
    row_version  INTEGER NOT NULL DEFAULT 1,
    sync_state   TEXT    NOT NULL DEFAULT 'synced',
    last_synced  TEXT
);

CREATE TABLE IF NOT EXISTS depositories (
    id           TEXT PRIMARY KEY,
    office_id    TEXT REFERENCES offices(id),
    name         TEXT NOT NULL,
    description  TEXT,
    tags         TEXT,
    is_disabled  INTEGER NOT NULL DEFAULT 0,
    created_at   TEXT NOT NULL DEFAULT (datetime('now')),
    updated_at   TEXT NOT NULL DEFAULT (datetime('now')),
    row_version  INTEGER NOT NULL DEFAULT 1,
    sync_state   TEXT    NOT NULL DEFAULT 'synced',
    last_synced  TEXT
);

CREATE TABLE IF NOT EXISTS currencies (
    id           TEXT PRIMARY KEY,
    name         TEXT NOT NULL,
    decimals     INTEGER NOT NULL DEFAULT 2,
    is_default   INTEGER NOT NULL DEFAULT 0,
    description  TEXT,
    is_disabled  INTEGER NOT NULL DEFAULT 0,
    created_at   TEXT NOT NULL DEFAULT (datetime('now')),
    updated_at   TEXT NOT NULL DEFAULT (datetime('now')),
    row_version  INTEGER NOT NULL DEFAULT 1,
    sync_state   TEXT    NOT NULL DEFAULT 'synced',
    last_synced  TEXT
);

CREATE TABLE IF NOT EXISTS currency_rates (
    id           TEXT PRIMARY KEY,
    currency_id  TEXT NOT NULL REFERENCES currencies(id) ON DELETE CASCADE,
    valid_from   TEXT NOT NULL,
    multiplier   NUMERIC(18,8) NOT NULL,
    divider      NUMERIC(18,8) NOT NULL,
    created_at   TEXT NOT NULL DEFAULT (datetime('now')),
    row_version  INTEGER NOT NULL DEFAULT 1
);

CREATE INDEX IF NOT EXISTS ix_currency_rates_lookup
  ON currency_rates(currency_id, valid_from DESC);

CREATE TABLE IF NOT EXISTS partners (
    id           TEXT PRIMARY KEY,
    code         TEXT,
    name         TEXT NOT NULL,
    phone        TEXT,
    address      TEXT,
    group_name   TEXT,
    credit_limit NUMERIC(18,4),
    tags         TEXT,
    description  TEXT,
    rating       NUMERIC(18,4),
    currency_id  TEXT REFERENCES currencies(id),
    is_disabled  INTEGER NOT NULL DEFAULT 0,
    created_at   TEXT NOT NULL DEFAULT (datetime('now')),
    updated_at   TEXT NOT NULL DEFAULT (datetime('now')),
    row_version  INTEGER NOT NULL DEFAULT 1,
    sync_state   TEXT    NOT NULL DEFAULT 'synced',
    last_synced  TEXT
);

-- ─────────────────────────────────────────────────────────────────────────────
-- Stock management
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS stocks (
    id           TEXT PRIMARY KEY,
    code         TEXT,
    name         TEXT NOT NULL,
    short_name   TEXT,
    type         TEXT,
    group_name   TEXT,
    tags         TEXT,
    barcodes     TEXT,
    limit_min    NUMERIC(18,4),
    limit_max    NUMERIC(18,4),
    description  TEXT,
    is_disabled  INTEGER NOT NULL DEFAULT 0,
    created_at   TEXT NOT NULL DEFAULT (datetime('now')),
    updated_at   TEXT NOT NULL DEFAULT (datetime('now')),
    row_version  INTEGER NOT NULL DEFAULT 1,
    sync_state   TEXT    NOT NULL DEFAULT 'synced',
    last_synced  TEXT
);

CREATE INDEX IF NOT EXISTS ix_stocks_code      ON stocks(code);
CREATE INDEX IF NOT EXISTS ix_stocks_disabled  ON stocks(is_disabled);
CREATE INDEX IF NOT EXISTS ix_stocks_sync      ON stocks(sync_state) WHERE sync_state != 'synced';

-- FTS5 virtual table replaces pg_trgm + tsvector for offline fuzzy search.
-- Triggers below keep it in sync with the main `stocks` table.
CREATE VIRTUAL TABLE IF NOT EXISTS stocks_fts USING fts5(
    stock_id  UNINDEXED,
    code,
    name,
    short_name,
    barcodes,
    tokenize = 'unicode61 remove_diacritics 2'
);

CREATE TRIGGER IF NOT EXISTS stocks_ai AFTER INSERT ON stocks BEGIN
    INSERT INTO stocks_fts(stock_id, code, name, short_name, barcodes)
    VALUES (NEW.id, COALESCE(NEW.code,''), NEW.name, COALESCE(NEW.short_name,''), COALESCE(NEW.barcodes,''));
END;
CREATE TRIGGER IF NOT EXISTS stocks_au AFTER UPDATE ON stocks BEGIN
    UPDATE stocks_fts
       SET code = COALESCE(NEW.code,''),
           name = NEW.name,
           short_name = COALESCE(NEW.short_name,''),
           barcodes   = COALESCE(NEW.barcodes,'')
     WHERE stock_id = NEW.id;
END;
CREATE TRIGGER IF NOT EXISTS stocks_ad AFTER DELETE ON stocks BEGIN
    DELETE FROM stocks_fts WHERE stock_id = OLD.id;
END;

CREATE TABLE IF NOT EXISTS stock_units (
    id           TEXT PRIMARY KEY,
    stock_id     TEXT NOT NULL REFERENCES stocks(id) ON DELETE CASCADE,
    name         TEXT NOT NULL,
    multiplier   NUMERIC(18,8) NOT NULL DEFAULT 1,
    divider      NUMERIC(18,8) NOT NULL DEFAULT 1,
    is_default   INTEGER NOT NULL DEFAULT 0,
    row_version  INTEGER NOT NULL DEFAULT 1
);

CREATE TABLE IF NOT EXISTS stock_prices (
    id           TEXT PRIMARY KEY,
    stock_id     TEXT NOT NULL REFERENCES stocks(id) ON DELETE CASCADE,
    valid_from   TEXT NOT NULL,
    price        NUMERIC(18,4) NOT NULL,
    currency_id  TEXT REFERENCES currencies(id),
    price_group  TEXT,
    created_at   TEXT NOT NULL DEFAULT (datetime('now')),
    row_version  INTEGER NOT NULL DEFAULT 1
);

CREATE INDEX IF NOT EXISTS ix_stock_prices_lookup
  ON stock_prices(stock_id, price_group, valid_from DESC);

CREATE TABLE IF NOT EXISTS stock_additional_prices (
    id           TEXT PRIMARY KEY,
    stock_id     TEXT NOT NULL REFERENCES stocks(id) ON DELETE CASCADE,
    price        NUMERIC(18,4) NOT NULL,
    currency_id  TEXT REFERENCES currencies(id),
    price_group  TEXT,
    valid_from   TEXT NOT NULL,
    row_version  INTEGER NOT NULL DEFAULT 1
);

CREATE TABLE IF NOT EXISTS stock_balances (
    warehouse_id TEXT NOT NULL REFERENCES warehouses(id),
    stock_id     TEXT NOT NULL REFERENCES stocks(id),
    income       NUMERIC(18,4) NOT NULL DEFAULT 0,
    expense      NUMERIC(18,4) NOT NULL DEFAULT 0,
    updated_at   TEXT NOT NULL DEFAULT (datetime('now')),
    PRIMARY KEY (warehouse_id, stock_id)
);

-- ─────────────────────────────────────────────────────────────────────────────
-- Commerce
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS invoices (
    id                       TEXT PRIMARY KEY,
    code                     TEXT,
    date                     TEXT NOT NULL,
    due_date                 TEXT,
    invoice_type             TEXT NOT NULL CHECK (invoice_type IN ('Purchase','PurchaseReturn','Sales','SalesReturn')),
    user_id                  TEXT,
    user_name                TEXT,
    office_id                TEXT REFERENCES offices(id),
    warehouse_id             TEXT REFERENCES warehouses(id),
    depository_id            TEXT REFERENCES depositories(id),
    partner_id               TEXT REFERENCES partners(id),
    display_currency_id      TEXT REFERENCES currencies(id),
    stock_price_group        TEXT,
    debit_credit_left_amount INTEGER NOT NULL DEFAULT 0,
    is_completed             INTEGER NOT NULL DEFAULT 0,
    is_disabled              INTEGER NOT NULL DEFAULT 0,
    group_name               TEXT,
    tags                     TEXT,
    description              TEXT,
    created_at               TEXT NOT NULL DEFAULT (datetime('now')),
    updated_at               TEXT NOT NULL DEFAULT (datetime('now')),
    row_version              INTEGER NOT NULL DEFAULT 1,
    sync_state               TEXT    NOT NULL DEFAULT 'synced',
    last_synced              TEXT
);

CREATE INDEX IF NOT EXISTS ix_invoices_date          ON invoices(date DESC);
CREATE INDEX IF NOT EXISTS ix_invoices_partner_date  ON invoices(partner_id, date DESC);
CREATE INDEX IF NOT EXISTS ix_invoices_warehouse_date ON invoices(warehouse_id, date DESC);
CREATE INDEX IF NOT EXISTS ix_invoices_sync          ON invoices(sync_state) WHERE sync_state != 'synced';

CREATE TABLE IF NOT EXISTS invoice_lines (
    id           TEXT PRIMARY KEY,
    invoice_id   TEXT NOT NULL REFERENCES invoices(id) ON DELETE CASCADE,
    source_id    TEXT,
    stock_id     TEXT REFERENCES stocks(id),
    unit_id      TEXT REFERENCES stock_units(id),
    quantity     NUMERIC(18,4) NOT NULL,
    price        NUMERIC(18,4) NOT NULL,
    currency_id  TEXT REFERENCES currencies(id),
    sort_order   INTEGER NOT NULL DEFAULT 0,
    row_version  INTEGER NOT NULL DEFAULT 1
);
CREATE INDEX IF NOT EXISTS ix_invoice_lines_invoice ON invoice_lines(invoice_id);
CREATE INDEX IF NOT EXISTS ix_invoice_lines_stock   ON invoice_lines(stock_id);
-- source_id chains a return line to its origin sale line; required by
-- Revenue Report cost-basis CTE (sales-return reuses original cost).
CREATE INDEX IF NOT EXISTS ix_invoice_lines_source  ON invoice_lines(source_id) WHERE source_id IS NOT NULL;

CREATE TABLE IF NOT EXISTS invoice_discounts (
    id            TEXT PRIMARY KEY,
    invoice_id    TEXT NOT NULL REFERENCES invoices(id) ON DELETE CASCADE,
    discount_type TEXT NOT NULL CHECK (discount_type IN ('Flat','Percentage')),
    amount        NUMERIC(18,4) NOT NULL,
    description   TEXT,
    sort_order    INTEGER NOT NULL DEFAULT 0,
    row_version   INTEGER NOT NULL DEFAULT 1
);
CREATE INDEX IF NOT EXISTS ix_invoice_discounts_invoice ON invoice_discounts(invoice_id);

CREATE TABLE IF NOT EXISTS invoice_payments (
    id           TEXT PRIMARY KEY,
    invoice_id   TEXT NOT NULL REFERENCES invoices(id) ON DELETE CASCADE,
    payment_type TEXT NOT NULL CHECK (payment_type IN ('Payment','Change')),
    amount       NUMERIC(18,4) NOT NULL,
    currency_id  TEXT REFERENCES currencies(id),
    sort_order   INTEGER NOT NULL DEFAULT 0,
    row_version  INTEGER NOT NULL DEFAULT 1
);
CREATE INDEX IF NOT EXISTS ix_invoice_payments_invoice ON invoice_payments(invoice_id);

CREATE TABLE IF NOT EXISTS invoice_overheads (
    id           TEXT PRIMARY KEY,
    invoice_id   TEXT NOT NULL REFERENCES invoices(id) ON DELETE CASCADE,
    amount       NUMERIC(18,4) NOT NULL,
    currency_id  TEXT REFERENCES currencies(id),
    description  TEXT,
    sort_order   INTEGER NOT NULL DEFAULT 0,
    row_version  INTEGER NOT NULL DEFAULT 1
);
CREATE INDEX IF NOT EXISTS ix_invoice_overheads_invoice ON invoice_overheads(invoice_id);

CREATE TABLE IF NOT EXISTS invoice_currency_convertions (
    id           TEXT PRIMARY KEY,
    invoice_id   TEXT NOT NULL REFERENCES invoices(id) ON DELETE CASCADE,
    currency_id  TEXT NOT NULL REFERENCES currencies(id),
    multiplier   NUMERIC(18,8) NOT NULL,
    divider      NUMERIC(18,8) NOT NULL,
    row_version  INTEGER NOT NULL DEFAULT 1
);
CREATE INDEX IF NOT EXISTS ix_invoice_currency_convertions_invoice ON invoice_currency_convertions(invoice_id);

CREATE TABLE IF NOT EXISTS invoice_stock_unit_convertions (
    id           TEXT PRIMARY KEY,
    invoice_id   TEXT NOT NULL REFERENCES invoices(id) ON DELETE CASCADE,
    stock_id     TEXT NOT NULL REFERENCES stocks(id),
    unit_id      TEXT NOT NULL REFERENCES stock_units(id),
    multiplier   NUMERIC(18,8) NOT NULL,
    divider      NUMERIC(18,8) NOT NULL,
    row_version  INTEGER NOT NULL DEFAULT 1
);
CREATE INDEX IF NOT EXISTS ix_invoice_stock_unit_convertions_invoice ON invoice_stock_unit_convertions(invoice_id);

-- ─────────────────────────────────────────────────────────────────────────────
-- Sync metadata
-- ─────────────────────────────────────────────────────────────────────────────

-- One row per table. Tracks the high-water mark we successfully pulled
-- from the central PostgreSQL. The next pull asks the server for rows
-- with updated_at > last_pulled_at.
CREATE TABLE IF NOT EXISTS sync_state (
    table_name      TEXT PRIMARY KEY,
    last_pulled_at  TEXT,             -- ISO-8601, server clock
    last_pushed_at  TEXT,             -- ISO-8601, local clock
    last_error      TEXT,
    last_run_at     TEXT NOT NULL DEFAULT (datetime('now'))
);

-- Per-row outbox of local mutations that haven't been pushed yet.
-- Populated automatically by triggers on each tracked table — see below.
CREATE TABLE IF NOT EXISTS sync_outbox (
    id           INTEGER PRIMARY KEY AUTOINCREMENT,
    table_name   TEXT NOT NULL,
    row_id       TEXT NOT NULL,
    operation    TEXT NOT NULL CHECK (operation IN ('insert','update','delete')),
    payload      TEXT,                -- JSON snapshot at the time of write
    queued_at    TEXT NOT NULL DEFAULT (datetime('now')),
    attempt      INTEGER NOT NULL DEFAULT 0,
    last_error   TEXT
);
CREATE INDEX IF NOT EXISTS ix_sync_outbox_queued
  ON sync_outbox(queued_at, table_name);

-- Conflict log — entries land here when a server pull overrides a local
-- dirty change (timestamp-based "server wins" today; see SyncService).
CREATE TABLE IF NOT EXISTS sync_conflicts (
    id           INTEGER PRIMARY KEY AUTOINCREMENT,
    table_name   TEXT NOT NULL,
    row_id       TEXT NOT NULL,
    local_data   TEXT NOT NULL,
    server_data  TEXT NOT NULL,
    detected_at  TEXT NOT NULL DEFAULT (datetime('now')),
    resolution   TEXT             -- 'server' | 'local' | NULL when unresolved
);
