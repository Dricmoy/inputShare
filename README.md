# InputShare — Seamless Multi-PC Extended Desktop
<img width="399" height="356" alt="image" src="https://github.com/user-attachments/assets/5a5bc355-13d3-4869-84f0-ed83b82b6a1e" />

InputShare is a **cross-machine extended desktop system** that allows a user to seamlessly move their mouse and keyboard control between multiple PCs as if they were a single system.  
It enables low-latency input sharing between a **Sender PC** and a **Receiver PC** for a smooth and unified desktop experience.

## Components

### 1. Monitor Manager GUI (.NET Client)
- Built with .NET Core / WPF / Avalonia.
- Allows users to:
  - Configure monitor layouts.
  - Enable/disable input logger.
  - Toggle input sharing.
- Communicates with the Java backend via REST API.

### 2. Java Backend (Sender Input Manager)**
- Runs on the **Sender PC**.
- Responsibilities:
  - Runs native mouse/keyboard hooks using [JNativeHook](https://github.com/kwhat/jnativehook).
  - Tracks monitor layout and boundaries.
  - Hosts a REST API for the .NET GUI.
  - Streams real-time input events to the Receiver backend.
  - Manages simulation control commands.

### 3. Receiver Input Simulation Backend
- Runs on Receiver PC without GUI.
- Responsibilities:
  - Receive simulation commands from Sender backend.
  - Inject mouse/keyboard events locally.
  - Handle boundary crossing and return control.

## Communication Flow
<img width="271" height="538" alt="image" src="https://github.com/user-attachments/assets/b0a0b1d5-00fb-4d76-83b1-76386858b091" />

## Features currently in the works

1. **Normal Operation**  
   Sender PC handles all inputs locally. GUI shows layout configuration.

2. **Boundary Crossing Detection**  
   Backend detects cursor crossing a configured monitor boundary.

3. **Simulation on Receiver PC**  
   Backend sends simulation commands + events to Receiver backend, which injects them locally.

4. **Returning Control**  
   Receiver backend stops simulation and control returns to Sender PC.

## Longterm Future Plans
* Multi-receiver support.
* Custom hotkeys for input sharing.
* Lower latency streaming with UDP or gRPC.
* Drag-and-drop monitor configuration GUI.
* Secure authentication for Receiver backend.

