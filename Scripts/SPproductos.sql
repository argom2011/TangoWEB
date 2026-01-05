CREATE PROCEDURE sp_Productos_Listar
    @Buscar VARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT ProductoID,
           Codigo,
           Nombre,
           Descripcion,
           Precio,
           StockActual,
           StockMinimo,
           Activo
    FROM Productos
    WHERE Activo = 1
      AND (@Buscar IS NULL 
           OR Nombre LIKE '%' + @Buscar + '%' 
           OR Codigo LIKE '%' + @Buscar + '%')
    ORDER BY Nombre;
END
GO



CREATE PROCEDURE sp_Productos_Insertar
    @Codigo VARCHAR(50),
    @Nombre VARCHAR(150),
    @Descripcion VARCHAR(MAX),
    @Precio DECIMAL(18,2),
    @StockActual INT,
    @StockMinimo INT,
    @Activo BIT
AS
BEGIN
    INSERT INTO Productos (Codigo, Nombre, Descripcion, Precio, StockActual, StockMinimo, Activo)
    VALUES (@Codigo, @Nombre, @Descripcion, @Precio, @StockActual, @StockMinimo, @Activo);

    SELECT SCOPE_IDENTITY() AS NuevoID;
END
GO



CREATE PROCEDURE sp_Productos_Actualizar
    @ProductoID INT,
    @Codigo VARCHAR(50),
    @Nombre VARCHAR(150),
    @Descripcion VARCHAR(MAX),
    @Precio DECIMAL(18,2),
    @StockActual INT,
    @StockMinimo INT,
    @Activo BIT
AS
BEGIN
    UPDATE Productos SET
        Codigo       = @Codigo,
        Nombre       = @Nombre,
        Descripcion  = @Descripcion,
        Precio       = @Precio,
        StockActual  = @StockActual,
        StockMinimo  = @StockMinimo,
        Activo       = @Activo
    WHERE ProductoID = @ProductoID;
END
GO



CREATE PROCEDURE sp_Productos_Eliminar
    @ProductoID INT
AS
BEGIN
    UPDATE Productos 
    SET Activo = 0  
    WHERE ProductoID = @ProductoID;
END
GO
