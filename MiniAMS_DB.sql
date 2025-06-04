USE master;
GO

-- Drop database if it exists to ensure a clean start
IF DB_ID('MiniAMS_DB') IS NOT NULL
BEGIN
    ALTER DATABASE MiniAMS_DB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE MiniAMS_DB;
END
GO

-- Create the database
CREATE DATABASE MiniAMS_DB;
GO

-- Use the newly created database
USE MiniAMS_DB;
GO

-- Create Roles Table
CREATE TABLE Roles (
    RoleId INT PRIMARY KEY IDENTITY(1,1),
    RoleName NVARCHAR(50) NOT NULL UNIQUE
);

-- Insert initial roles
INSERT INTO Roles (RoleName) VALUES ('Admin'), ('Accountant'), ('Viewer');


-- Create Users Table
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(256) NOT NULL UNIQUE, 
    Email NVARCHAR(256) NOT NULL,           
    PasswordHash NVARCHAR(MAX),             
    NormalizedUserName NVARCHAR(256) UNIQUE,
    NormalizedEmail NVARCHAR(256) UNIQUE,   
    CreatedAt DATETIME DEFAULT GETDATE()
);


-- Create UserRoles Table
CREATE TABLE UserRoles (
    UserRoleId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    RoleId INT NOT NULL,
    CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId) REFERENCES Roles(RoleId),
    CONSTRAINT UQ_UserRoles UNIQUE (UserId, RoleId) 
);


-- Create Accounts Table (Chart of Accounts)
CREATE TABLE Accounts (
    AccountId INT PRIMARY KEY IDENTITY(1,1),
    AccountName NVARCHAR(255) NOT NULL,
    AccountNumber NVARCHAR(50) UNIQUE,
    AccountType NVARCHAR(50) NOT NULL,
    ParentAccountId INT NULL,         
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Accounts_ParentAccount FOREIGN KEY (ParentAccountId) REFERENCES Accounts(AccountId)
);


-- Create Vouchers Table (Main transaction records)
CREATE TABLE Vouchers (
    VoucherId INT PRIMARY KEY IDENTITY(1,1),
    VoucherNo NVARCHAR(50) NOT NULL UNIQUE,  
    VoucherDate DATE NOT NULL,
    VoucherType NVARCHAR(20) NOT NULL,       
    Description NVARCHAR(MAX),
    TotalDebit DECIMAL(18, 2) NOT NULL,
    TotalCredit DECIMAL(18, 2) NOT NULL,
    CreatedByUserId INT,                     
    CreatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT CK_Vouchers_DebitCreditBalance CHECK (TotalDebit = TotalCredit),
    CONSTRAINT FK_Vouchers_Users FOREIGN KEY (CreatedByUserId) REFERENCES Users(UserId)
);


-- Create VoucherDetails Table (Individual entries for each voucher)
CREATE TABLE VoucherDetails (
    VoucherDetailId INT PRIMARY KEY IDENTITY(1,1),
    VoucherId INT NOT NULL,
    AccountId INT NOT NULL,
    Debit DECIMAL(18, 2) DEFAULT 0,
    Credit DECIMAL(18, 2) DEFAULT 0,
    Remarks NVARCHAR(MAX),
    CONSTRAINT FK_VoucherDetails_Vouchers FOREIGN KEY (VoucherId) REFERENCES Vouchers(VoucherId),
    CONSTRAINT FK_VoucherDetails_Accounts FOREIGN KEY (AccountId) REFERENCES Accounts(AccountId),
    CONSTRAINT CK_VoucherDetails_DebitOrCredit CHECK ((Debit > 0 AND Credit = 0) OR (Credit > 0 AND Debit = 0) OR (Debit = 0 AND Credit = 0)),
    CONSTRAINT CK_VoucherDetails_PositiveAmount CHECK (Debit >= 0 AND Credit >= 0)
);


-- Create User-Defined Table Type for passing multiple voucher details to stored procedures
CREATE TYPE VoucherDetailType AS TABLE
(
    AccountId INT NOT NULL,
    Debit DECIMAL(18, 2) DEFAULT 0,
    Credit DECIMAL(18, 2) DEFAULT 0,
    Remarks NVARCHAR(MAX)
);


----------------------------------------------------------------------------------------------------
-- Stored Procedures --
----------------------------------------------------------------------------------------------------

