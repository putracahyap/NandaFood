CREATE DATABASE NANDAFOOD_ORDER

CREATE TABLE [dbo].[FoodOrder] (
    [id]           VARCHAR (50)  NOT NULL,
    [name]         VARCHAR (50)  NOT NULL,
    [menu]         VARCHAR (100) NOT NULL,
    [quantity]     INT           NOT NULL,
    [total_price]  BIGINT        NOT NULL,
    [order_by]     VARCHAR (50)  NOT NULL,
    [order_date]   DATETIME      NOT NULL,
    [order_status] VARCHAR (20)  NULL,
    [updated_by]   VARCHAR (50)  NULL,
    [updated_date] DATETIME      NULL,
    CONSTRAINT [PK_Order] PRIMARY KEY CLUSTERED ([id] ASC)
);

