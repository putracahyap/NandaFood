CREATE DATABASE NANDAFOOD

CREATE TABLE [dbo].[Roles] (
    [role_code]   VARCHAR (5)   NOT NULL,
    [description] NVARCHAR (20) NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY CLUSTERED ([role_code] ASC)
);

CREATE TABLE [dbo].[Accounts] (
    [id]           VARCHAR (50) NOT NULL,
    [username]     VARCHAR (50) NOT NULL,
    [user_secret]  VARCHAR (50) NOT NULL,
    [user_role]    VARCHAR (5)  NOT NULL,
    [first_name]   VARCHAR (50) NOT NULL,
    [last_name]    VARCHAR (50) NULL,
    [created_date] DATETIME     NOT NULL,
    [updated_date] DATETIME     NULL,
    [is_login]     BIT          NOT NULL,
    CONSTRAINT [PK_Accounts] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_Accounts_Roles] FOREIGN KEY ([user_role]) REFERENCES [dbo].[Roles] ([role_code])
);

CREATE TABLE [dbo].[RefreshToken] (
    [id]          VARCHAR (50)  NOT NULL,
    [token]       VARCHAR (MAX) NULL,
    [jwt_id]      VARCHAR (MAX) NULL,
    [is_revoked]  BIT           NOT NULL,
    [accounts_id] VARCHAR (50)  NULL,
    [date_added]  DATETIME      NULL,
    [date_expire] DATETIME      NULL,
    CONSTRAINT [PK_RefreshToken] PRIMARY KEY CLUSTERED ([id] ASC)
);

CREATE TABLE [dbo].[RevokedToken] (
    [id]              VARCHAR (50)  NOT NULL,
    [token]           VARCHAR (MAX) NOT NULL,
    [revocation_date] DATETIME      NOT NULL,
    CONSTRAINT [PK_RevokedToken] PRIMARY KEY CLUSTERED ([id] ASC)
);

INSERT INTO Roles (role_code, [description]) VALUES ('NFA', 'Admin'), ('NFM', 'Member')