USE [deckza_db];
GO

IF EXISTS (SELECT 1 FROM sys.synonyms WHERE name = N'tb_audit_trail')
    DROP SYNONYM [dbo].[tb_audit_trail];
GO

CREATE SYNONYM [dbo].[tb_audit_trail]
    FOR [deckza_log_db].[dbo].[tb_audit_trail];
GO

IF EXISTS (SELECT 1 FROM sys.synonyms WHERE name = N'tb_audit_trail_detail')
    DROP SYNONYM [dbo].[tb_audit_trail_detail];
GO

CREATE SYNONYM [dbo].[tb_audit_trail_detail]
    FOR [deckza_log_db].[dbo].[tb_audit_trail_detail];
GO

IF EXISTS (SELECT 1 FROM sys.synonyms WHERE name = N'tb_log')
    DROP SYNONYM [dbo].[tb_log];
GO

CREATE SYNONYM [dbo].[tb_log]
    FOR [deckza_log_db].[dbo].[tb_log];
GO
