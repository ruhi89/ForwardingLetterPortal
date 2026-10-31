-- ============================================================
-- Database: ForwardingLetterPortal
-- ============================================================
CREATE DATABASE ForwardingLetterPortal;
GO

USE ForwardingLetterPortal;
GO

-- ============================================================
-- 1. BankAccounts
--    Pre-loaded from bank core system. Never written by app.
-- ============================================================
CREATE TABLE BankAccounts (
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
GO

-- ============================================================
-- 2. VerificationAttempts
--    Logs every identity check — success and failure.
--    No FK to BankAccounts so failed attempts can still be logged.
-- ============================================================
CREATE TABLE VerificationAttempts (
    AttemptId       INT             IDENTITY(1,1)   PRIMARY KEY,
    AccountNumber   VARCHAR(20)     NOT NULL,
    EmailEntered    VARCHAR(150)    NOT NULL,
    DobEntered      DATE            NOT NULL,
    IsSuccess       BIT             NOT NULL DEFAULT 0,
    FailureReason   VARCHAR(100)    NULL,           -- 'AccountNotFound' | 'EmailMismatch' | 'DobMismatch' | 'AlreadyRegistered'
    IpAddress       VARCHAR(45)     NULL,
    AttemptedAt     DATETIME2       NOT NULL DEFAULT GETUTCDATE()
);
GO

-- ============================================================
-- 3. Customers
--    Created only after successful verification.
--    Real info (name, email, DOB) always read from BankAccounts.
-- ============================================================
CREATE TABLE Customers (
    CustomerId      INT             IDENTITY(1,1)   PRIMARY KEY,
    BankAccountId   INT             NOT NULL UNIQUE,
    Username        VARCHAR(100)    NOT NULL UNIQUE,
    PasswordHash    NVARCHAR(256)   NOT NULL,
    IsActive        BIT             NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NULL,

    CONSTRAINT FK_Customers_BankAccount
        FOREIGN KEY (BankAccountId) REFERENCES BankAccounts(BankAccountId)
);
GO

-- ============================================================
-- 4. ForwardingLetterRequests
--    One row per letter request submitted via the form.
-- ============================================================
CREATE TABLE ForwardingLetterRequests (
    RequestId           INT             IDENTITY(1,1)   PRIMARY KEY,
    CustomerId          INT             NOT NULL,
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
        FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId)
);
GO

-- ============================================================
-- 5. GeneratedLetters
--    PDF metadata — one row per generated file.
-- ============================================================
CREATE TABLE GeneratedLetters (
    LetterId        INT             IDENTITY(1,1)   PRIMARY KEY,
    RequestId       INT             NOT NULL UNIQUE,
    CustomerId      INT             NOT NULL,
    FilePath        NVARCHAR(500)   NOT NULL,
    FileSize        BIGINT          NULL,
    GeneratedAt     DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    DownloadCount   INT             NOT NULL DEFAULT 0,
    LastDownloadAt  DATETIME2       NULL,

    CONSTRAINT FK_GeneratedLetters_Request
        FOREIGN KEY (RequestId) REFERENCES ForwardingLetterRequests(RequestId),
    CONSTRAINT FK_GeneratedLetters_Customer
        FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId)
);
GO

-- ============================================================
-- 6. DownloadLogs
--    Audit trail for every PDF download event.
-- ============================================================
CREATE TABLE DownloadLogs (
    LogId           INT             IDENTITY(1,1)   PRIMARY KEY,
    LetterId        INT             NOT NULL,
    CustomerId      INT             NOT NULL,
    DownloadedAt    DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    IpAddress       VARCHAR(45)     NULL,
    UserAgent       NVARCHAR(500)   NULL,

    CONSTRAINT FK_DownloadLogs_Letter
        FOREIGN KEY (LetterId) REFERENCES GeneratedLetters(LetterId),
    CONSTRAINT FK_DownloadLogs_Customer
        FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId)
);
GO

-- ============================================================
-- 7. Admins  (future — bank officers)
-- ============================================================
CREATE TABLE Admins (
    AdminId         INT             IDENTITY(1,1)   PRIMARY KEY,
    FullName        NVARCHAR(150)   NOT NULL,
    Email           VARCHAR(150)    NOT NULL UNIQUE,
    PasswordHash    NVARCHAR(256)   NOT NULL,
    Role            VARCHAR(50)     NOT NULL DEFAULT 'Officer',     -- 'Officer' | 'Manager' | 'SuperAdmin'
    IsActive        BIT             NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE()
);
GO

-- ============================================================
-- 8. ApprovalLogs  (future — officer approve/reject audit)
-- ============================================================
CREATE TABLE ApprovalLogs (
    ApprovalLogId   INT             IDENTITY(1,1)   PRIMARY KEY,
    RequestId       INT             NOT NULL,
    AdminId         INT             NOT NULL,
    Action          VARCHAR(20)     NOT NULL,       -- 'Approved' | 'Rejected'
    Note            NVARCHAR(500)   NULL,
    ActionAt        DATETIME2       NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_ApprovalLogs_Request
        FOREIGN KEY (RequestId) REFERENCES ForwardingLetterRequests(RequestId),
    CONSTRAINT FK_ApprovalLogs_Admin
        FOREIGN KEY (AdminId) REFERENCES Admins(AdminId)
);
GO

-- Add FK from AccountVerifications back to Admins once Admins table exists
ALTER TABLE VerificationAttempts
    ADD VerifiedByAdminId INT NULL;
ALTER TABLE VerificationAttempts
    ADD CONSTRAINT FK_VerificationAttempts_Admin
        FOREIGN KEY (VerifiedByAdminId) REFERENCES Admins(AdminId);
GO

-- ============================================================
-- Indexes
-- ============================================================

-- BankAccounts
CREATE UNIQUE INDEX IX_BankAccounts_AccountNumber
    ON BankAccounts(AccountNumber);
CREATE INDEX IX_BankAccounts_Email
    ON BankAccounts(Email);

-- VerificationAttempts
CREATE INDEX IX_VerificationAttempts_AccountNumber
    ON VerificationAttempts(AccountNumber, AttemptedAt DESC);

-- Customers
CREATE UNIQUE INDEX IX_Customers_Username
    ON Customers(Username);
CREATE UNIQUE INDEX IX_Customers_BankAccountId
    ON Customers(BankAccountId);

-- ForwardingLetterRequests
CREATE INDEX IX_ForwardingLetterRequests_CustomerId
    ON ForwardingLetterRequests(CustomerId, SubmittedAt DESC);
CREATE INDEX IX_ForwardingLetterRequests_Status
    ON ForwardingLetterRequests(Status);

-- GeneratedLetters
CREATE INDEX IX_GeneratedLetters_CustomerId
    ON GeneratedLetters(CustomerId, GeneratedAt DESC);

-- DownloadLogs
CREATE INDEX IX_DownloadLogs_LetterId
    ON DownloadLogs(LetterId);
CREATE INDEX IX_DownloadLogs_CustomerId
    ON DownloadLogs(CustomerId, DownloadedAt DESC);

-- ApprovalLogs
CREATE INDEX IX_ApprovalLogs_RequestId
    ON ApprovalLogs(RequestId);
CREATE INDEX IX_ApprovalLogs_AdminId
    ON ApprovalLogs(AdminId);
GO