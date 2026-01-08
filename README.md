
# TangoWEB – Sistema de Ventas con Arquitectura Transaccional

## 📌 Descripción general

**TangoWEB** es una aplicación web desarrollada en **ASP.NET Core + SQL Server** que implementa un flujo completo de ventas con foco en:

* Integridad de datos
* Diseño transaccional real
* Arquitectura escalable y mantenible
* Separación clara de responsabilidades

El objetivo del proyecto es demostrar **patrones productivos reales**, similares a los utilizados en sistemas financieros y bancarios.

---

## 🏗 Arquitectura

La solución está organizada en capas bien definidas:

Frontend MVC (Razor / Views)
        │
        ▼
Controllers (API / MVC)
        │
        ▼
Business / Services
        │
        ▼
SQL Server (Stored Procedures + TVP)

No se utilizan repositorios explícitos:
la lógica crítica se delega a **Stored Procedures transaccionales**, evitando estados intermedios corruptos.

---

## 🔐 Confirmación de Venta – Diseño Transaccional

La operación principal se realiza a través del procedimiento:


sp_Ventas_ConfirmarPedido


Características:

* La API envía:

  * Cabecera de venta
  * Detalle como **Table Valued Parameter (TVP)**
* El motor SQL Server se encarga de:

  * Abrir la transacción
  * Insertar pedido y detalle
  * Actualizar stock
  * Validar reglas de negocio
  * Confirmar o deshacer toda la operación (COMMIT / ROLLBACK)

Esto garantiza:

* Atomicidad
* Consistencia
* Integridad
* Cero estados intermedios

Este patrón replica el comportamiento de sistemas reales de pagos.

---

## 📊 Reportes y Dashboard

El módulo de reportes permite:

* Filtros por fechas, cliente y producto
* Dashboard con:

  * Ventas por cliente
  * Ventas por producto
* Visualización directa desde MVC

---

## ⚠️ Contabilidad – Asientos

La arquitectura **está preparada** para integrar generación automática de asientos contables dentro de la misma transacción de venta.

👉 **En esta entrega NO se incluyen las tablas ni la lógica de asientos contables.**

Esto se deja de forma intencional como:

> Punto de extensión para que cada desarrollador pueda implementar
> su propia lógica contable según su dominio de negocio.

Cualquier contribución o adaptación es bienvenida.

---

## 🎯 Objetivo del proyecto

Este repositorio no busca ser un sistema cerrado, sino:

* Mostrar patrones profesionales reales
* Servir como base de aprendizaje
* Compartir arquitectura sin exponer reglas de negocio propietarias

---

# Guía de Implementación de la Base de Datos

## Introducción
Este documento describe los pasos necesarios para implementar la base de datos, crear las tablas y configurar los procedimientos almacenados para el proyecto.

## Requisitos Previos
- **Sistema de Gestión de Base de Datos (DBMS)**: SQL Server (versión 2019 express).

## Pasos para la Implementación

-**Ruta del proyecto:SCRIPTS, en orden de la siguiente manera: EsquemadeCreacion.SQL, SPclientes.SQL, SPproductos.SQL, sp_Ventas_ConfirmarPedido.SQL
-**Contemplar la cadena de conexion: "TangoDB": "Server=ARGOM\\SQLEXPRESS;Database=Tango;Trusted_Connection=True;TrustServerCertificate=True;": cambiar por el de su alias del SERVER.

