-- ============================================================
-- Database: RetailForwardingLetterPortal  (Agrani Bank PLC.)
-- Officer-driven Retail Banking Forwarding Letter workflow.
-- Re-runnable: safe to execute any number of times.
--   * Missing tables, constraints, indexes and seed rows are created.
--   * Existing objects and data are left untouched.
--
-- Workflow -> table mapping
--   1-2  Officer login / user management ........ Officers
--   3-4  Retail service + purpose ................ RetailServices, ServicePurposes
--   5-7  Customer info, OTP verify, retrieve ..... BankAccounts, OtpVerifications
--   8-10 Generate / letter type / preview+print .. ForwardingLetters
--   11   Record management ....................... ForwardingLetters, LetterPrintLogs, OfficerLoginLogs
-- ============================================================
IF DB_ID(N'RetailForwardingLetterPortal') IS NULL
    CREATE DATABASE RetailForwardingLetterPortal;
GO

USE RetailForwardingLetterPortal;
GO

-- ============================================================
-- 1. Officers  (steps 1-2: login + Super Admin user management)
-- ============================================================
IF OBJECT_ID(N'dbo.Officers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Officers (
        OfficerId           INT             IDENTITY(1,1)   PRIMARY KEY,
        EmployeeId          VARCHAR(30)     NOT NULL UNIQUE,
        FullName            NVARCHAR(150)   NOT NULL,
        Designation         NVARCHAR(100)   NULL,
        BranchName          NVARCHAR(200)   NOT NULL,
        Email               VARCHAR(150)    NOT NULL UNIQUE,
        PasswordHash        NVARCHAR(256)   NOT NULL,
        Role                VARCHAR(20)     NOT NULL DEFAULT 'Officer',   -- 'Officer' | 'SuperAdmin'
        IsActive            BIT             NOT NULL DEFAULT 1,
        FailedLoginCount    INT             NOT NULL DEFAULT 0,
        LockedUntil         DATETIME2       NULL,
        CreatedByOfficerId  INT             NULL,                         -- Super Admin who created the account
        CreatedAt           DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt           DATETIME2       NULL,
        LastLoginAt         DATETIME2       NULL,

        CONSTRAINT CK_Officers_Role CHECK (Role IN ('Officer', 'SuperAdmin')),
        CONSTRAINT FK_Officers_CreatedBy
            FOREIGN KEY (CreatedByOfficerId) REFERENCES dbo.Officers(OfficerId)
    );
END
GO

-- ============================================================
-- 2. OfficerLoginLogs  (audit of every login attempt)
-- ============================================================
IF OBJECT_ID(N'dbo.OfficerLoginLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OfficerLoginLogs (
        LoginLogId      INT             IDENTITY(1,1)   PRIMARY KEY,
        OfficerId       INT             NULL,                       -- NULL when the identifier is unknown
        IdentifierUsed  VARCHAR(150)    NOT NULL,
        IsSuccess       BIT             NOT NULL,
        FailureReason   VARCHAR(100)    NULL,                       -- 'UnknownUser' | 'BadPassword' | 'Locked' | 'Inactive'
        IpAddress       VARCHAR(45)     NULL,
        AttemptedAt     DATETIME2       NOT NULL DEFAULT GETUTCDATE(),

        CONSTRAINT FK_OfficerLoginLogs_Officer
            FOREIGN KEY (OfficerId) REFERENCES dbo.Officers(OfficerId)
    );
END
GO

