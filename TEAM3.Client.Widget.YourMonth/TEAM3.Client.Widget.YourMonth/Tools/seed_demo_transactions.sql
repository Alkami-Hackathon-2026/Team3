-- Local development only: seeds recent demo transactions for the mike.brady test user
-- (account 21086) so the Your Month widget has data in its lookback window, with a
-- category split on every transaction (categories resolved by DisplayName from the
-- user's own core.TransactionCategory tree, never hardcoded IDs).
-- Safe to re-run: deletes previously seeded rows (TranKey LIKE 'TEAM3SEED%') first.
USE [DeveloperDynamic]
GO
SET XACT_ABORT ON;
BEGIN TRAN;

DECLARE @AccountId BIGINT = 21086;
DECLARE @UserId BIGINT = 4858;
DECLARE @Now DATETIME = GETUTCDATE();
DECLARE @Seq BIGINT = 2000000000000;

DELETE s FROM core.TransactionCategorySplit s
    JOIN core.Transactions t ON t.ID = s.TransactionID
    WHERE t.TranKey LIKE 'TEAM3SEED%';
DELETE FROM core.Transactions WHERE TranKey LIKE 'TEAM3SEED%';

DECLARE @Rows TABLE (DaysAgo INT, Amount DECIMAL(18,5), Debit BIT, Descr NVARCHAR(255), Category NVARCHAR(255));

-- Recurring subscriptions: same merchant both calendar months, amounts within 10%
INSERT INTO @Rows VALUES
 ( 8, 15.99, 1, N'NETFLIX.COM #4821',          N'Movies & DVDs'),
 (38, 15.49, 1, N'NETFLIX.COM #4821',          N'Movies & DVDs'),
 ( 6, 11.99, 1, N'SPOTIFY USA',                N'Music'),
 (36, 11.99, 1, N'SPOTIFY USA',                N'Music'),
 (12, 45.00, 1, N'POWERHOUSE GYM MEMBERSHIP',  N'Gym'),
 (42, 45.00, 1, N'POWERHOUSE GYM MEMBERSHIP',  N'Gym'),
 ( 4, 79.99, 1, N'CITY INSURANCE PREMIUM',     N'Home Insurance'),
 (34, 79.99, 1, N'CITY INSURANCE PREMIUM',     N'Home Insurance');

-- Groceries (top merchant)
INSERT INTO @Rows VALUES
 ( 2, 124.37, 1, N'POS GROCERY MART STORE 42', N'Groceries'),
 ( 9,  86.12, 1, N'POS GROCERY MART STORE 42', N'Groceries'),
 (16, 102.45, 1, N'POS GROCERY MART STORE 42', N'Groceries'),
 (23,  93.30, 1, N'POS GROCERY MART STORE 42', N'Groceries'),
 (33, 110.84, 1, N'POS GROCERY MART STORE 42', N'Groceries'),
 (40,  78.29, 1, N'POS GROCERY MART STORE 42', N'Groceries'),
 (47,  95.61, 1, N'POS GROCERY MART STORE 42', N'Groceries'),
 (54,  88.14, 1, N'POS GROCERY MART STORE 42', N'Groceries');

-- Gas
INSERT INTO @Rows VALUES
 ( 5, 52.20, 1, N'GAS STOP #17', N'Gas & Fuel'),
 (13, 48.75, 1, N'GAS STOP #17', N'Gas & Fuel'),
 (21, 55.10, 1, N'GAS STOP #17', N'Gas & Fuel'),
 (37, 49.95, 1, N'GAS STOP #17', N'Gas & Fuel'),
 (51, 53.40, 1, N'GAS STOP #17', N'Gas & Fuel');

-- Restaurants / coffee / misc (this month heavier than last month)
INSERT INTO @Rows VALUES
 ( 1, 18.42, 1, N'COLON CLEANSE COFFEE HUT',              N'Coffee Shops'),
 ( 3, 64.90, 1, N'BELLA PASTA RESTAURANT',  N'Restaurants'),
 ( 7, 14.85, 1, N'COLON CLEANSE COFFEE HUT',              N'Coffee Shops'),
 (10, 42.10, 1, N'BURGER BARN',             N'Fast Food'),
 (11, 156.99, 1, N'BIG BOX ELECTRONICS',    N'Electronics & Software'),
 (14, 17.30, 1, N'COLON CLEANSE COFFEE HUT',              N'Coffee Shops'),
 (15, 89.99, 1, N'SHOE WAREHOUSE',          N'Clothing'),
 (18, 27.60, 1, N'PIZZA PALACE',            N'Restaurants'),
 (20, 12.75, 1, N'COLON CLEANSE COFFEE HUT',              N'Coffee Shops'),
 (25, 73.25, 1, N'HOME AND GARDEN CENTER',  N'Home Improvement'),
 (35, 22.18, 1, N'BURGER BARN',             N'Fast Food'),
 (41, 16.20, 1, N'COLON CLEANSE COFFEE HUT',              N'Coffee Shops'),
 (44, 58.35, 1, N'BELLA PASTA RESTAURANT',  N'Restaurants'),
 (49, 31.44, 1, N'PIZZA PALACE',            N'Restaurants'),
 (56, 19.05, 1, N'COLON CLEANSE COFFEE HUT',              N'Coffee Shops');

-- Credits (ignored by the summary math)
INSERT INTO @Rows VALUES
 ( 5, 2450.00, 0, N'ACME CORP PAYROLL', N'Paycheck'),
 (19, 2450.00, 0, N'ACME CORP PAYROLL', N'Paycheck'),
 (35, 2450.00, 0, N'ACME CORP PAYROLL', N'Paycheck'),
 (49, 2450.00, 0, N'ACME CORP PAYROLL', N'Paycheck');

-- Every category name must resolve to a category owned by the test user, or abort.
IF EXISTS (
    SELECT 1 FROM @Rows r
    WHERE NOT EXISTS (
        SELECT 1 FROM core.TransactionCategory c
        WHERE c.UserID = @UserId AND c.DisplayName = r.Category))
BEGIN
    RAISERROR ('One or more category names do not exist in core.TransactionCategory for the test user.', 16, 1);
    RETURN;
END

INSERT INTO core.Transactions
    (AccountID, PostingDate, EffectiveDate, Amount, Debit, GeneralDescription, SpecificDescription,
     CoreSequence, UserID, CreateDate, TranKey, IsVoid)
SELECT
    @AccountId,
    DATEADD(day, -r.DaysAgo, CONVERT(DATE, @Now)),
    DATEADD(day, -r.DaysAgo, CONVERT(DATE, @Now)),
    r.Amount,
    r.Debit,
    r.Descr,
    r.Descr,
    @Seq + ROW_NUMBER() OVER (ORDER BY r.DaysAgo, r.Descr, r.Amount),
    @UserId,
    @Now,
    'TEAM3SEED' + CONVERT(NVARCHAR(20), ROW_NUMBER() OVER (ORDER BY r.DaysAgo, r.Descr, r.Amount)),
    0
FROM @Rows r;

-- One full-amount category split per seeded transaction. The seeded CoreSequence
-- (@Seq + n, ordered by DaysAgo) lines the transaction back up with its @Rows entry.
INSERT INTO core.TransactionCategorySplit (TransactionID, TransactionCategoryID, Amount, CreateDate)
SELECT
    t.ID,
    c.ID,
    t.Amount,
    @Now
FROM (SELECT r.*, ROW_NUMBER() OVER (ORDER BY r.DaysAgo, r.Descr, r.Amount) AS RowNum FROM @Rows r) r
JOIN core.Transactions t
    ON t.TranKey = 'TEAM3SEED' + CONVERT(NVARCHAR(20), r.RowNum)
JOIN core.TransactionCategory c
    ON c.UserID = @UserId AND c.DisplayName = r.Category;

SELECT COUNT(*) AS SeededRows FROM core.Transactions WHERE TranKey LIKE 'TEAM3SEED%';
SELECT COUNT(*) AS SeededSplits FROM core.TransactionCategorySplit s
    JOIN core.Transactions t ON t.ID = s.TransactionID WHERE t.TranKey LIKE 'TEAM3SEED%';

COMMIT TRAN;
