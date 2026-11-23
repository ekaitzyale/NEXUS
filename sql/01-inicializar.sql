USE master;
GO
IF DB_ID(N'NexusIris') IS NULL
    CREATE DATABASE NexusIris;
GO
USE NexusIris;
GO
IF OBJECT_ID(N'dbo.Partida', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Partida
    (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_Partida PRIMARY KEY,
        Cadete nvarchar(60) NOT NULL,
        FechaGuardado datetime2(3) NOT NULL CONSTRAINT DF_Partida_Fecha DEFAULT SYSUTCDATETIME(),
        Fase tinyint NOT NULL,
        Sala tinyint NOT NULL,
        FaseCompletada bit NOT NULL,
        Estado nvarchar(max) NOT NULL,
        CONSTRAINT CK_Partida_Cadete CHECK (LEN(Cadete) > 0),
        CONSTRAINT CK_Partida_Estado CHECK (ISJSON(Estado) = 1),
        CONSTRAINT CK_Partida_Sala CHECK
        (
            (Fase = 0 AND Sala BETWEEN 1 AND 3)
            OR (Fase = 1 AND Sala BETWEEN 4 AND 7)
            OR (Fase = 2 AND Sala BETWEEN 8 AND 13)
            OR (Fase = 3 AND Sala BETWEEN 14 AND 18)
        )
    );
END;
GO
