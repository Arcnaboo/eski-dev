CREATE TABLE [dbo].[Users] (
    [Name]     NVARCHAR (MAX)   NOT NULL,
    [Email]    NVARCHAR (MAX)   NOT NULL,
    [Password] NVARCHAR (MAX)   NOT NULL,
    [Tck]      NVARCHAR (MAX)   NOT NULL,
    [Phone]    NVARCHAR (MAX)   NOT NULL,
    [UserId]   UNIQUEIDENTIFIER CONSTRAINT [DF_Users_UserId] DEFAULT (newid()) NOT NULL,
    [Photo]    NVARCHAR (MAX)   NULL,
    [Balance]  DECIMAL (18, 2)  NOT NULL,
    [IBAN]     NVARCHAR (MAX)   NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([UserId] ASC)
);

