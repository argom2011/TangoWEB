USE [Tango]
GO
/****** Object:  StoredProcedure [dbo].[sp_Productos_Actualizar]    Script Date: 8/1/2026 12:32:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[sp_Productos_Actualizar]
    @ProductoID INT,    
    @Nombre VARCHAR(150),
    @Descripcion VARCHAR(MAX),
    @Precio DECIMAL(18,2),
    @StockActual INT,
    @StockMinimo INT,
    @Activo BIT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Actualiza los datos del producto
        UPDATE Productos 
        SET
            Nombre       = @Nombre,
            Descripcion  = @Descripcion,
            Precio       = @Precio,
            StockActual  = @StockActual,
            StockMinimo  = @StockMinimo,
            Activo       = @Activo
        WHERE ProductoID = @ProductoID;

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
/****** Object:  StoredProcedure [dbo].[sp_Productos_Eliminar]    Script Date: 8/1/2026 12:32:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[sp_Productos_Eliminar]
    @ProductoID INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Eliminación lógica: marca como inactivo
        UPDATE Productos 
        SET Activo = 0  
        WHERE ProductoID = @ProductoID;

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
/****** Object:  StoredProcedure [dbo].[sp_Productos_Insertar]    Script Date: 8/1/2026 12:32:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[sp_Productos_Insertar]
    @Nombre VARCHAR(150),
    @Descripcion VARCHAR(MAX),
    @Precio DECIMAL(18,2),
    @StockActual INT,
    @StockMinimo INT,
    @Activo BIT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Inserción del producto
        INSERT INTO Productos (Nombre, Descripcion, Precio, StockActual, StockMinimo, Activo)
        VALUES (@Nombre, @Descripcion, @Precio, @StockActual, @StockMinimo, @Activo);

        -- Devuelve el ID generado
        SELECT SCOPE_IDENTITY() AS NuevoID;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        -- Si hay error, revierte la transacción
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        -- Devuelve mensaje de error
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;



USE [Tango]
GO
/****** Object:  StoredProcedure [dbo].[sp_Productos_Listar]    Script Date: 8/1/2026 12:32:36 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[sp_Productos_Listar]
    @Buscar VARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Consulta principal
        SELECT ProductoID,           
               Nombre,
               Descripcion,
               Precio,
               StockActual,
               StockMinimo,
               Activo
        FROM Productos
        WHERE 
           (@Buscar IS NULL 
               OR Nombre LIKE '%' + @Buscar + '%')
        ORDER BY Nombre;
    END TRY
    BEGIN CATCH
        -- Manejo del error
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();

        -- Opcional: registrar en tabla de logs
        -- INSERT INTO LogErrores(Fecha, Mensaje) VALUES(GETDATE(), @ErrorMessage);

        -- Re-lanzar el error al cliente
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
