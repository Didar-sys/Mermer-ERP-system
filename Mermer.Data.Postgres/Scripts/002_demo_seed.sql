-- ============================================================
-- Mermer ERP ERP — Demo Seed Data (50 000 записей)
-- Запускать ПОСЛЕ 001_create_database.sql
-- ============================================================

-- 1. Валюта
INSERT INTO currencies (id, name, decimals, is_default)
VALUES ('00000000-0000-0000-0000-000000000001', 'TMT', 2, true);

-- 2. Офис и склады
INSERT INTO offices (id, name, region)
VALUES ('00000000-0000-0000-0000-000000000010', 'Главный офис', 'Ашхабад');

INSERT INTO warehouses (id, office_id, name) VALUES
('00000000-0000-0000-0000-000000000020', '00000000-0000-0000-0000-000000000010', 'Склад №1'),
('00000000-0000-0000-0000-000000000021', '00000000-0000-0000-0000-000000000010', 'Склад №2'),
('00000000-0000-0000-0000-000000000022', '00000000-0000-0000-0000-000000000010', 'Склад №3');

INSERT INTO depositories (id, office_id, name)
VALUES ('00000000-0000-0000-0000-000000000030', '00000000-0000-0000-0000-000000000010', 'Касса');

-- 3. Пользователь
INSERT INTO users (id, username, password_hash, display_name)
VALUES ('00000000-0000-0000-0000-000000000040', 'admin', 'pbkdf2:sha256:demo_hash', 'Администратор');

-- 4. 500 партнёров (клиентов/поставщиков)
INSERT INTO partners (id, code, name, phone, group_name, credit_limit, currency_id)
SELECT
    gen_random_uuid(),
    'P-' || LPAD(n::text, 4, '0'),
    CASE (n % 5)
        WHEN 0 THEN 'ООО Меркурий ' || n
        WHEN 1 THEN 'ИП Атаев ' || n
        WHEN 2 THEN 'Компания Берекет ' || n
        WHEN 3 THEN 'Фирма Ылхам ' || n
        ELSE 'Предприятие Нур ' || n
    END,
    '+993 6' || LPAD((n % 9 + 1)::text, 1, '0') || '-' || LPAD(n::text, 6, '0'),
    CASE (n % 3) WHEN 0 THEN 'Оптовики' WHEN 1 THEN 'Розница' ELSE 'VIP' END,
    ROUND((RANDOM() * 50000 + 5000)::numeric, 2),
    '00000000-0000-0000-0000-000000000001'
FROM generate_series(1, 500) n;

-- 5. 1000 товаров
INSERT INTO stocks (id, code, name, short_name, type, group_name)
SELECT
    gen_random_uuid(),
    'SKU-' || LPAD(n::text, 5, '0'),
    CASE (n % 8)
        WHEN 0 THEN 'Цемент М' || (n % 4 + 1) * 100 || ' ' || n
        WHEN 1 THEN 'Арматура Ø' || (n % 6 + 8) || 'мм ' || n
        WHEN 2 THEN 'Кирпич рядовой ' || n
        WHEN 3 THEN 'Плитка керамическая ' || n
        WHEN 4 THEN 'Краска акриловая ' || n
        WHEN 5 THEN 'Профиль металлический ' || n
        WHEN 6 THEN 'Утеплитель Rockwool ' || n
        ELSE 'Труба ПВХ ' || n
    END,
    'Товар-' || n,
    CASE (n % 3) WHEN 0 THEN 'Строительный' WHEN 1 THEN 'Отделочный' ELSE 'Металл' END,
    CASE (n % 4) WHEN 0 THEN 'Цемент/Бетон' WHEN 1 THEN 'Металл' WHEN 2 THEN 'Отделка' ELSE 'Прочее' END
FROM generate_series(1, 1000) n;

-- 6. Единицы измерения для всех товаров
INSERT INTO stock_units (id, stock_id, name, multiplier, divider, is_default)
SELECT gen_random_uuid(), s.id,
    CASE (ROW_NUMBER() OVER () % 4)
        WHEN 0 THEN 'шт' WHEN 1 THEN 'кг' WHEN 2 THEN 'м²' ELSE 'м³'
    END,
    1, 1, true
FROM stocks s;

-- 7. Цены для всех товаров
INSERT INTO stock_prices (id, stock_id, valid_from, price, currency_id, price_group)
SELECT gen_random_uuid(), s.id, CURRENT_DATE,
    ROUND((RANDOM() * 5000 + 100)::numeric, 2),
    '00000000-0000-0000-0000-000000000001',
    NULL
FROM stocks s;

-- 8. Начальные остатки на складах
INSERT INTO stock_balances (warehouse_id, stock_id, income, expense)
SELECT
    w.id,
    s.id,
    ROUND((RANDOM() * 10000 + 500)::numeric, 4),
    ROUND((RANDOM() * 2000)::numeric, 4)
FROM stocks s
CROSS JOIN warehouses w;

-- 9. 50 000 накладных (продажи и закупки)
-- Создаём накладные
INSERT INTO invoices (
    id, code, date, invoice_type,
    user_id, user_name, office_id, warehouse_id, depository_id, partner_id,
    display_currency_id, is_completed, stock_price_group
)
SELECT
    gen_random_uuid(),
    CASE (n % 2) WHEN 0 THEN 'ПРД-' ELSE 'ЗКП-' END || LPAD(n::text, 6, '0'),
    NOW() - ((RANDOM() * 365 * 2)::int || ' days')::interval,
    CASE (n % 4)
        WHEN 0 THEN 'Sales'
        WHEN 1 THEN 'Purchase'
        WHEN 2 THEN 'Sales'
        ELSE 'Purchase'
    END,
    '00000000-0000-0000-0000-000000000040',
    'Администратор',
    '00000000-0000-0000-0000-000000000010',
    (ARRAY['00000000-0000-0000-0000-000000000020',
           '00000000-0000-0000-0000-000000000021',
           '00000000-0000-0000-0000-000000000022'])[1 + (n % 3)]::uuid,
    '00000000-0000-0000-0000-000000000030',
    (SELECT id FROM partners ORDER BY RANDOM() LIMIT 1),
    '00000000-0000-0000-0000-000000000001',
    true,
    NULL
