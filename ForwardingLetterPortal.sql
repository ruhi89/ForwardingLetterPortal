-- ============================================================
-- Database: ForwardingLetterPortal
-- Re-runnable: safe to execute any number of times.
--   * Missing tables, columns, constraints and indexes are created.
--   * Existing objects and data are left untouched.
-- ============================================================
IF DB_ID(N'ForwardingLetterPortal') IS NULL
    CREATE DATABASE ForwardingLetterPortal;
GO

USE ForwardingLetterPortal;
GO

-- ============================================================
-- 1. BankAccounts
--    Pre-loaded from bank core system. Never written by app.
-- ============================================================
IF OBJECT_ID(N'dbo.BankAccounts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BankAccounts (
        BankAccountId   INT             IDENTITY(1,1)   PRIMARY KEY,
        AccountNumber   VARCHAR(20)     NOT NULL UNIQUE,
        FullName        NVARCHAR(150)   NOT NULL,
        Email           VARCHAR(150)    NOT NULL,
        MobileNumber    VARCHAR(20)     NOT NULL,
        DateOfBirth     DATE            NOT NULL,
        BranchName      NVARCHAR(200)   NOT NULL,
        AccountType     VARCHAR(50)     NOT NULL,       -- 'Savings' | 'Current' | 'Fixed'
        IsActive        BIT             NOT NULL DEFAULT 1,
        CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE()
    );
END
GO

-- ============================================================
-- 2. VerificationAttempts
--    Logs every identity check — success and failure.
--    No FK to BankAccounts so failed attempts can still be logged.
-- ============================================================
IF OBJECT_ID(N'dbo.VerificationAttempts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.VerificationAttempts (
        AttemptId       INT             IDENTITY(1,1)   PRIMARY KEY,
        AccountNumber   VARCHAR(20)     NOT NULL,
        EmailEntered    VARCHAR(150)    NOT NULL,
        DobEntered      DATE            NOT NULL,
        IsSuccess       BIT             NOT NULL DEFAULT 0,
        FailureReason   VARCHAR(100)    NULL,           -- 'AccountNotFound' | 'EmailMismatch' | 'DobMismatch' | 'AlreadyRegistered'
        IpAddress       VARCHAR(45)     NULL,
        AttemptedAt     DATETIME2       NOT NULL DEFAULT GETUTCDATE()
    );
END
GO

-- ============================================================
-- 3. Customers
--    Created only after successful verification.
--    Real info (name, email, DOB) always read from BankAccounts.
-- ============================================================
IF OBJECT_ID(N'dbo.Customers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Customers (
        CustomerId      INT             IDENTITY(1,1)   PRIMARY KEY,
        BankAccountId   INT             NOT NULL UNIQUE,
        Username        VARCHAR(100)    NOT NULL UNIQUE,
        PasswordHash    NVARCHAR(256)   NOT NULL,
        IsActive        BIT             NOT NULL DEFAULT 1,
        CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt       DATETIME2       NULL,

        CONSTRAINT FK_Customers_BankAccount
            FOREIGN KEY (BankAccountId) REFERENCES dbo.BankAccounts(BankAccountId)
    );
END
GO

