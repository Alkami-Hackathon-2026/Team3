-- Local development only: seeds recent demo transactions for the mike.brady test user
-- (account 21086) so the Your Month widget has data in its lookback window.
-- Safe to re-run: deletes previously seeded rows (TranKey LIKE 'TEAM3SEED%') first.
USE [DeveloperDynamic]
GO
SET XACT_ABORT ON;
BEGIN TRAN;

DECLARE @AccountId BIGINT = 21086;
DECLARE @Now DATETIME = GETUTCDATE();
DECLARE @Seq BIGINT = 2000000000000;

DELETE FROM core.Transactions WHERE TranKey LIKE 'TEAM3SEED%';

DECLARE @Rows TABLE (DaysAgo INT, Amount DECIMAL(18,5), Debit BIT, Descr NVARCHAR(255));

-- Recurring subscriptions: same merchant both calendar months, amounts within 10%
INSERT INTO @Rows VALUES
 ( 8, 15.99, 1, N'NETFLIX.COM #4821'),
 (38, 15.49, 1, N'NETFLIX.COM #4821'),
 ( 6, 11.99, 1, N'SPOTIFY USA'),
 (36, 11.99, 1, N'SPOTIFY USA'),
 (12, 45.00, 1, N'POWERHOUSE GYM MEMBERSHIP'),
 (42, 45.00, 1, N'POWERHOUSE GYM MEMBERSHIP'),
 ( 4, 79.99, 1, N'CITY INSURANCE PREMIUM'),
 (34, 79.99, 1, N'CITY INSURANCE PREMIUM');

-- Groceries (top merchant)
INSERT INTO @Rows VALUES
 ( 2, 124.37, 1, N'POS GROCERY MART STORE 42'),
 ( 9,  86.12, 1, N'POS GROCERY MART STORE 42'),
 (16, 102.45, 1, N'POS GROCERY MART STORE 42'),
 (23,  93.30, 1, N'POS GROCERY MART STORE 42'),
 (33, 110.84, 1, N'POS GROCERY MART STORE 42'),
 (40,  78.29, 1, N'POS GROCERY MART STORE 42'),
 (47,  95.61, 1, N'POS GROCERY MART STORE 42'),
 (54,  88.14, 1, N'POS GROCERY MART STORE 42');

-- Gas
INSERT INTO @Rows VALUES
 ( 5, 52.20, 1, N'GAS STOP #17'),
 (13, 48.75, 1, N'GAS STOP #17'),
 (21, 55.10, 1, N'GAS STOP #17'),
 (37, 49.95, 1, N'GAS STOP #17'),
 (51, 53.40, 1, N'GAS STOP #17');

-- Restaurants / coffee / misc (this month heavier than last month)
INSERT INTO @Rows VALUES
 ( 1, 18.42, 1, N'COFFEE HUT'),
 ( 3, 64.90, 1, N'BELLA PASTA RESTAURANT'),
 ( 7, 14.85, 1, N'COFFEE HUT'),
 (10, 42.10, 1, N'BURGER BARN'),
 (11, 156.99, 1, N'BIG BOX ELECTRONICS'),
 (14, 17.30, 1, N'COFFEE HUT'),
 (15, 89.99, 1, N'SHOE WAREHOUSE'),
 (18, 27.60, 1, N'PIZZA PALACE'),
 (20, 12.75, 1, N'COFFEE HUT'),
 (25, 73.25, 1, N'HOME AND GARDEN CENTER'),
 (35, 22.18, 1, N'BURGER BARN'),
 (41, 16.20, 1, N'COFFEE HUT'),
 (44, 58.35, 1, N'BELLA PASTA RESTAURANT'),
 (49, 31.44, 1, N'PIZZA PALACE'),
 (56, 19.05, 1, N'COFFEE HUT');

-- Credits (ignored by the summary math)
INSERT INTO @Rows VALUES
 ( 5, 2450.00, 0, N'ACME CORP PAYROLL'),
 (19, 2450.00, 0, N'ACME CORP PAYROLL'),
 (35, 2450.00, 0, N'ACME CORP PAYROLL'),
 (49, 2450.00, 0, N'ACME CORP PAYROLL');

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
    @Seq + ROW_NUMBER() OVER (ORDER BY r.DaysAgo),
    4858,
    @Now,
    'TEAM3SEED' + CONVERT(NVARCHAR(20), ROW_NUMBER() OVER (ORDER BY r.DaysAgo)),
    0
FROM @Rows r;

SELECT COUNT(*) AS SeededRows FROM core.Transactions WHERE TranKey LIKE 'TEAM3SEED%';

COMMIT TRAN;
