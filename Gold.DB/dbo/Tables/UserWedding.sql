CREATE TABLE [dbo].[UserWedding] (
    [UserId]    UNIQUEIDENTIFIER NOT NULL,
    [WeddingId] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_UserWedding] PRIMARY KEY CLUSTERED ([UserId] ASC, [WeddingId] ASC),
    CONSTRAINT [FK_UserWedding_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]),
    CONSTRAINT [FK_UserWedding_Wedding] FOREIGN KEY ([WeddingId]) REFERENCES [dbo].[Wedding] ([WeddingId])
);