-- PROCEDURE: sp_ManageChartOfAccounts
IF OBJECT_ID('sp_ManageChartOfAccounts', 'P') IS NOT NULL DROP PROCEDURE sp_ManageChartOfAccounts;
GO
CREATE PROCEDURE sp_ManageChartOfAccounts
    @Action NVARCHAR(10),      
    @AccountId INT = NULL,
    @AccountName NVARCHAR(255) = NULL,
    @AccountNumber NVARCHAR(50) = NULL,
    @AccountType NVARCHAR(50) = NULL,
    @ParentAccountId INT = NULL,
    @IsActive BIT = NULL,
    @OutputMessage NVARCHAR(255) OUTPUT 
AS
BEGIN
    SET NOCOUNT ON;
    SET @OutputMessage = ''; 

    IF @Action = 'CREATE'
    BEGIN
        IF EXISTS (SELECT 1 FROM Accounts WHERE AccountNumber = @AccountNumber)
        BEGIN
            SET @OutputMessage = 'Account with this number already exists.';
            RETURN;
        END

        INSERT INTO Accounts (AccountName, AccountNumber, AccountType, ParentAccountId, IsActive)
        VALUES (@AccountName, @AccountNumber, @AccountType, @ParentAccountId, ISNULL(@IsActive, 1));

        SET @AccountId = SCOPE_IDENTITY(); 
        SET @OutputMessage = 'Account created successfully. AccountId: ' + CAST(@AccountId AS NVARCHAR(10));
    END
    ELSE IF @Action = 'UPDATE'
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM Accounts WHERE AccountId = @AccountId)
        BEGIN
            SET @OutputMessage = 'Account not found for update.';
            RETURN;
        END

        UPDATE Accounts
        SET
            AccountName = ISNULL(@AccountName, AccountName),
            AccountNumber = ISNULL(@AccountNumber, AccountNumber),
            AccountType = ISNULL(@AccountType, AccountType),
            ParentAccountId = @ParentAccountId,
            IsActive = ISNULL(@IsActive, IsActive),
            UpdatedAt = GETDATE()
        WHERE AccountId = @AccountId;

        SET @OutputMessage = 'Account updated successfully.';
    END
    ELSE IF @Action = 'DELETE'
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM Accounts WHERE AccountId = @AccountId)
        BEGIN
            SET @OutputMessage = 'Account not found for deletion.';
            RETURN;
        END

        -- Prevent deletion if account is used in voucher details
        IF EXISTS (SELECT 1 FROM VoucherDetails WHERE AccountId = @AccountId)
        BEGIN
            SET @OutputMessage = 'Cannot delete account: it is used in voucher details.';
            RETURN;
        END

        DELETE FROM Accounts WHERE AccountId = @AccountId;
        SET @OutputMessage = 'Account deleted successfully.';
    END
    ELSE IF @Action = 'GET'
    BEGIN
        IF @AccountId IS NOT NULL
        BEGIN
            SELECT AccountId, AccountName, AccountNumber, AccountType, ParentAccountId, IsActive, CreatedAt, UpdatedAt
            FROM Accounts
            WHERE AccountId = @AccountId;
        END
        ELSE
        BEGIN
            SELECT AccountId, AccountName, AccountNumber, AccountType, ParentAccountId, IsActive, CreatedAt, UpdatedAt
            FROM Accounts
            ORDER BY AccountType, AccountName;
        END
        SET @OutputMessage = 'Accounts retrieved.';
    END
    ELSE
    BEGIN
        SET @OutputMessage = 'Invalid action specified.';
    END
END;
GO

