CREATE TABLE [dbo].[Specific] (
    [SpecificId]          UNIQUEIDENTIFIER CONSTRAINT [DF_Specific_SpecificId] DEFAULT (newid()) NOT NULL,
    [SpecificName]        NVARCHAR (MAX)   NOT NULL,
    [SpecificDate]        DATE             NOT NULL,
    [SpecificDescription] NVARCHAR (MAX)   NOT NULL,
    [SpecificTransaction] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_Specific] PRIMARY KEY CLUSTERED ([SpecificId] ASC)
);

