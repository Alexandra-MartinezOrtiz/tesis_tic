# Evidencia — Configuración del entorno de desarrollo

**Proyecto:** Módulo de Consultas y Reportes — Sistema web TIC-FIS
**Facultad de Ingeniería de Sistemas — Escuela Politécnica Nacional**
**Tipo de documento:** Evidencia complementaria del repositorio

---

## Introducción

Este documento reúne las capturas de soporte del entorno técnico configurado en el
Sprint 0 (estructura modular del backend y contenedores Docker). En el cuerpo principal
de la tesis se conserva la descripción de la arquitectura y el diseño; estas capturas de
infraestructura se trasladan aquí para reducir la extensión del documento **sin perder la
evidencia técnica**. Este material **no forma parte del documento principal de la tesis**.

> Las imágenes referenciadas se encuentran en `02Figures/02Chapter/` del repositorio.

---

## 1. Estructura modular del backend (Arquitectura Limpia)

- **Imagen:** `SPRINT0_Arquitectura_Modular_Backend.png`
- **Qué evidencia:** cada microservicio (Propuestas y Reportes) organiza su código en
  cuatro proyectos .NET independientes:
  - `*.Api` — controladores y configuración de inicio.
  - `*.Application` — abstracciones, DTOs y servicios de aplicación.
  - `*.Domain` — entidades y reglas de negocio.
  - `*.Infrastructure` — repositorios, contexto de Entity Framework Core e integraciones
    con librerías externas.
- **Relevancia:** la separación física en proyectos distintos refuerza el principio de
  inversión de dependencias: el compilador impide que una capa superior importe
  directamente los tipos de la capa de infraestructura.

## 2. Contenedores Docker activos

- **Imagen:** `SPRINT0_Docker_PostgreSQL_Activo.png`
- **Qué evidencia:** dos contenedores activos durante las sesiones de desarrollo:
  - `backend` — API REST desarrollada con ASP.NET Core.
  - `ticfis-postgre` — imagen oficial de PostgreSQL 16, expuesta en el puerto `5432:5432`.
- **Relevancia:** el entorno contenerizado garantiza reproducibilidad, aísla las
  dependencias y permite ejecutar las pruebas de integración sin instalaciones globales
  del sistema operativo.

---

> **Nota aclaratoria:** este documento es un respaldo técnico complementario del
> repositorio del proyecto y **no forma parte del cuerpo principal de la tesis**.