-- ============================================================
-- 3. RetailServices  (step 3)
-- ============================================================
IF OBJECT_ID(N'dbo.RetailServices', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RetailServices (
        ServiceId       INT             IDENTITY(1,1)   PRIMARY KEY,
        ServiceCode     VARCHAR(30)     NOT NULL UNIQUE,
        ServiceName     NVARCHAR(100)   NOT NULL,
        DisplayOrder    INT             NOT NULL DEFAULT 0,
        IsActive        BIT             NOT NULL DEFAULT 1
    );
END
GO

-- ============================================================
-- 4. ServicePurposes  (step 4: predefined purposes per service)
-- ============================================================
IF OBJECT_ID(N'dbo.ServicePurposes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ServicePurposes (
        PurposeId       INT             IDENTITY(1,1)   PRIMARY KEY,
        ServiceId       INT             NOT NULL,
        PurposeName     NVARCHAR(200)   NOT NULL,
        DisplayOrder    INT             NOT NULL DEFAULT 0,
        IsActive        BIT             NOT NULL DEFAULT 1,

        CONSTRAINT FK_ServicePurposes_Service
            FOREIGN KEY (ServiceId) REFERENCES dbo.RetailServices(ServiceId),
        CONSTRAINT UQ_ServicePurposes UNIQUE (ServiceId, PurposeName)
    );
END
GO

-- ============================================================
-- 4b. LetterTemplates  (steps 4, 8-9: subject + body per
--     Service / Purpose / Letter Type, managed by Super Admin)
--     Placeholders in SubjectText/BodyText, e.g. {{CustomerName}},
--     {{AccountNumber}}, {{BranchName}}, {{ServiceName}},
--     {{PurposeName}}, {{ReferenceNumber}}, {{Date}}.
--     Only one ACTIVE template per (Service, Purpose, LetterType).
-- ============================================================
IF OBJECT_ID(N'dbo.LetterTemplates', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LetterTemplates (
        TemplateId          INT             IDENTITY(1,1)   PRIMARY KEY,
        ServiceId           INT             NOT NULL,
        PurposeId           INT             NOT NULL,
        LetterType          VARCHAR(20)     NOT NULL,           -- 'CustomerCopy' | 'InternalCopy'
        TemplateName        NVARCHAR(150)   NOT NULL,
        SubjectText         NVARCHAR(500)   NOT NULL,
        BodyText            NVARCHAR(MAX)   NOT NULL,
        IsActive            BIT             NOT NULL DEFAULT 1,
        CreatedByOfficerId  INT             NULL,
        CreatedAt           DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
        UpdatedByOfficerId  INT             NULL,
        UpdatedAt           DATETIME2       NULL,

        CONSTRAINT CK_LetterTemplates_Type
            CHECK (LetterType IN ('CustomerCopy', 'InternalCopy')),
        CONSTRAINT FK_LetterTemplates_Service
            FOREIGN KEY (ServiceId) REFERENCES dbo.RetailServices(ServiceId),
        CONSTRAINT FK_LetterTemplates_Purpose
            FOREIGN KEY (PurposeId) REFERENCES dbo.ServicePurposes(PurposeId),
        CONSTRAINT FK_LetterTemplates_CreatedBy
            FOREIGN KEY (CreatedByOfficerId) REFERENCES dbo.Officers(OfficerId),
        CONSTRAINT FK_LetterTemplates_UpdatedBy
            FOREIGN KEY (UpdatedByOfficerId) REFERENCES dbo.Officers(OfficerId)
    );
END
GO

-- ============================================================
-- 5. BankAccounts  (steps 5-7: mirror of core banking records)
--    Pre-loaded from the core system. Never written by the app.
-- ============================================================
IF OBJECT_ID(N'dbo.BankAccounts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BankAccounts (
        BankAccountId   INT             IDENTITY(1,1)   PRIMARY KEY,
        AccountNumber   VARCHAR(20)     NOT NULL UNIQUE,
        FullName        NVARCHAR(150)   NOT NULL,
        FatherName      NVARCHAR(150)   NULL,
        Address         NVARCHAR(500)   NULL,
        Email           VARCHAR(150)    NULL,
        MobileNumber    VARCHAR(20)     NULL,
        DateOfBirth     DATE            NULL,
        BranchName      NVARCHAR(200)   NOT NULL,
        ServiceId       INT             NULL,                       -- product held (Savings, Current, FDR ...)
        OpenedOn        DATE            NULL,
        AccountStatus   VARCHAR(20)     NOT NULL DEFAULT 'Active',  -- 'Active' | 'Dormant' | 'Closed'
        CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),

        CONSTRAINT FK_BankAccounts_Service
            FOREIGN KEY (ServiceId) REFERENCES dbo.RetailServices(ServiceId)
    );
END
GO

-- ============================================================
-- 6. OtpVerifications  (step 6: customer verification + OTP)
--    OtpHash only - the plain OTP is never stored.
-- ============================================================
IF OBJECT_ID(N'dbo.OtpVerifications', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OtpVerifications (
        VerificationId  INT             IDENTITY(1,1)   PRIMARY KEY,
        OfficerId       INT             NOT NULL,
        AccountNumber   VARCHAR(20)     NOT NULL,           -- as entered (no FK so failures are logged)
        BankAccountId   INT             NULL,               -- NULL when the account was not found
        ContactEntered  VARCHAR(150)    NOT NULL,           -- mobile or email typed by the officer
        Channel         VARCHAR(10)     NULL,               -- 'SMS' | 'Email'
        OtpHash         NVARCHAR(256)   NULL,
        OtpExpiresAt    DATETIME2       NULL,
        AttemptCount    INT             NOT NULL DEFAULT 0,
        Status          VARCHAR(20)     NOT NULL DEFAULT 'Pending',
                        -- 'Pending' | 'Verified' | 'Failed' | 'Expired' | 'NotMatched'
        FailureReason   VARCHAR(100)    NULL,               -- 'AccountNotFound' | 'ContactMismatch' | 'WrongOtp' | 'Expired'
        IpAddress       VARCHAR(45)     NULL,
        CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
        VerifiedAt      DATETIME2       NULL,

        CONSTRAINT CK_OtpVerifications_Status
            CHECK (Status IN ('Pending', 'Verified', 'Failed', 'Expired', 'NotMatched')),
        CONSTRAINT FK_OtpVerifications_Officer
            FOREIGN KEY (OfficerId) REFERENCES dbo.Officers(OfficerId),
        CONSTRAINT FK_OtpVerifications_BankAccount
            FOREIGN KEY (BankAccountId) REFERENCES dbo.BankAccounts(BankAccountId)
    );
END
GO

-- ============================================================
-- 7. ForwardingLetters  (steps 8-11: generated letter + record)
--    A letter can only be generated from a Verified OTP session.
-- ============================================================
IF OBJECT_ID(N'dbo.ForwardingLetters', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ForwardingLetters (
        LetterId            INT             IDENTITY(1,1)   PRIMARY KEY,
        ReferenceNumber     VARCHAR(40)     NOT NULL UNIQUE,    -- e.g. AGR/RET/2026/000123
        LetterType          VARCHAR(20)     NOT NULL,           -- 'CustomerCopy' | 'InternalCopy'
        VerificationId      INT             NOT NULL,
        OfficerId           INT             NOT NULL,
        BankAccountId       INT             NOT NULL,
        ServiceId           INT             NOT NULL,
        PurposeId           INT             NOT NULL,
        TemplateId          INT             NULL,               -- template used (NULL for legacy rows)
        SubjectSnapshot     NVARCHAR(500)   NOT NULL,           -- exact rendered subject
        BodySnapshot        NVARCHAR(MAX)   NOT NULL,           -- exact rendered body

        -- Snapshot of customer data at generation time (letter must never change later)
        CustomerName        NVARCHAR(150)   NOT NULL,
        AccountNumber       VARCHAR(20)     NOT NULL,
        CustomerAddress     NVARCHAR(500)   NULL,
        CustomerMobile      VARCHAR(20)     NULL,
        CustomerEmail       VARCHAR(150)    NULL,
        BranchName          NVARCHAR(200)   NOT NULL,
        ServiceName         NVARCHAR(100)   NOT NULL,
        PurposeName         NVARCHAR(200)   NOT NULL,

        AddressedTo         NVARCHAR(200)   NOT NULL DEFAULT N'The Managing Director & CEO, Agrani Bank PLC.',
        Enclosures          NVARCHAR(1000)  NULL,               -- supporting documents; InternalCopy only
        Status              VARCHAR(20)     NOT NULL DEFAULT 'Generated',   -- 'Generated' | 'Printed' | 'Cancelled'
        GeneratedAt         DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
        FirstPrintedAt      DATETIME2       NULL,
        PrintCount          INT             NOT NULL DEFAULT 0,

        CONSTRAINT CK_ForwardingLetters_Type
            CHECK (LetterType IN ('CustomerCopy', 'InternalCopy')),
        CONSTRAINT CK_ForwardingLetters_Status
            CHECK (Status IN ('Generated', 'Printed', 'Cancelled')),
        CONSTRAINT CK_ForwardingLetters_Enclosures
            CHECK (LetterType = 'InternalCopy' OR Enclosures IS NULL),
        CONSTRAINT FK_ForwardingLetters_Verification
            FOREIGN KEY (VerificationId) REFERENCES dbo.OtpVerifications(VerificationId),
        CONSTRAINT FK_ForwardingLetters_Officer
            FOREIGN KEY (OfficerId) REFERENCES dbo.Officers(OfficerId),
        CONSTRAINT FK_ForwardingLetters_BankAccount
            FOREIGN KEY (BankAccountId) REFERENCES dbo.BankAccounts(BankAccountId),
        CONSTRAINT FK_ForwardingLetters_Service
            FOREIGN KEY (ServiceId) REFERENCES dbo.RetailServices(ServiceId),
        CONSTRAINT FK_ForwardingLetters_Purpose
            FOREIGN KEY (PurposeId) REFERENCES dbo.ServicePurposes(PurposeId),
        CONSTRAINT FK_ForwardingLetters_Template
            FOREIGN KEY (TemplateId) REFERENCES dbo.LetterTemplates(TemplateId)
    );
END
GO

-- Migration: add template columns to a table created before they existed.
IF COL_LENGTH('dbo.ForwardingLetters', 'TemplateId') IS NULL
    ALTER TABLE dbo.ForwardingLetters ADD TemplateId INT NULL;
IF COL_LENGTH('dbo.ForwardingLetters', 'SubjectSnapshot') IS NULL
    ALTER TABLE dbo.ForwardingLetters ADD SubjectSnapshot NVARCHAR(500) NOT NULL
        CONSTRAINT DF_FL_SubjectSnapshot DEFAULT '';
IF COL_LENGTH('dbo.ForwardingLetters', 'BodySnapshot') IS NULL
    ALTER TABLE dbo.ForwardingLetters ADD BodySnapshot NVARCHAR(MAX) NOT NULL
        CONSTRAINT DF_FL_BodySnapshot DEFAULT '';
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys
               WHERE name = N'FK_ForwardingLetters_Template'
                 AND parent_object_id = OBJECT_ID(N'dbo.ForwardingLetters'))
    ALTER TABLE dbo.ForwardingLetters
        ADD CONSTRAINT FK_ForwardingLetters_Template
            FOREIGN KEY (TemplateId) REFERENCES dbo.LetterTemplates(TemplateId);
GO

-- ============================================================
-- 8. LetterPrintLogs  (step 10: every preview / print / reprint)
-- ============================================================
IF OBJECT_ID(N'dbo.LetterPrintLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LetterPrintLogs (
        PrintLogId      INT             IDENTITY(1,1)   PRIMARY KEY,
        LetterId        INT             NOT NULL,
        OfficerId       INT             NOT NULL,
        Action          VARCHAR(10)     NOT NULL,           -- 'Preview' | 'Print'
        IpAddress       VARCHAR(45)     NULL,
        ActionAt        DATETIME2       NOT NULL DEFAULT GETUTCDATE(),

        CONSTRAINT CK_LetterPrintLogs_Action CHECK (Action IN ('Preview', 'Print')),
        CONSTRAINT FK_LetterPrintLogs_Letter
            FOREIGN KEY (LetterId) REFERENCES dbo.ForwardingLetters(LetterId),
        CONSTRAINT FK_LetterPrintLogs_Officer
            FOREIGN KEY (OfficerId) REFERENCES dbo.Officers(OfficerId)
    );
END
GO

-- ============================================================
-- Indexes
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_OfficerLoginLogs_Officer' AND object_id = OBJECT_ID(N'dbo.OfficerLoginLogs'))
    CREATE INDEX IX_OfficerLoginLogs_Officer ON dbo.OfficerLoginLogs(OfficerId, AttemptedAt DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ServicePurposes_Service' AND object_id = OBJECT_ID(N'dbo.ServicePurposes'))
    CREATE INDEX IX_ServicePurposes_Service ON dbo.ServicePurposes(ServiceId, DisplayOrder);
-- One active template per Service + Purpose + Letter Type
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_LetterTemplates_Active' AND object_id = OBJECT_ID(N'dbo.LetterTemplates'))
    CREATE UNIQUE INDEX UX_LetterTemplates_Active
        ON dbo.LetterTemplates(ServiceId, PurposeId, LetterType) WHERE IsActive = 1;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ForwardingLetters_Template' AND object_id = OBJECT_ID(N'dbo.ForwardingLetters'))
    CREATE INDEX IX_ForwardingLetters_Template ON dbo.ForwardingLetters(TemplateId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BankAccounts_Mobile' AND object_id = OBJECT_ID(N'dbo.BankAccounts'))
    CREATE INDEX IX_BankAccounts_Mobile ON dbo.BankAccounts(MobileNumber);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BankAccounts_Email' AND object_id = OBJECT_ID(N'dbo.BankAccounts'))
    CREATE INDEX IX_BankAccounts_Email ON dbo.BankAccounts(Email);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_OtpVerifications_Account' AND object_id = OBJECT_ID(N'dbo.OtpVerifications'))
    CREATE INDEX IX_OtpVerifications_Account ON dbo.OtpVerifications(AccountNumber, CreatedAt DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_OtpVerifications_Officer' AND object_id = OBJECT_ID(N'dbo.OtpVerifications'))
    CREATE INDEX IX_OtpVerifications_Officer ON dbo.OtpVerifications(OfficerId, CreatedAt DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ForwardingLetters_Officer' AND object_id = OBJECT_ID(N'dbo.ForwardingLetters'))
    CREATE INDEX IX_ForwardingLetters_Officer ON dbo.ForwardingLetters(OfficerId, GeneratedAt DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ForwardingLetters_Account' AND object_id = OBJECT_ID(N'dbo.ForwardingLetters'))
    CREATE INDEX IX_ForwardingLetters_Account ON dbo.ForwardingLetters(AccountNumber, GeneratedAt DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ForwardingLetters_GeneratedAt' AND object_id = OBJECT_ID(N'dbo.ForwardingLetters'))
    CREATE INDEX IX_ForwardingLetters_GeneratedAt ON dbo.ForwardingLetters(GeneratedAt DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_LetterPrintLogs_Letter' AND object_id = OBJECT_ID(N'dbo.LetterPrintLogs'))
    CREATE INDEX IX_LetterPrintLogs_Letter ON dbo.LetterPrintLogs(LetterId, ActionAt DESC);
GO

-- ============================================================
-- Reference number sequence  (AGR/RET/<year>/<6-digit seq>)
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.sequences WHERE name = N'LetterReferenceSeq')
    CREATE SEQUENCE dbo.LetterReferenceSeq AS INT START WITH 1 INCREMENT BY 1;
GO

-- ============================================================
-- Seed: retail services
-- ============================================================
INSERT INTO dbo.RetailServices (ServiceCode, ServiceName, DisplayOrder)
SELECT v.ServiceCode, v.ServiceName, v.DisplayOrder
FROM (VALUES
    ('SAVINGS',       N'Savings Account', 1),
    ('CURRENT',       N'Current Account', 2),
    ('FDR',           N'FDR',             3),
    ('PERSONAL_LOAN', N'Personal Loan',   4),
    ('DEBIT_CARD',    N'Debit Card',      5)
) AS v(ServiceCode, ServiceName, DisplayOrder)
WHERE NOT EXISTS (SELECT 1 FROM dbo.RetailServices r WHERE r.ServiceCode = v.ServiceCode);
GO

-- ============================================================
-- Seed: predefined purposes per service (edit to match bank policy)
-- ============================================================
INSERT INTO dbo.ServicePurposes (ServiceId, PurposeName, DisplayOrder)
SELECT s.ServiceId, p.PurposeName, p.DisplayOrder
FROM (VALUES
    ('SAVINGS', N'Account Opening',               1),
    ('SAVINGS', N'Account Closure',               2),
    ('SAVINGS', N'Nominee / Information Update',  3),
    ('SAVINGS', N'Dormant Account Reactivation',  4),
    ('CURRENT', N'Account Opening',               1),
    ('CURRENT', N'Account Closure',               2),
    ('CURRENT', N'Cheque Book Issuance',          3),
    ('CURRENT', N'Information Update',            4),
    ('FDR',     N'New FDR Opening',               1),
    ('FDR',     N'FDR Encashment',                2),
    ('FDR',     N'FDR Renewal',                   3),
    ('FDR',     N'Loan Against FDR (Lien)',       4),
    ('PERSONAL_LOAN', N'New Loan Application',    1),
    ('PERSONAL_LOAN', N'Loan Settlement / Closure', 2),
    ('PERSONAL_LOAN', N'Loan Rescheduling',       3),
    ('DEBIT_CARD', N'New Card Issuance',          1),
    ('DEBIT_CARD', N'Card Replacement',           2),
    ('DEBIT_CARD', N'Card Block / Unblock',       3),
    ('DEBIT_CARD', N'PIN Reissue',                4)
) AS p(ServiceCode, PurposeName, DisplayOrder)
JOIN dbo.RetailServices s ON s.ServiceCode = p.ServiceCode
WHERE NOT EXISTS (SELECT 1 FROM dbo.ServicePurposes sp
                  WHERE sp.ServiceId = s.ServiceId AND sp.PurposeName = p.PurposeName);
GO

-- ============================================================
-- Seed: sample bank accounts (TEST DATA - replace with the core-banking load).
-- OTPs are e-mailed to the Email below, so use an inbox you can read.
-- ============================================================
INSERT INTO dbo.BankAccounts
    (AccountNumber, FullName, FatherName, Address, Email, MobileNumber, DateOfBirth, BranchName, ServiceId, OpenedOn, AccountStatus)
SELECT v.AccountNumber, v.FullName, v.FatherName, v.Address, v.Email, v.MobileNumber, v.DateOfBirth,
       v.BranchName, s.ServiceId, v.OpenedOn, v.AccountStatus
FROM (VALUES
    ('0200012345678', N'Md. Rahim Uddin',     N'Md. Abdul Karim',  N'House 12, Road 5, Dhanmondi, Dhaka-1205',        'rahim.uddin@example.com',  '01711000001', '1985-03-14', N'Dilkusha Corporate Branch', 'SAVINGS',       '2015-06-10', 'Active'),
    ('0200012345679', N'Fatema Begum',        N'Md. Jalal Ahmed',  N'45 Station Road, Agrabad, Chattogram-4100',      'fatema.begum@example.com', '01811000002', '1990-11-02', N'Agrabad Branch',            'SAVINGS',       '2018-01-22', 'Active'),
    ('0200012345680', N'Kamal Hossain',       N'Md. Nurul Islam',  N'Sector 7, Uttara, Dhaka-1230',                   'kamal.hossain@example.com','01911000003', '1978-07-30', N'Uttara Branch',             'CURRENT',       '2012-09-05', 'Active'),
    ('0200012345681', N'Nasrin Akter',        N'Md. Abul Kashem',  N'Zindabazar, Sylhet-3100',                        'nasrin.akter@example.com', '01611000004', '1992-05-19', N'Sylhet Main Branch',        'FDR',           '2020-02-17', 'Active'),
    ('0200012345682', N'Sabbir Ahmed',        N'Md. Shafiqul Alam',N'Rajpara, Boalia, Rajshahi-6100',                 'sabbir.ahmed@example.com', '01511000005', '1988-12-08', N'Rajshahi Branch',           'PERSONAL_LOAN', '2021-08-30', 'Active'),
    ('0200012345683', N'Ruma Chowdhury',      N'Md. Anwar Hossain',N'Kandirpar, Cumilla-3500',                        'ruma.chowdhury@example.com','01311000006','1995-01-25', N'Cumilla Branch',            'DEBIT_CARD',    '2022-04-12', 'Active'),
    ('0200012345684', N'Dormant Test Account',N'Test Father',      N'Test Address, Dhaka',                            'dormant.test@example.com', '01700000007', '1980-01-01', N'Dilkusha Corporate Branch', 'SAVINGS',       '2010-01-01', 'Dormant')
) AS v(AccountNumber, FullName, FatherName, Address, Email, MobileNumber, DateOfBirth, BranchName, ServiceCode, OpenedOn, AccountStatus)
JOIN dbo.RetailServices s ON s.ServiceCode = v.ServiceCode
WHERE NOT EXISTS (SELECT 1 FROM dbo.BankAccounts b WHERE b.AccountNumber = v.AccountNumber);
GO

-- ============================================================
-- Seed: default letter templates - one active template per
-- purpose for each letter type (Customer Copy / Internal Copy).
-- Super Admin can edit these or add/activate replacements.
-- Existing active templates are never overwritten.
-- ============================================================
DECLARE @nl NVARCHAR(4) = CHAR(13) + CHAR(10);

INSERT INTO dbo.LetterTemplates (ServiceId, PurposeId, LetterType, TemplateName, SubjectText, BodyText, IsActive)
SELECT sp.ServiceId, sp.PurposeId, t.LetterType,
       rs.ServiceName + N' - ' + sp.PurposeName + N' (' + t.TypeLabel + N')',
       t.SubjectText,
       t.BodyText,
       1
FROM dbo.ServicePurposes sp
JOIN dbo.RetailServices rs ON rs.ServiceId = sp.ServiceId
CROSS JOIN (VALUES
    ('CustomerCopy', N'Customer Copy',
     N'Request for {{PurposeName}} - {{ServiceName}} (A/C No. {{AccountNumber}})',
     N'Dear Sir,' + @nl + @nl +
     N'With due respect, I am pleased to forward the application of {{CustomerName}}, son/daughter of {{FatherName}}, residing at {{CustomerAddress}}, holder of {{ServiceName}} A/C No. {{AccountNumber}} maintained with {{BranchName}}, regarding {{PurposeName}}.' + @nl + @nl +
     N'The customer''s identity has been verified through OTP against the bank''s records.' + @nl + @nl +
     N'You are therefore kindly requested to consider the matter and take necessary action at your earliest convenience.' + @nl + @nl +
     N'Thanking you,'),
    ('InternalCopy', N'Internal Forwarding Copy',
     N'Forwarding of {{PurposeName}} request - {{ServiceName}}, A/C No. {{AccountNumber}} ({{CustomerName}})',
     N'Dear Sir,' + @nl + @nl +
     N'This is to inform you that {{CustomerName}}, son/daughter of {{FatherName}}, holder of {{ServiceName}} A/C No. {{AccountNumber}} at {{BranchName}}, has requested {{PurposeName}}. Ref: {{ReferenceNumber}}, dated {{Date}}.' + @nl + @nl +
     N'The customer was verified through OTP (registered contact: {{CustomerEmail}}). The relevant supporting documents are enclosed for your kind perusal.' + @nl + @nl +
     N'Forwarded for your kind consideration and necessary approval.' + @nl + @nl +
     N'Yours faithfully,')
) AS t(LetterType, TypeLabel, SubjectText, BodyText)
WHERE NOT EXISTS (SELECT 1 FROM dbo.LetterTemplates lt
                  WHERE lt.ServiceId = sp.ServiceId AND lt.PurposeId = sp.PurposeId
                    AND lt.LetterType = t.LetterType AND lt.IsActive = 1);
GO

-- NOTE: the first SuperAdmin must be inserted by the app/setup script
-- (PasswordHash has to come from the app's hasher), e.g.:
--   INSERT INTO dbo.Officers (EmployeeId, FullName, BranchName, Email, PasswordHash, Role)
--   VALUES ('SA0001', N'Super Admin', N'Head Office', 'admin@agranibank.example', '<hash>', 'SuperAdmin');
GO
