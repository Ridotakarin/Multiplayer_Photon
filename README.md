<a name="readme-top"></a>



<h1 align="center">🌐 Unity Real-Time Multiplayer Framework</h1>

<p align="center">
  <img src="https://img.shields.io/badge/Creator-Nhat%20Huy-red?style=for-the-badge&logo=github" />
  <img src="https://img.shields.io/badge/Engine-Unity-black?style=for-the-badge&logo=unity" />
  <img src="https://img.shields.io/badge/Network-Photon-blue?style=for-the-badge" />
</p>

---

## 👋 About This Project

Welcome to **Multiplayer_Photon**. This project is a dedicated research and implementation framework focusing on real-time multiplayer mechanics utilizing **Photon** solutions. 

I built this to solve core networking challenges, specifically focusing on low-latency data synchronization across multiple clients, smooth state interpolation, and building a fully modular lobby matchmaking system from scratch.

---

## 🎬 Gameplay Gallery

This section demonstrates the network architecture and synchronization systems scripted within this project:

### 1. Room Matchmaking & Lobby System
The lobby script manages full room creation logic, allowing players to join existing sessions and synchronize room states globally.
<p align="center">
  <img src="README_assets/room_management.gif" width="90%" />
</p>

### 2. Real-Time State Synchronization 
Character movement and rotation data are synchronized across clients smoothly using custom interpolation to eliminate jitter caused by network latency.
<p align="center">
  <img src="README_assets/sync_movement.gif" width="90%" />
   <img src="README_assets/sync_movement.gif" width="90%" />
</p>

Player names and character are bound and synchronized globally utilizing **Custom Properties**, while RPCs instantly trigger discrete events like combat actions.
<p align="center">
  <img src="README_assets/customization_demo.gif" width="90%" />
</p>

<p align="right">(<a href="#readme-top">back to top</a>)</p>

---

## ⚙️ Technical Features (The Code)

### 1. Connection Management
* **Master Server Connection:** Handles secure asynchronous connection logic to the Photon Cloud Server and handles transition states between Offline/Online gameplay modes.
* **Lobby System:** Dynamic creation, joining, and listing of active game sessions.
* **Player Custom Properties:** Synchronizes localized data (like player usernames and mesh colors) globally across network peers without heavy bandwidth costs.

### 2. Network Synchronization Architecture
* **Transform Interpolation:** Implements `Photon View` and `Photon Transform View` layers to achieve seamless, lag-compensated position updates across separate game clients.
* **Animation Syncing:** Synchronizes active Animator states (`Run`, `Jump`, `Attack`) instantly utilizing the `Photon Animator View` pipeline.
* **RPCs (Remote Procedure Calls):** Handles instantaneous network events—such as firing projectiles, registering hits, and creating impact particles—precisely when they occur.

### 3. Performance & Authority Logic
* **Ownership Transfer:** Handles dynamic object authority management when players interact with shared physics objects or picking up items in the environment.
* **Network Object Pooling:** Optimizes memory and network allocation by reusing projectiles and visual effects rather than calling heavy network instantiation/destruction routines.

---

## 🛠️ Tools & Libraries
* **Engine:** Unity 2022.3+
* **Networking SDK:** Photon Unity Networking 2 (PUN2) / Photon Fusion
* **Language:** Advanced C# (Asynchronous tasks and networked state logic)

---

## 🚀 Installation & Setup

### ⚙️ Quick Start
1. **Clone the Repository:** 
   ```bash
   git clone [https://github.com/Ridotakarin/Multiplayer_Photon.git](https://github.com/Ridotakarin/Multiplayer_Photon.git)
