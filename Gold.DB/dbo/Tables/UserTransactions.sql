CREATE TABLE [dbo].[UserTransactions] (
    [UserId]        UNIQUEIDENTIFIER NOT NULL,
    [TransactionId] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_UserTransactions] PRIMARY KEY CLUSTERED ([UserId] ASC, [TransactionId] ASC),
    CONSTRAINT [FK_UserTransactions_Transactions] FOREIGN KEY ([TransactionId]) REFERENCES [dbo].[Transactions] ([TransactionId]),
    CONSTRAINT [FK_UserTransactions_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId])
);

