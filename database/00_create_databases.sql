IF DB_ID(N'deckza_db') IS NULL
BEGIN
    CREATE DATABASE [deckza_db];
END;
GO

IF DB_ID(N'deckza_log_db') IS NULL
BEGIN
    CREATE DATABASE [deckza_log_db];
END;
GO
