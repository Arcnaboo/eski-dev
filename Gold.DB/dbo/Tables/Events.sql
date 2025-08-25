CREATE TABLE [dbo].[Events] (
    [EventName] NVARCHAR (MAX)   NOT NULL,
    [EventId]   UNIQUEIDENTIFIER CONSTRAINT [DF_Events_EventId] DEFAULT (newid()) NOT NULL,
    CONSTRAINT [PK_Events] PRIMARY KEY CLUSTERED ([EventId] ASC)
);

