-- ============================================
-- CREAR BASE DE DATOS
-- ============================================
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'Tango')
BEGIN
    CREATE DATABASE Tango;
END
GO

USE Tango;
GO

-- ============================================
-- TABLA: Clientes
-- ============================================
CREATE TABLE Clientes (
    ClienteID INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    CUIT NVARCHAR(20) NULL,
    Direccion NVARCHAR(200) NULL,
    Telefono NVARCHAR(50) NULL,
    Email NVARCHAR(100) NULL,
    FechaAlta DATETIME DEFAULT GETDATE(),
    Activo BIT DEFAULT 1
);
GO

-- ============================================
-- TABLA: Productos
-- ============================================
CREATE TABLE Productos (
    ProductoID INT IDENTITY(1,1) PRIMARY KEY,
    Codigo NVARCHAR(20) NOT NULL,
    Nombre NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(200) NULL,
    Precio DECIMAL(18,2) NOT NULL DEFAULT 0,
    StockActual INT NOT NULL DEFAULT 0,
    StockMinimo INT NOT NULL DEFAULT 0,
    Activo BIT DEFAULT 1
);
GO

-- ============================================
-- TABLA: Pedidos
-- ============================================
CREATE TABLE Pedidos (
    PedidoID INT IDENTITY(1,1) PRIMARY KEY,
    ClienteID INT NOT NULL,
    Fecha DATETIME DEFAULT GETDATE(),
    Total DECIMAL(18,2) DEFAULT 0,
    Estado NVARCHAR(50) DEFAULT 'Pendiente',
    FOREIGN KEY (ClienteID) REFERENCES Clientes(ClienteID)
);
GO

-- Tabla detalle de pedidos
CREATE TABLE PedidoDetalle (
    PedidoDetalleID INT IDENTITY(1,1) PRIMARY KEY,
    PedidoID INT NOT NULL,
    ProductoID INT NOT NULL,
    Cantidad INT NOT NULL,
    PrecioUnitario DECIMAL(18,2) NOT NULL,
    Subtotal AS (Cantidad * PrecioUnitario) PERSISTED,
    FOREIGN KEY (PedidoID) REFERENCES Pedidos(PedidoID),
    FOREIGN KEY (ProductoID) REFERENCES Productos(ProductoID)
);
GO

-- ============================================
-- TABLA: Facturas
-- ============================================
CREATE TABLE Facturas (
    FacturaID INT IDENTITY(1,1) PRIMARY KEY,
    PedidoID INT NOT NULL,
    Fecha DATETIME DEFAULT GETDATE(),
    Total DECIMAL(18,2) DEFAULT 0,
    Tipo NVARCHAR(20) DEFAULT 'A',
    Estado NVARCHAR(20) DEFAULT 'Pendiente',
    FOREIGN KEY (PedidoID) REFERENCES Pedidos(PedidoID)
);
GO

-- Tabla detalle de facturas
CREATE TABLE FacturaDetalle (
    FacturaDetalleID INT IDENTITY(1,1) PRIMARY KEY,
    FacturaID INT NOT NULL,
    ProductoID INT NOT NULL,
    Cantidad INT NOT NULL,
    PrecioUnitario DECIMAL(18,2) NOT NULL,
    Subtotal AS (Cantidad * PrecioUnitario) PERSISTED,
    FOREIGN KEY (FacturaID) REFERENCES Facturas(FacturaID),
    FOREIGN KEY (ProductoID) REFERENCES Productos(ProductoID)
);
GO

-- ============================================
-- TABLA: MovStock
-- ============================================
CREATE TABLE MovStock (
    MovStockID INT IDENTITY(1,1) PRIMARY KEY,
    ProductoID INT NOT NULL,
    Fecha DATETIME DEFAULT GETDATE(),
    TipoMovimiento NVARCHAR(20) NOT NULL, -- Entrada / Salida
    Cantidad INT NOT NULL,
    DocumentoRef NVARCHAR(50) NULL, -- PedidoID o FacturaID
    StockResultante INT NOT NULL,
    FOREIGN KEY (ProductoID) REFERENCES Productos(ProductoID)
);
GO

-- ============================================
-- TABLA: Asientos contables
-- ============================================
CREATE TABLE Asientos (
    AsientoID INT IDENTITY(1,1) PRIMARY KEY,
    Fecha DATETIME DEFAULT GETDATE(),
    Descripcion NVARCHAR(200) NOT NULL,
    TotalDebe DECIMAL(18,2) NOT NULL,
    TotalHaber DECIMAL(18,2) NOT NULL,
    Referencia NVARCHAR(50) NULL -- FacturaID, PedidoID, etc.
);
GO

-- Tabla detalle de asiento
CREATE TABLE AsientoDetalle (
    AsientoDetalleID INT IDENTITY(1,1) PRIMARY KEY,
    AsientoID INT NOT NULL,
    Cuenta NVARCHAR(50) NOT NULL,
    Debe DECIMAL(18,2) NOT NULL DEFAULT 0,
    Haber DECIMAL(18,2) NOT NULL DEFAULT 0,
    FOREIGN KEY (AsientoID) REFERENCES Asientos(AsientoID)
);
GO


CREATE TYPE dbo.PedidoDetalleType AS TABLE
(
    ProductoID     INT,
    Cantidad       INT,
    PrecioUnitario DECIMAL(18,2)
)
GO