-- ============================================================
-- 4. ForwardingLetterRequests
--    One row per letter request submitted via the form.
-- ============================================================
IF OBJECT_ID(N'dbo.ForwardingLetterRequests', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ForwardingLetterRequests (
        RequestId           INT             IDENTITY(1,1)   PRIMARY KEY,
        CustomerId          INT             NOT NULL,
        SenderName          NVARCHAR(150)   NOT NULL,
        SenderAccountNumber VARCHAR(30)     NOT NULL,
        SenderAddress       NVARCHAR(500)   NOT NULL,
        SenderPhone         VARCHAR(20)     NULL,
        SenderEmail         NVARCHAR(150)   NULL,
        RecipientName       NVARCHAR(150)   NOT NULL,
        RecipientBankName   NVARCHAR(200)   NOT NULL,
        RecipientBranchName NVARCHAR(200)   NOT NULL,
        RecipientAddress    NVARCHAR(500)   NOT NULL,
        Purpose             NVARCHAR(500)   NOT NULL,
        DocumentDescription NVARCHAR(1000)  NOT NULL,
        Status              VARCHAR(20)     NOT NULL DEFAULT 'Submitted',   -- 'Submitted' | 'Approved' | 'Rejected' | 'Generated'
        RejectionReason     NVARCHAR(500)   NULL,
        SubmittedAt         DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt           DATETIME2       NULL,

        CONSTRAINT FK_ForwardingLetterRequests_Customer
            FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(CustomerId)
    );
END
GO

-- Migration: add Sender columns to a table created before they existed.
-- NOT NULL columns get an empty-string default so existing rows can be backfilled.
IF COL_LENGTH('dbo.ForwardingLetterRequests', 'SenderName') IS NULL
    ALTER TABLE dbo.ForwardingLetterRequests ADD SenderName NVARCHAR(150) NOT NULL
        CONSTRAINT DF_FLR_SenderName DEFAULT '';
IF COL_LENGTH('dbo.ForwardingLetterRequests', 'SenderAccountNumber') IS NULL
    ALTER TABLE dbo.ForwardingLetterRequests ADD SenderAccountNumber VARCHAR(30) NOT NULL
        CONSTRAINT DF_FLR_SenderAccountNumber DEFAULT '';
IF COL_LENGTH('dbo.ForwardingLetterRequests', 'SenderAddress') IS NULL
    ALTER TABLE dbo.ForwardingLetterRequests ADD SenderAddress NVARCHAR(500) NOT NULL
        CONSTRAINT DF_FLR_SenderAddress DEFAULT '';
IF COL_LENGTH('dbo.ForwardingLetterRequests', 'SenderPhone') IS NULL
    ALTER TABLE dbo.ForwardingLetterRequests ADD SenderPhone VARCHAR(20) NULL;
IF COL_LENGTH('dbo.ForwardingLetterRequests', 'SenderEmail') IS NULL
    ALTER TABLE dbo.ForwardingLetterRequests ADD SenderEmail NVARCHAR(150) NULL;
GO

-- ============================================================
-- 5. GeneratedLetters
--    PDF metadata — one row per generated file.
-- ============================================================
IF OBJECT_ID(N'dbo.GeneratedLetters', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.GeneratedLetters (
        LetterId        INT             IDENTITY(1,1)   PRIMARY KEY,
        RequestId       INT             NOT NULL UNIQUE,
        CustomerId      INT             NOT NULL,
        FilePath        NVARCHAR(500)   NOT NULL,
        FileSize        BIGINT          NULL,
        GeneratedAt     DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
        DownloadCount   INT             NOT NULL DEFAULT 0,
        LastDownloadAt  DATETIME2       NULL,

        CONSTRAINT FK_GeneratedLetters_Request
            FOREIGN KEY (RequestId) REFERENCES dbo.ForwardingLetterRequests(RequestId),
        CONSTRAINT FK_GeneratedLetters_Customer
            FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(CustomerId)
    );
END
GO

-- ============================================================
-- 6. DownloadLogs
--    Audit trail for every PDF download event.
-- ============================================================
IF OBJECT_ID(N'dbo.DownloadLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DownloadLogs (
        LogId           INT             IDENTITY(1,1)   PRIMARY KEY,
        LetterId        INT             NOT NULL,
        CustomerId      INT             NOT NULL,
        DownloadedAt    DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
        IpAddress       VARCHAR(45)     NULL,
        UserAgent       NVARCHAR(500)   NULL,

        CONSTRAINT FK_DownloadLogs_Letter
            FOREIGN KEY (LetterId) REFERENCES dbo.GeneratedLetters(LetterId),
        CONSTRAINT FK_DownloadLogs_Customer
            FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(CustomerId)
    );
END
GO

-- ============================================================
-- 7. Admins  (future — bank officers)
-- ============================================================
IF OBJECT_ID(N'dbo.Admins', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Admins (
        AdminId         INT             IDENTITY(1,1)   PRIMARY KEY,
        FullName        NVARCHAR(150)   NOT NULL,
        Email           VARCHAR(150)    NOT NULL UNIQUE,
        PasswordHash    NVARCHAR(256)   NOT NULL,
        Role            VARCHAR(50)     NOT NULL DEFAULT 'Officer',     -- 'Officer' | 'Manager' | 'SuperAdmin'
        IsActive        BIT             NOT NULL DEFAULT 1,
        CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE()
    );
END
GO

-- ============================================================
-- 8. ApprovalLogs  (future — officer approve/reject audit)
-- ============================================================
IF OBJECT_ID(N'dbo.ApprovalLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ApprovalLogs (
        ApprovalLogId   INT             IDENTITY(1,1)   PRIMARY KEY,
        RequestId       INT             NOT NULL,
        AdminId         INT             NOT NULL,
        Action          VARCHAR(20)     NOT NULL,       -- 'Approved' | 'Rejected'
        Note            NVARCHAR(500)   NULL,
        ActionAt        DATETIME2       NOT NULL DEFAULT GETUTCDATE(),

        CONSTRAINT FK_ApprovalLogs_Request
            FOREIGN KEY (RequestId) REFERENCES dbo.ForwardingLetterRequests(RequestId),
        CONSTRAINT FK_ApprovalLogs_Admin
            FOREIGN KEY (AdminId) REFERENCES dbo.Admins(AdminId)
    );
END
GO

-- Add VerifiedByAdminId (and its FK) to VerificationAttempts once Admins exists
IF COL_LENGTH('dbo.VerificationAttempts', 'VerifiedByAdminId') IS NULL
    ALTER TABLE dbo.VerificationAttempts ADD VerifiedByAdminId INT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys
               WHERE name = N'FK_VerificationAttempts_Admin'
                 AND parent_object_id = OBJECT_ID(N'dbo.VerificationAttempts'))
    ALTER TABLE dbo.VerificationAttempts
        ADD CONSTRAINT FK_VerificationAttempts_Admin
            FOREIGN KEY (VerifiedByAdminId) REFERENCES dbo.Admins(AdminId);
GO

-- ============================================================
-- Indexes
-- ============================================================

-- BankAccounts
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BankAccounts_AccountNumber' AND object_id = OBJECT_ID(N'dbo.BankAccounts'))
    CREATE UNIQUE INDEX IX_BankAccounts_AccountNumber ON dbo.BankAccounts(AccountNumber);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BankAccounts_Email' AND object_id = OBJECT_ID(N'dbo.BankAccounts'))
    CREATE INDEX IX_BankAccounts_Email ON dbo.BankAccounts(Email);

-- VerificationAttempts
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_VerificationAttempts_AccountNumber' AND object_id = OBJECT_ID(N'dbo.VerificationAttempts'))
    CREATE INDEX IX_VerificationAttempts_AccountNumber ON dbo.VerificationAttempts(AccountNumber, AttemptedAt DESC);

-- Customers
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Customers_Username' AND object_id = OBJECT_ID(N'dbo.Customers'))
    CREATE UNIQUE INDEX IX_Customers_Username ON dbo.Customers(Username);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Customers_BankAccountId' AND object_id = OBJECT_ID(N'dbo.Customers'))
    CREATE UNIQUE INDEX IX_Customers_BankAccountId ON dbo.Customers(BankAccountId);

-- ForwardingLetterRequests
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ForwardingLetterRequests_CustomerId' AND object_id = OBJECT_ID(N'dbo.ForwardingLetterRequests'))
    CREATE INDEX IX_ForwardingLetterRequests_CustomerId ON dbo.ForwardingLetterRequests(CustomerId, SubmittedAt DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ForwardingLetterRequests_Status' AND object_id = OBJECT_ID(N'dbo.ForwardingLetterRequests'))
    CREATE INDEX IX_ForwardingLetterRequests_Status ON dbo.ForwardingLetterRequests(Status);

-- GeneratedLetters
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_GeneratedLetters_CustomerId' AND object_id = OBJECT_ID(N'dbo.GeneratedLetters'))
    CREATE INDEX IX_GeneratedLetters_CustomerId ON dbo.GeneratedLetters(CustomerId, GeneratedAt DESC);

-- DownloadLogs
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DownloadLogs_LetterId' AND object_id = OBJECT_ID(N'dbo.DownloadLogs'))
    CREATE INDEX IX_DownloadLogs_LetterId ON dbo.DownloadLogs(LetterId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DownloadLogs_CustomerId' AND object_id = OBJECT_ID(N'dbo.DownloadLogs'))
    CREATE INDEX IX_DownloadLogs_CustomerId ON dbo.DownloadLogs(CustomerId, DownloadedAt DESC);

-- ApprovalLogs
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ApprovalLogs_RequestId' AND object_id = OBJECT_ID(N'dbo.ApprovalLogs'))
    CREATE INDEX IX_ApprovalLogs_RequestId ON dbo.ApprovalLogs(RequestId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ApprovalLogs_AdminId' AND object_id = OBJECT_ID(N'dbo.ApprovalLogs'))
    CREATE INDEX IX_ApprovalLogs_AdminId ON dbo.ApprovalLogs(AdminId);
GO
