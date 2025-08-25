CREATE TABLE [dbo].[Wedding] (
    [WeddingId]          UNIQUEIDENTIFIER CONSTRAINT [DF_Wedding_WeddingId] DEFAULT (newid()) NOT NULL,
    [WeddingDate]        DATE             NOT NULL,
    [WeddingName]        NVARCHAR (MAX)   NOT NULL,
    [WeddingDescription] NVARCHAR (MAX)   NOT NULL,
    [WeddingTransaction] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_Wedding] PRIMARY KEY CLUSTERED ([WeddingId] ASC)
);

