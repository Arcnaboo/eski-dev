CREATE TABLE [dbo].[Transactions] (
    [TransactionId] UNIQUEIDENTIFIER CONSTRAINT [DF_Transactions_TransactionId] DEFAULT (newid()) NOT NULL,
    [Source]        UNIQUEIDENTIFIER NOT NULL,
    [Destination]   UNIQUEIDENTIFIER NOT NULL,
    [Result]        BIT              NOT NULL,
    [DateTime]      DATETIME         NOT NULL,
    CONSTRAINT [PK_Transactions] PRIMARY KEY CLUSTERED ([TransactionId] ASC)
);

