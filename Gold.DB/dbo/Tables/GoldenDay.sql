CREATE TABLE [dbo].[GoldenDay] (
    [GoldenDayId]          UNIQUEIDENTIFIER NOT NULL,
    [GoldenDayName]        NVARCHAR (MAX)   NOT NULL,
    [GoldenDayAmount]      NUMERIC (18)     NOT NULL,
    [GoldenDayInterval]    DATE             NOT NULL,
    [GoldenDayNumOfPeople] NUMERIC (18)     NOT NULL,
    [GoldenDayDate]        DATE             NOT NULL,
    [GoldenDayParticipant] NVARCHAR (MAX)   NOT NULL,
    [GoldenDayOrder]       INT              NOT NULL,
    CONSTRAINT [PK_GoldenDay] PRIMARY KEY CLUSTERED ([GoldenDayId] ASC)
);