-- PROCEDURE: sp_SaveVoucher
IF OBJECT_ID('sp_SaveVoucher', 'P') IS NOT NULL DROP PROCEDURE sp_SaveVoucher;
GO
CREATE PROCEDURE sp_SaveVoucher
    @VoucherId INT = NULL OUTPUT,
    @VoucherNo NVARCHAR(50),
    @VoucherDate DATE,
    @VoucherType NVARCHAR(20),
    @Description NVARCHAR(MAX) = NULL,
    @CreatedByUserId INT,
    @VoucherDetails [dbo].[VoucherDetailType] READONLY, 
    @OutputMessage NVARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @OutputMessage = '';
    DECLARE @TotalDebit DECIMAL(18, 2);
    DECLARE @TotalCredit DECIMAL(18, 2);

    SELECT @TotalDebit = ISNULL(SUM(Debit), 0),
           @TotalCredit = ISNULL(SUM(Credit), 0)
    FROM @VoucherDetails;

    IF @TotalDebit <> @TotalCredit
    BEGIN
        SET @OutputMessage = 'Debit and Credit totals must balance.';
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @VoucherId IS NULL -- New Voucher
        BEGIN
            IF EXISTS (SELECT 1 FROM Vouchers WHERE VoucherNo = @VoucherNo)
            BEGIN
                SET @OutputMessage = 'Voucher number already exists.';
                ROLLBACK TRANSACTION;
                RETURN;
            END

            INSERT INTO Vouchers (VoucherNo, VoucherDate, VoucherType, Description, TotalDebit, TotalCredit, CreatedByUserId)
            VALUES (@VoucherNo, @VoucherDate, @VoucherType, @Description, @TotalDebit, @TotalCredit, @CreatedByUserId);

            SET @VoucherId = SCOPE_IDENTITY(); 
            SET @OutputMessage = 'Voucher created successfully.';
        END
        ELSE 
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM Vouchers WHERE VoucherId = @VoucherId)
            BEGIN
                SET @OutputMessage = 'Voucher not found for update.';
                ROLLBACK TRANSACTION;
                RETURN;
            END

     
            DELETE FROM VoucherDetails WHERE VoucherId = @VoucherId;

            UPDATE Vouchers
            SET
                VoucherNo = @VoucherNo,
                VoucherDate = @VoucherDate,
                VoucherType = @VoucherType,
                Description = ISNULL(@Description, Description),
                TotalDebit = @TotalDebit,
                TotalCredit = @TotalCredit
            WHERE VoucherId = @VoucherId;

            SET @OutputMessage = 'Voucher updated successfully.';
        END

 
        INSERT INTO VoucherDetails (VoucherId, AccountId, Debit, Credit, Remarks)
        SELECT @VoucherId, AccountId, Debit, Credit, Remarks
        FROM @VoucherDetails;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SET @OutputMessage = ERROR_MESSAGE();
    END CATCH
END;
GO

-- PROCEDURE: spGetAccountById
IF OBJECT_ID('spGetAccountById', 'P') IS NOT NULL DROP PROCEDURE spGetAccountById;
GO
CREATE PROCEDURE spGetAccountById
    @AccountId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        A.AccountId,
        A.AccountName,
        A.AccountNumber,
        A.AccountType,
        A.ParentAccountId,
        P.AccountName AS ParentAccountName,
        A.IsActive,
        A.CreatedAt,
        A.UpdatedAt
    FROM Accounts A
    LEFT JOIN Accounts P ON A.ParentAccountId = P.AccountId 
    WHERE A.AccountId = @AccountId;
END
GO

-- PROCEDURE: spUpdateAccount
IF OBJECT_ID('spUpdateAccount', 'P') IS NOT NULL DROP PROCEDURE spUpdateAccount;
GO
CREATE PROCEDURE spUpdateAccount
    @AccountId INT,
    @AccountName NVARCHAR(100),
    @AccountNumber NVARCHAR(50),
    @AccountType NVARCHAR(50),
    @ParentAccountId INT = NULL, 
    @IsActive BIT,
    @UpdatedAt DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Accounts
    SET
        AccountName = @AccountName,
        AccountNumber = @AccountNumber,
        AccountType = @AccountType,
        ParentAccountId = @ParentAccountId,
        IsActive = @IsActive,
        UpdatedAt = @UpdatedAt
    WHERE AccountId = @AccountId;
END
GO

-- PROCEDURE: spGetAllVouchers
IF OBJECT_ID('spGetAllVouchers', 'P') IS NOT NULL DROP PROCEDURE spGetAllVouchers;
GO
CREATE PROCEDURE spGetAllVouchers
AS
BEGIN
    SELECT
        VoucherId,
        VoucherNo,
        VoucherDate,
        VoucherType,
        Description,
        TotalDebit,
        TotalCredit,
        CreatedByUserId,
        CreatedAt
    FROM Vouchers;
END;
GO

-- PROCEDURE: spGetVoucherById
IF OBJECT_ID('spGetVoucherById', 'P') IS NOT NULL DROP PROCEDURE spGetVoucherById;
GO
CREATE PROCEDURE spGetVoucherById
    @VoucherId INT
AS
BEGIN
    SELECT
        VoucherId,
        VoucherNo,
        VoucherDate,
        VoucherType,
        Description,
        TotalDebit,
        TotalCredit,
        CreatedByUserId,
        CreatedAt
    FROM Vouchers
    WHERE VoucherId = @VoucherId;
END;
GO

