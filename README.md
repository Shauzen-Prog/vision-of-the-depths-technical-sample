# Vision of the Depths – Technical Sample
### Unity Gameplay Systems Overview

---

## Overview

**Vision of the Depths** es un proyecto desarrollado en **Unity 6** como parte de una tesis de desarrollo de videojuegos.

El foco principal del proyecto está puesto en la **arquitectura de sistemas**, la **modularidad**, el **desacoplamiento** y la **preparación para extensión y testeo**, más que en el contenido artístico o visual.

El juego es una experiencia narrativa de **terror psicológico sci-fi**, pero este repositorio público contiene **únicamente un subset curado del código**, con el objetivo de mostrar **criterios técnicos y decisiones de diseño de sistemas**.

> Este repositorio no representa el proyecto completo del juego, sino un **technical sample orientado a gameplay y systems programming**.

---

## Architectural Approach

El proyecto está construido bajo una filosofía de **arquitectura modular**, priorizando:

- Bajo acoplamiento entre sistemas  
- Comunicación desacoplada mediante eventos  
- Separación clara de responsabilidades  
- Preparación para extensión futura  
- Facilidad de testeo y mantenimiento  

---

## Principios y Técnicas Utilizadas

- Dependency Injection (**Zenject**)
- Event-Driven Architecture (**EventBus**)
- Facade Pattern para sistemas complejos (Player)
- State Machines / **HFSM** para lógica de gameplay
- ScriptableObjects para configuraciones y tooling data-driven
- Uso extensivo de **interfaces** para desacoplar contratos

---

## Core Architecture

El proyecto cuenta con una capa **Core reutilizable**, independiente del gameplay específico, que incluye:

- **EventBus global** para comunicación desacoplada
- **Scene Routing System** con soporte para:
  - escenas aditivas
  - preload
  - unload
- **Abstracciones de Input** desacopladas del Unity Input System
- **Arquitectura base de Interaction** reutilizable
- **HFSM genérico** basado en coroutines

Esta capa está pensada para ser reutilizable en otros proyectos.

---

## Gameplay Systems Destacados

### Player Modular (Facade + Input desacoplado)

- Implementación de un **PlayerFacade** como punto único de acceso al jugador
- Input completamente desacoplado mediante puertos
- Separación clara entre:
  - Dominio
  - Aplicación
  - Presentación
- Preparado para bloquear capacidades específicas:
  - movement
  - look
  - pause
  - closeups

---

### Interaction System + Closeup System

- Sistema de interacción basado en **interfaces y contexto**
- Targets de interacción desacoplados del input
- **Closeup System** con:
  - prioridades
  - requests
  - bloqueo de control del jugador
- Integración limpia con diálogos y escenas aditivas

---

### Drill HFSM (Core Mecánico)

- **Hierarchical Finite State Machine** dedicada al drill
- Estados y sub-estados claramente separados
- Lógica de gameplay completamente desacoplada de:
  - UI
  - Audio
  - VFX
- Comunicación vía eventos
- Ejemplo de **mecánica core con arquitectura extensible**

---

### Scene Routing / Additive Loading

- Sistema de navegación de escenas desacoplado
- Soporte para:
  - cambio inmediato
  - carga asíncrona con progreso
  - escenas aditivas
  - preload sin activación
- Uso de **SceneReferenceObject** para evitar strings hardcodeados
- Preparado para futura integración con Addressables

---

### EventBus

- EventBus central para desacoplar sistemas
- Uso extensivo en:
  - gameplay
  - diálogos
  - closeups
  - drill
  - scene routing
- Incluye **tests unitarios básicos** de publish / subscribe

---

### Cheats & Tools (Data-Driven)

- Sistema de cheats pensado como **herramienta de desarrollo**
- Configuración data-driven mediante ScriptableObjects
- Útil para:
  - debug
  - QA
  - iteración rápida
- Ejemplo de tooling diseñado desde arquitectura

---

## Estructura del Repositorio

Este repositorio contiene únicamente código:

2-Scripts/

├── Core/

├── Data/

├── Gameplay/

└── Tests/

No incluye:

- Assets
- Escenas
- Audio
- UI visual
- Arte

---

## Material Adicional

- 🎥 Video técnico corto (overview de arquitectura)
- 🎥 Video técnico largo (deep dive de sistemas)
- 🎮 Build jugable (fuera de este repositorio)

---

## Objetivo del Repositorio

Este repositorio está pensado para:

- Mostrar **criterio arquitectónico**
- Evidenciar pensamiento en **extensibilidad**
- Demostrar buenas prácticas de **gameplay systems**
- Servir como **portfolio técnico**

---

## Notas Finales

Este proyecto prioriza **cómo están diseñados los sistemas**, no su complejidad visual.

Cada sistema fue pensado para ser:
- escalable
- testeable
- mantenible
- fácil de razonar en equipo
