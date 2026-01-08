USE [Tango]
GO
/****** Object:  StoredProcedure [dbo].[sp_Ventas_ConfirmarPedido]    Script Date: 8/1/2026 12:33:04 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_Ventas_ConfirmarPedido]
(
    @ClienteID INT,
    @Total     DECIMAL(18,2),
    @Detalle   dbo.PedidoDetalleType READONLY
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @PedidoID  INT;
    DECLARE @FacturaID INT;

    BEGIN TRY
        BEGIN TRAN;

        -- ============================
        -- PEDIDO
        -- ============================
        INSERT INTO Pedidos (ClienteID, Total, Estado)
        VALUES (@ClienteID, @Total, 'Confirmado');

        SET @PedidoID = SCOPE_IDENTITY();

        INSERT INTO PedidoDetalle (PedidoID, ProductoID, Cantidad, PrecioUnitario)
        SELECT @PedidoID, ProductoID, Cantidad, PrecioUnitario
        FROM @Detalle;

        -- ============================
        -- FACTURA
        -- ============================
        INSERT INTO Facturas (PedidoID, Total, Tipo, Estado)
        VALUES (@PedidoID, @Total, 'A', 'Emitida');

        SET @FacturaID = SCOPE_IDENTITY();

        INSERT INTO FacturaDetalle (FacturaID, ProductoID, Cantidad, PrecioUnitario)
        SELECT @FacturaID, ProductoID, Cantidad, PrecioUnitario
        FROM @Detalle;

        -- ============================
        -- STOCK
        -- ============================
        DECLARE @ProductoID INT, @Cantidad INT, @StockActual INT;

        DECLARE curStock CURSOR FOR
            SELECT ProductoID, Cantidad FROM @Detalle;

        OPEN curStock;
        FETCH NEXT FROM curStock INTO @ProductoID, @Cantidad;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            SELECT @StockActual = StockActual
            FROM Productos
            WHERE ProductoID = @ProductoID;

            IF (@StockActual - @Cantidad) < 0
                THROW 50001, 'Stock insuficiente para el producto', 1;

            UPDATE Productos
            SET StockActual = StockActual - @Cantidad
            WHERE ProductoID = @ProductoID;

            INSERT INTO MovStock
            (
                ProductoID,
                TipoMovimiento,
                Cantidad,
                DocumentoRef,
                StockResultante
            )
            VALUES
            (
                @ProductoID,
                'VENTA',
                -@Cantidad,
                'FAC-' + CAST(@FacturaID AS NVARCHAR),
                @StockActual - @Cantidad
            );

            FETCH NEXT FROM curStock INTO @ProductoID, @Cantidad;
        END

        CLOSE curStock;
        DEALLOCATE curStock;

        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;

        DECLARE @Msg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@Msg,16,1);
    END CATCH
END
