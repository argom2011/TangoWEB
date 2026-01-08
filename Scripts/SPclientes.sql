USE [Tango]
GO
/****** Object:  StoredProcedure [dbo].[sp_Clientes_Actualizar]    Script Date: 8/1/2026 12:28:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_Clientes_Actualizar]
    @Id INT,
    @Nombre VARCHAR(100),
    @CUIT VARCHAR(20),
    @Direccion VARCHAR(200),
    @Telefono VARCHAR(30),
    @Email VARCHAR(100),
    @Activo BIT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Actualiza los datos del cliente
        UPDATE Clientes 
        SET
            Nombre = @Nombre,
            CUIT = @CUIT,
            Direccion = @Direccion,
            Telefono = @Telefono,
            Email = @Email,
            Activo = @Activo
        WHERE ClienteID = @Id;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        -- Si ocurre algún error, revierte la transacción
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        -- Devuelve el mensaje de error
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;




USE [Tango]
GO
/****** Object:  StoredProcedure [dbo].[sp_Clientes_Eliminar]    Script Date: 8/1/2026 12:29:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_Clientes_Eliminar]
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Desactiva el cliente
        UPDATE Clientes 
        SET Activo = 0 
        WHERE ClienteID = @Id;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        -- Si algo falla, revierte la transacción
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        -- Devuelve el error
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;



USE [Tango]
GO
/****** Object:  StoredProcedure [dbo].[sp_Clientes_Insertar]    Script Date: 8/1/2026 12:30:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_Clientes_Insertar]
    @Nombre VARCHAR(100),
    @CUIT VARCHAR(20),
    @Direccion VARCHAR(200),
    @Telefono VARCHAR(30),
    @Email VARCHAR(100),
    @Activo BIT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Inserta el cliente
        INSERT INTO Clientes (Nombre, CUIT, Direccion, Telefono, Email, Activo)
        VALUES (@Nombre, @CUIT, @Direccion, @Telefono, @Email, @Activo);

        -- Devuelve el ID generado
        SELECT SCOPE_IDENTITY() AS NuevoId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        -- Si algo falla, revierte la transacción
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        -- Devuelve el error
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;


USE [Tango]
GO
/****** Object:  StoredProcedure [dbo].[sp_Clientes_Listar]    Script Date: 8/1/2026 12:30:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_Clientes_Listar]
    @Buscar VARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Consulta principal
        SELECT ClienteId, 
               Nombre, 
               CUIT, 
               Direccion, 
               Telefono, 
               Email, 
               FechaAlta, 
               ISNULL(Activo, 0) AS Activo
        FROM Clientes
        WHERE (@Buscar IS NULL OR Nombre LIKE '%' + @Buscar + '%')
        ORDER BY ClienteID;
    END TRY
    BEGIN CATCH
        -- Captura y relanza el error
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();

        -- Opcional: guardar en tabla de logs
        -- INSERT INTO LogErrores(Fecha, Mensaje) VALUES(GETDATE(), @ErrorMessage);

        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
