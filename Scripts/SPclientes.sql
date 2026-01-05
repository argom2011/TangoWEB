CREATE  PROCEDURE sp_Clientes_Listar
    @Buscar VARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT ClienteId, Nombre, CUIT, Direccion, Telefono, Email, FechaAlta, Activo
    FROM Clientes
    WHERE Activo = 1
      AND (@Buscar IS NULL OR Nombre LIKE '%' + @Buscar + '%')
    ORDER BY Nombre;
END;

CREATE PROCEDURE sp_Clientes_Insertar
    @Nombre VARCHAR(100),
    @CUIT VARCHAR(20),
    @Direccion VARCHAR(200),
    @Telefono VARCHAR(30),
    @Email VARCHAR(100)
AS
BEGIN
    INSERT INTO Clientes (Nombre, CUIT, Direccion, Telefono, Email)
    VALUES (@Nombre, @CUIT, @Direccion, @Telefono, @Email);

    SELECT SCOPE_IDENTITY() AS NuevoId; -- Devuelve ID generado
END;
CREATE PROCEDURE sp_Clientes_Actualizar
    @Id INT,
    @Nombre VARCHAR(100),
    @CUIT VARCHAR(20),
    @Direccion VARCHAR(200),
    @Telefono VARCHAR(30),
    @Email VARCHAR(100)
AS
BEGIN
    UPDATE Clientes SET
        Nombre = @Nombre,
        CUIT = @CUIT,
        Direccion = @Direccion,
        Telefono = @Telefono,
        Email = @Email
    WHERE ClienteID = @Id;
END;
CREATE OR ALTER PROCEDURE sp_Clientes_Eliminar
    @Id INT
AS
BEGIN
    UPDATE Clientes SET Activo = 0 WHERE ClienteID = @Id;
END;
