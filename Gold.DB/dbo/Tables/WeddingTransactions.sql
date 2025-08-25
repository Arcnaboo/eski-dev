CREATE TABLE [dbo].[WeddingTransactions] (
    [WeddingId]     UNIQUEIDENTIFIER NOT NULL,
    [TransactionId] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_WeddingTransactions] PRIMARY KEY CLUSTERED ([WeddingId] ASC, [TransactionId] ASC),
    CONSTRAINT [FK_WeddingTransactions_Transactions] FOREIGN KEY ([TransactionId]) REFERENCES [dbo].[Transactions] ([TransactionId]),
    CONSTRAINT [FK_WeddingTransactions_Wedding] FOREIGN KEY ([WeddingId]) REFERENCES [dbo].[Wedding] ([WeddingId])
);

