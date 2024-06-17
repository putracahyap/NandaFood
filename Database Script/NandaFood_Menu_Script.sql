CREATE DATABASE NANDAFOOD_MENU

CREATE TABLE [dbo].[FoodMenu] (
    [id]           VARCHAR (50)  NOT NULL,
    [menu]         VARCHAR (100) NOT NULL,
    [price]        BIGINT        NOT NULL,
    [status]       BIT           NOT NULL,
    [created_by]   VARCHAR (50)  NOT NULL,
    [created_date] DATETIME      NOT NULL,
    [updated_by]   VARCHAR (50)  NULL,
    [updated_date] DATETIME      NULL,
    CONSTRAINT [PK_Menu] PRIMARY KEY CLUSTERED ([id] ASC)
);

