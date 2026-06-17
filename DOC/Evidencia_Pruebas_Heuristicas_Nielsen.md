# Evidencia complementaria — Revisión heurística de Nielsen

**Proyecto:** Módulo de Consultas y Reportes — Sistema web TIC-FIS
**Facultad de Ingeniería de Sistemas — Escuela Politécnica Nacional**
**Tipo de documento:** Evidencia técnica complementaria (repositorio)

---

## 1. Introducción

Durante el ciclo de revisión de la interfaz del Módulo de Consultas y Reportes se
realizó una **evaluación heurística basada en los diez principios de usabilidad de
Jakob Nielsen**. Esta revisión se llevó a cabo como una inspección interna de
usabilidad, con el objetivo de detectar oportunidades de mejora en el diseño de
interacción antes de dar por concluido el desarrollo del frontend Angular.

Este documento recopila los resultados de esa revisión como **evidencia técnica
complementaria**. Se mantiene en el repositorio para dejar constancia del trabajo de
inspección realizado sobre la interfaz, **sin formar parte del documento principal de
la tesis**, donde la validación de la calidad de la interfaz se reporta mediante las
pruebas no funcionales (auditoría de rendimiento y accesibilidad con Lighthouse).

## 2. ¿Qué es la evaluación heurística de Nielsen?

La evaluación heurística es una técnica de inspección de usabilidad que consiste en
examinar una interfaz frente a un conjunto de principios reconocidos
internacionalmente, identificando problemas de diseño **sin necesidad de involucrar a
usuarios finales** en sesiones de prueba. Cada heurística se valora según el grado en
que la interfaz la cumple.

Para esta revisión se utilizó una **escala de severidad de cuatro niveles**:

| Puntaje | Interpretación |
|:------:|----------------|
| 0 | No aplica / sin problemas |
| 1 | Problema estético |
| 2 | Problema menor |
| 3 | Problema mayor |
| 4 | Cumplimiento satisfactorio / sin problema identificado |

La revisión cubrió los flujos principales del módulo: consulta del listado de
propuestas, aplicación de filtros, visualización del formulario F\_AA\_233A y
exportación como PDF.

## 3. Criterios revisados y mejoras implementadas

| N.° | Heurística | Puntaje | Mejora implementada |
|:---:|------------|:-------:|---------------------|
| H1 | Visibilidad del estado del sistema | 4 | Toast de retroalimentación al imprimir |
| H2 | Correspondencia con el mundo real | 4 | Terminología académica e institucional |
| H3 | Control y libertad del usuario | 4 | Confirmación antes de ejecutar la impresión |
| H4 | Consistencia y estándares | 4 | Botones unificados con íconos SVG coherentes |
| H5 | Prevención de errores | 4 | Validación del identificador de propuesta en la URL |
| H6 | Reconocimiento antes que recuerdo | 4 | Filtros y acciones visibles en pantalla en todo momento |
| H7 | Flexibilidad y eficiencia de uso | 4 | Atajos de teclado `/` para buscar y `Esc` para limpiar |
| H8 | Diseño estético y minimalista | 4 | Interfaz limpia, sin elementos decorativos innecesarios |
| H9 | Ayuda a reconocer y recuperarse de errores | 4 | Mensajes de error descriptivos con indicación de acción |
| H10 | Ayuda y documentación | 4 | Tooltip de ayuda `?` contextual en campos complejos |
| | **Total** | **40 / 40** | |

## 4. Observaciones encontradas

La revisión inicial identificó varias oportunidades de mejora que se atendieron dentro
del mismo ciclo de desarrollo:

- **H1 — Visibilidad del estado:** la acción de imprimir no ofrecía retroalimentación
  visible. Se incorporó un *toast* que informa el inicio y la finalización del proceso.
- **H3 — Control y libertad:** la impresión se ejecutaba de forma inmediata. Se añadió
  un diálogo de confirmación para evitar activaciones accidentales.
- **H4 — Consistencia:** los botones de acción presentaban estilos heterogéneos. Se
  unificaron con íconos SVG coherentes en todo el módulo.
- **H5 — Prevención de errores:** se agregó validación del identificador de propuesta
  recibido en la URL antes de consultar al servicio, evitando navegaciones inválidas.
- **H7 — Eficiencia de uso:** se incorporaron atajos de teclado (`/` para buscar,
  `Esc` para limpiar filtros) orientados a usuarios frecuentes.
- **H10 — Ayuda y documentación:** se añadió un *tooltip* contextual `?` en los campos
  de filtrado cuyo comportamiento podía no resultar evidente para un usuario nuevo.

## 5. Resultado general

Tras implementar las mejoras descritas, la interfaz alcanzó un **cumplimiento
satisfactorio en las diez heurísticas (40/40, 100 %)**. La revisión heurística resultó
una técnica ágil y de bajo costo para detectar y corregir problemas de usabilidad de
forma temprana, sin requerir sesiones formales con usuarios finales.

## 6. Nota aclaratoria

> Esta revisión heurística constituye **evidencia técnica complementaria** del trabajo
> realizado sobre la interfaz del módulo. **No forma parte del documento principal de
> la tesis** y se conserva únicamente como respaldo en el repositorio. En el documento
> de tesis, la calidad de la interfaz se valida dentro de las **pruebas no funcionales**
> mediante la auditoría automatizada de rendimiento y accesibilidad realizada con
> Lighthouse.