FROM generate_series(1, 50000) n;

-- 10. Позиции накладных (1-3 позиции на каждую)
INSERT INTO invoice_lines (id, invoice_id, stock_id, unit_id, quantity, price, currency_id)
SELECT
    gen_random_uuid(),
    i.id,
    s.id,
    su.id,
    ROUND((RANDOM() * 100 + 1)::numeric, 4),
    ROUND((RANDOM() * 5000 + 50)::numeric, 4),
    '00000000-0000-0000-0000-000000000001'
FROM invoices i
CROSS JOIN LATERAL (
    SELECT id FROM stocks ORDER BY RANDOM() LIMIT (1 + (RANDOM() * 2)::int)
) s
JOIN stock_units su ON su.stock_id = s.id AND su.is_default = true;

-- 11. Скидки (30% накладных имеют скидку)
INSERT INTO invoice_discounts (id, invoice_id, discount_type, amount)
SELECT gen_random_uuid(), i.id,
    CASE WHEN RANDOM() > 0.5 THEN 'Percentage' ELSE 'Flat' END,
    ROUND((RANDOM() * 1000 + 50)::numeric, 4)
FROM invoices i
WHERE RANDOM() < 0.3;

-- 12. Платежи (80% накладных оплачены)
INSERT INTO invoice_payments (id, invoice_id, payment_type, amount, currency_id)
SELECT gen_random_uuid(), i.id, 'Payment',
    ROUND((
        SELECT COALESCE(SUM(il2.quantity * il2.price), 0)
        FROM invoice_lines il2
        WHERE il2.invoice_id = i.id
    ) * (0.7 + RANDOM() * 0.3), 4),
    '00000000-0000-0000-0000-000000000001'
FROM invoices i
WHERE RANDOM() < 0.8;

-- ============================================================
-- ПРОВЕРКА — эти запросы должны вернуть результат за < 100ms
-- ============================================================
\echo ''
\echo '=== РЕЗУЛЬТАТЫ ДЕМО ==='
\echo ''

\echo '--- Кол-во записей ---'
SELECT
    (SELECT COUNT(*) FROM invoices)      AS invoices,
    (SELECT COUNT(*) FROM invoice_lines) AS lines,
    (SELECT COUNT(*) FROM stocks)        AS stocks,
    (SELECT COUNT(*) FROM partners)      AS partners;

\echo ''
\echo '--- Финансовый отчёт за последние 30 дней (без декартова произведения) ---'
\timing on
WITH invoice_subtotals AS (
    SELECT i.id, COALESCE(SUM(il.quantity * il.price), 0) AS subtotal
    FROM invoices i
    LEFT JOIN invoice_lines il ON il.invoice_id = i.id
    WHERE i.date >= NOW() - INTERVAL '30 days' AND i.is_completed = true
    GROUP BY i.id
),
invoice_discount_totals AS (
    -- Корректный учёт типа скидки: Flat — абсолютная, Percentage — от subtotal
    SELECT
        i.id,
        COALESCE(SUM(
            CASE id2.discount_type
                WHEN 'Percentage' THEN s.subtotal * id2.amount / 100
                ELSE id2.amount
            END
        ), 0) AS discount_total
    FROM invoices i
    JOIN invoice_subtotals s ON s.id = i.id
    LEFT JOIN invoice_discounts id2 ON id2.invoice_id = i.id
    GROUP BY i.id
),
invoice_payment_totals AS (
    SELECT i.id,
           COALESCE(SUM(ip.amount) FILTER (WHERE ip.payment_type = 'Payment'), 0) AS paid
    FROM invoices i
    LEFT JOIN invoice_payments ip ON ip.invoice_id = i.id
    WHERE i.date >= NOW() - INTERVAL '30 days' AND i.is_completed = true
    GROUP BY i.id
)
SELECT
    COUNT(*)                                            AS "Накладных",
    ROUND(SUM(s.subtotal)::numeric, 2)                  AS "Оборот (TMT)",
    ROUND(SUM(d.discount_total)::numeric, 2)            AS "Скидки (TMT)",
    ROUND((SUM(s.subtotal) - SUM(d.discount_total))::numeric, 2) AS "Итог (TMT)",
    ROUND(SUM(p.paid)::numeric, 2)                      AS "Оплачено (TMT)"
FROM invoice_subtotals s
JOIN invoice_discount_totals d ON d.id = s.id
JOIN invoice_payment_totals  p ON p.id = s.id;
\timing off

\echo ''
\echo '--- Остатки по складам (топ 10 товаров) ---'
\timing on
SELECT
    s.name                         AS "Товар",
    s.code                         AS "Код",
    SUM(sb.income - sb.expense)    AS "Остаток",
    su.name                        AS "Ед."
FROM stock_balances sb
JOIN stocks     s  ON s.id  = sb.stock_id
JOIN stock_units su ON su.stock_id = s.id AND su.is_default = true
GROUP BY s.name, s.code, su.name
ORDER BY SUM(sb.income - sb.expense) DESC
LIMIT 10;
\timing off

\echo ''
\echo '=== ДЕМО ЗАВЕРШЕНО — всё работает! ==='
