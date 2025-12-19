# Vision of the Depths – Technical Sample

**TL;DR:** A Unity 6 technical gameplay sample focused on clean, decoupled systems and solid architectural foundations for scalable projects.

---

## If You Have 5 Minutes

If you don’t have time to read the full README, these files provide a **high-signal overview** of how the project is architected and how gameplay systems are designed:

- **EventBus**  
  Central, non-static, testable event system used to decouple gameplay, dialogue, closeups, and scene routing.

- **PlayerFacade + PlayerFacadeService**  
  Single access point to the player, safe to use across additive scenes and fully decoupled from presentation.

- **SceneRouter**  
  Additive scene routing with async loading, preload support, controlled activation, and transition abstraction.

- **CloseupInteractionV2**  
  Interaction system integrating cameras, player control locking, dialogue coordination, and additive scenes.

- **Drill HFSM**  
  Core gameplay mechanic implemented as a hierarchical finite state machine, fully decoupled from UI, audio, and VFX.

These files reflect a focus on **maintainability, testability, and team-friendly gameplay architecture**.

---

## Overview

**Vision of the Depths** is a project developed in **Unity 6** as part of a game development thesis.

The main focus of the project is on **systems architecture**, **modularity**, **decoupling**, and **preparation for extension and testing**, rather than on visual or artistic content.

The game itself is a **sci-fi psychological horror narrative experience**, but this public repository contains **only a curated subset of the code**, intended to showcase **technical decision-making and gameplay systems design**.

> This repository does not represent the full game project, but rather a **technical sample focused on gameplay and systems programming**.

---

## Architectural Approach

The project is built around a **modular architecture philosophy**, prioritizing:

- Low coupling between systems  
- Event-driven, decoupled communication  
- Clear separation of responsibilities  
- Future extensibility  
- Ease of testing and maintenance  

---

## Principles & Techniques Used

- Dependency Injection (**Zenject**)  
- Event-Driven Architecture (**EventBus**)  
- Facade Pattern for complex systems (Player)  
- State Machines / **HFSM** for gameplay logic  
- ScriptableObjects for data-driven configuration and tooling  
- Extensive use of **interfaces** to define clear contracts  

---

## Core Architecture

The project includes a **reusable Core layer**, independent from game-specific gameplay logic, which provides:

- A **global EventBus** for decoupled communication  
- A **Scene Routing System** supporting:
  - additive scenes
  - preload
  - unload
- **Input abstractions** decoupled from the Unity Input System  
- A reusable **Interaction architecture**  
- A **generic HFSM** implementation based on coroutines  

This layer is designed to be reusable across other projects.

---

## Gameplay Systems

### Modular Player (Facade + Decoupled Input)

- **PlayerFacade** implemented as a single access point to the player  
- Input fully decoupled via input ports  
- Clear separation between:
  - Domain
  - Application
  - Presentation  
- Designed to selectively block player capabilities:
  - movement
  - look
  - pause
  - closeups  

---

### Interaction System + Closeup System

- Interaction system based on **interfaces and interaction context**  
- Interaction targets fully decoupled from input handling  
- **Closeup system** supporting:
  - priorities
  - requests
  - player control locking  
- Clean integration with dialogue system and additive scenes  

---

### Drill HFSM (Core Gameplay Mechanic)

- **Hierarchical Finite State Machine** dedicated to the drill mechanic  
- Clear separation of states and substates  
- Gameplay logic fully decoupled from:
  - UI
  - Audio
  - VFX  
- Event-based communication  
- Example of a core gameplay system designed with extensibility in mind  

---

### Scene Routing / Additive Loading

- Decoupled scene navigation system  
- Supports:
  - immediate transitions
  - asynchronous loading with progress reporting
  - additive scenes
  - preload without activation  
- Uses **SceneReferenceObject** to avoid hard-coded strings  
- Prepared for future **Addressables** integration  

---

### EventBus

- Central EventBus used to decouple systems  
- Extensively used across:
  - gameplay
  - dialogues
  - closeups
  - drill logic
  - scene routing  
- Includes **basic unit tests** validating publish / subscribe behavior  

---

### Cheats & Tools (Data-Driven)

- Cheat system designed as a **development and debugging tool**  
- Fully data-driven via ScriptableObjects  
- Useful for:
  - debugging
  - QA
  - rapid iteration  
- Example of tooling designed with architecture in mind  

---

## Repository Structure

This repository contains **code only**:

2-Scripts/

├── Core/

├── Data/

├── Gameplay/

└── Tests/

It does **not** include:

- Assets  
- Scenes  
- Audio  
- Visual UI  
- Art  

---

## Additional Material

- 🎮 **Playable technical prototype** (Windows)
  
  https://alesc.itch.io/vision-of-the-depths-technical-prototype
  
- 🎥 **1 minute technical overview video** (architecture & systems highlight)
  
  https://www.youtube.com/watch?v=7rdFdFWgqtQ

---

## Repository Purpose

This repository is intended to:

- Showcase architectural decision-making  
- Demonstrate extensibility-oriented design  
- Highlight good practices for gameplay systems  
- Serve as a technical portfolio sample  

---

## Final Notes

This project prioritizes **how systems are designed**, not visual complexity.

Each system was built to be:
- scalable  
- testable  
- maintainable  
- easy to reason about in a team environment  