-- PROCEDURE: spCreateVoucher
IF OBJECT_ID('spCreateVoucher', 'P') IS NOT NULL DROP PROCEDURE spCreateVoucher;
GO
CREATE PROCEDURE spCreateVoucher
    @VoucherNo NVARCHAR(50),
    @VoucherType NVARCHAR(20),
    @VoucherDate DATE,
    @Description NVARCHAR(MAX),
    @TotalDebit DECIMAL(18, 2),
    @TotalCredit DECIMAL(18, 2),
    @CreatedByUserId INT,
    @OutputMessage NVARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        INSERT INTO Vouchers (VoucherNo, VoucherType, VoucherDate, Description, TotalDebit, TotalCredit, CreatedByUserId, CreatedAt)
        VALUES (@VoucherNo, @VoucherType, @VoucherDate, @Description, @TotalDebit, @TotalCredit, @CreatedByUserId, GETDATE());

        SET @OutputMessage = 'Voucher created successfully!';
    END TRY
    BEGIN CATCH
        SET @OutputMessage = 'Error: ' + ERROR_MESSAGE();
    END CATCH
END;
GO

-- PROCEDURE: spUpdateVoucher
IF OBJECT_ID('spUpdateVoucher', 'P') IS NOT NULL DROP PROCEDURE spUpdateVoucher;
GO
CREATE PROCEDURE spUpdateVoucher
    @VoucherId INT,
    @VoucherNo NVARCHAR(50),
    @VoucherType NVARCHAR(20),
    @VoucherDate DATE,
    @Description NVARCHAR(MAX),
    @TotalDebit DECIMAL(18, 2),
    @TotalCredit DECIMAL(18, 2),
    @OutputMessage NVARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE Vouchers
        SET
            VoucherNo = @VoucherNo,
            VoucherType = @VoucherType,
            VoucherDate = @VoucherDate,
            Description = @Description,
            TotalDebit = @TotalDebit,
            TotalCredit = @TotalCredit
        WHERE VoucherId = @VoucherId;

        IF @@ROWCOUNT = 0
            SET @OutputMessage = 'Voucher not found or no changes made.';
        ELSE
            SET @OutputMessage = 'Voucher updated successfully!';
    END TRY
    BEGIN CATCH
        SET @OutputMessage = 'Error: ' + ERROR_MESSAGE();
    END CATCH
END;
GO

-- PROCEDURE: spDeleteVoucher
IF OBJECT_ID('spDeleteVoucher', 'P') IS NOT NULL DROP PROCEDURE spDeleteVoucher;
GO
CREATE PROCEDURE spDeleteVoucher
    @VoucherId INT,
    @OutputMessage NVARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        DELETE FROM Vouchers
        WHERE VoucherId = @VoucherId;

        IF @@ROWCOUNT = 0
            SET @OutputMessage = 'Voucher not found.';
        ELSE
            SET @OutputMessage = 'Voucher deleted successfully!';
    END TRY
    BEGIN CATCH
        SET @OutputMessage = 'Error: ' + ERROR_MESSAGE();
    END CATCH
END;
GO

-- PROCEDURE: spGetVoucherDetailsByVoucherId
IF OBJECT_ID('spGetVoucherDetailsByVoucherId', 'P') IS NOT NULL DROP PROCEDURE spGetVoucherDetailsByVoucherId;
GO
CREATE PROCEDURE spGetVoucherDetailsByVoucherId
    @VoucherId INT
AS
BEGIN
    SELECT
        vd.VoucherDetailId,
        vd.VoucherId,
        vd.AccountId,
        a.AccountName,
        vd.Debit,
        vd.Credit,
        vd.Remarks
    FROM VoucherDetails vd
    INNER JOIN Accounts a ON vd.AccountId = a.AccountId
    WHERE vd.VoucherId = @VoucherId
    ORDER BY vd.VoucherDetailId;
END;
GO

-- PROCEDURE: spGetVoucherDetailById
IF OBJECT_ID('spGetVoucherDetailById', 'P') IS NOT NULL DROP PROCEDURE spGetVoucherDetailById;
GO
CREATE PROCEDURE spGetVoucherDetailById
    @VoucherDetailId INT
AS
BEGIN
    SELECT
        vd.VoucherDetailId,
        vd.VoucherId,
        vd.AccountId,
        a.AccountName, 
        vd.Debit,
        vd.Credit,
        vd.Remarks
    FROM VoucherDetails vd
    INNER JOIN Accounts a ON vd.AccountId = a.AccountId
    WHERE vd.VoucherDetailId = @VoucherDetailId;
END;
GO

