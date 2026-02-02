-- Add missing columns to orders table
ALTER TABLE [dbo].[orders] ADD [payment_method] NVARCHAR(50) NOT NULL DEFAULT 'card';
ALTER TABLE [dbo].[orders] ADD [phone] NVARCHAR(20) NOT NULL DEFAULT '';
ALTER TABLE [dbo].[orders] ADD [shipping_address] NVARCHAR(500) NOT NULL DEFAULT '';
ALTER TABLE [dbo].[orders] ADD [shipping_cost] DECIMAL(10,2) NOT NULL DEFAULT 0;
ALTER TABLE [dbo].[orders] ADD [tax] DECIMAL(10,2) NOT NULL DEFAULT 0;
ALTER TABLE [dbo].[orders] ADD [created_at] DATETIME2 NOT NULL DEFAULT GETUTCDATE();
ALTER TABLE [dbo].[orders] ADD [updated_at] DATETIME2 NOT NULL DEFAULT GETUTCDATE();

PRINT 'Columns added to [orders] table successfully.';
GO
