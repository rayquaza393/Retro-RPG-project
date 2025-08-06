# Retro RPG Dungeon Crawler - Project Overview

A low-poly, first-person dungeon crawler inspired by retro classics like *Champions: Return to Arms*, *Lunacid*, *Ultima Underworld*, and *EverQuest (1999)*. Built using the Godot Engine with PS1/early-PC era aesthetics and simple but immersive gameplay systems.

## 🎮 Core Gameplay Concept

- Explore dark, atmospheric dungeons in first-person
- Hack and slash through low-poly enemies
- Discover secrets and lore
- Level up, unlock new powers, and equip rare loot
- Retro-inspired visuals with slightly modernized UI (inspired by Morrowind/Ultima Underworld)

---

## 🧱 Core Systems Breakdown

### 1. Player System

- First-person movement and mouse look
- Health, stamina, and basic stats
- Handles weapon/tool usage and animations

### 2. Input System

- Keyboard + mouse support
- Action key bindings (attack, interact, pause)
- Planned modular input handler (for rebinding and future controller support)

### 3. Combat System

- Melee combat with hit detection and cooldowns
- Future additions: ranged weapons, blocking, critical hits

### 4. Enemy/AI System

- Finite state machine behavior (idle, chase, attack, die)
- Pathfinding for navigating dungeon tiles
- Drops loot or XP on death

### 5. Dungeon System

- Modular room and hallway tiles
- Scene streaming for large dungeons
- Triggers and interactables placed in layout

### 6. UI System

- Health/stamina HUD
- Inventory and item management
- Dialogue and interaction prompts
- Menu/pause screen

### 7. Inventory & Equipment

- Equip/unequip system for weapons and armor
- Stackable consumables (potions, scrolls)
- Stat-modifying gear with rarity tiers

### 8. Save/Progression System

- Player data persistence (level, stats, inventory)
- Level completion tracking
- Unlockables (e.g., improved abilities, new areas)

### 9. Audio System

- Footsteps, ambient dungeon sounds
- Combat effects (sword swings, enemy groans)
- Background music for areas

### 10. Interaction System

- Interactables like chests, levers, doors
- Raycast or trigger-based detection
- Popup prompts and action responses

---

## 🔧 Development Roadmap

### ✅ Current Progress

- Player movement prototype (Player\_Proto\_1)
- Mouse sensitivity adjusted
- Test area with ground tiles

### 🟨 Next Steps

- Basic melee combat (weapon swing + damage)
- Simple enemy AI (chase + attack)
- Health UI
- Pause menu with ESC
- Basic dungeon tile layout

### 🔮 Future Plans

- Loot and item pickups
- Inventory screen
- Story/quest system
- Expanded dungeon biomes and bosses

---

## 🛠 Tools

- **Game Engine**: [Godot 4.x](https://godotengine.org/)
- **3D Modeling**: Blender (low-poly style)
- **Version Control**: Git + GitHub

---

## 📁 Project Structure

```
/res
  /player
  /enemy
  /items
  /scenes
  /ui
  /dungeon
  /audio
```

---

## 🤝 Credits & Inspiration

- *Lunacid* by KIRA LLC
- *Shadow Tower*, *King's Field* (FromSoftware)
- *Ultima Underworld* (Looking Glass Studios)
- *EverQuest*, *DAoC*, *Morrowind*

---

## 📌 Notes

> This project is in early prototyping. Systems and assets are subject to overhaul as core mechanics are refined and tested. Goal is a modular, clean architecture that can scale.

---

Want to contribute or follow progress? Star this repo and check back for updates.