-- PROCEDURE: spCreateVoucherDetail (FIXED)
IF OBJECT_ID('spCreateVoucherDetail', 'P') IS NOT NULL DROP PROCEDURE spCreateVoucherDetail;
GO
CREATE PROCEDURE spCreateVoucherDetail
    @VoucherId INT,
    @AccountId INT,
    @Debit DECIMAL(18, 2),
    @Credit DECIMAL(18, 2),
    @Remarks NVARCHAR(MAX),
    @OutputMessage NVARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF (@Debit > 0 AND @Credit > 0) OR (@Debit < 0 OR @Credit < 0)
        BEGIN
            SET @OutputMessage = 'Error: Only one of Debit or Credit can be a positive value, and neither can be negative.';
            RETURN;
        END
        IF (@Debit = 0 AND @Credit = 0)
        BEGIN
             SET @OutputMessage = 'Error: Debit or Credit must be greater than zero for a valid entry.';
             RETURN;
        END

        INSERT INTO VoucherDetails (VoucherId, AccountId, Debit, Credit, Remarks)
        VALUES (@VoucherId, @AccountId, @Debit, @Credit, @Remarks); 

        SET @OutputMessage = 'Voucher detail created successfully!';
    END TRY
    BEGIN CATCH
        SET @OutputMessage = 'Error: ' + ERROR_MESSAGE();
    END CATCH
END;
GO

-- PROCEDURE: spUpdateVoucherDetail (FIXED)
IF OBJECT_ID('spUpdateVoucherDetail', 'P') IS NOT NULL DROP PROCEDURE spUpdateVoucherDetail;
GO
CREATE PROCEDURE spUpdateVoucherDetail
    @VoucherDetailId INT,
    -- @VoucherId INT, 
    @AccountId INT,
    @Debit DECIMAL(18, 2),
    @Credit DECIMAL(18, 2),
    @Remarks NVARCHAR(MAX),
    @OutputMessage NVARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF (@Debit > 0 AND @Credit > 0) OR (@Debit < 0 OR @Credit < 0)
        BEGIN
            SET @OutputMessage = 'Error: Only one of Debit or Credit can be a positive value, and neither can be negative.';
            RETURN;
        END

        IF (@Debit = 0 AND @Credit = 0)
        BEGIN
             SET @OutputMessage = 'Error: Debit or Credit must be greater than zero.';
             RETURN;
        END

        UPDATE VoucherDetails
        SET
            -- VoucherId = @VoucherId,
            AccountId = @AccountId,
            Debit = @Debit,
            Credit = @Credit,
            Remarks = @Remarks
        WHERE VoucherDetailId = @VoucherDetailId;

        IF @@ROWCOUNT = 0
            SET @OutputMessage = 'Voucher detail not found or no changes made.';
        ELSE
            SET @OutputMessage = 'Voucher detail updated successfully!';
    END TRY
    BEGIN CATCH
        SET @OutputMessage = 'Error: ' + ERROR_MESSAGE();
    END CATCH
END;
GO

-- PROCEDURE: spDeleteVoucherDetail
IF OBJECT_ID('spDeleteVoucherDetail', 'P') IS NOT NULL DROP PROCEDURE spDeleteVoucherDetail;
GO
CREATE PROCEDURE spDeleteVoucherDetail
    @VoucherDetailId INT,
    @OutputMessage NVARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        DELETE FROM VoucherDetails
        WHERE VoucherDetailId = @VoucherDetailId;

        IF @@ROWCOUNT = 0
            SET @OutputMessage = 'Voucher detail not found.';
        ELSE
            SET @OutputMessage = 'Voucher detail deleted successfully!';
    END TRY
    BEGIN CATCH
        SET @OutputMessage = 'Error: ' + ERROR_MESSAGE();
    END CATCH
END;
GO

-- PROCEDURE: spDeleteVoucherDetailsByVoucherId
IF OBJECT_ID('spDeleteVoucherDetailsByVoucherId', 'P') IS NOT NULL DROP PROCEDURE spDeleteVoucherDetailsByVoucherId;
GO
CREATE PROCEDURE spDeleteVoucherDetailsByVoucherId
    @VoucherId INT,
    @OutputMessage NVARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        DELETE FROM VoucherDetails
        WHERE VoucherId = @VoucherId;

        SET @OutputMessage = 'Voucher details deleted successfully for voucher.';
    END TRY
    BEGIN CATCH
        SET @OutputMessage = 'Error: ' + ERROR_MESSAGE();
    END CATCH
END;
GO




