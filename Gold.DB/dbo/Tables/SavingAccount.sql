CREATE TABLE [dbo].[SavingAccount] (
    [SavingAccountId]             UNIQUEIDENTIFIER CONSTRAINT [DF_SavingAccount_SavingAccountId] DEFAULT (newid()) NOT NULL,
    [SavingAccountTitle]          NVARCHAR (MAX)   NOT NULL,
    [SavingAccountAmount]         DECIMAL (18)     NOT NULL,
    [SavingAccountDate]           DATETIME         NOT NULL,
    [SavingAccountGoal]           DECIMAL (18)     NOT NULL,
    [SavingAccountCurrentBalance] DECIMAL (18)     NOT NULL,
    CONSTRAINT [PK_SavingAccount] PRIMARY KEY CLUSTERED ([SavingAccountId] ASC)
);

