# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Fantasy Grower** — 2D Idle RPG Mobile Game (Unity 6.0.0, Android)

Players build characters by combining jobs, weapons, and skill trees, then progress through dungeons via auto-combat.

## Development Environment

- **Engine**: Unity 6.0.0+
- **IDE**: Visual Studio 2022 or JetBrains Rider (open `Fantasy-Grower.sln`)
- **Rendering**: Universal Render Pipeline (URP) 2D
- **Input**: Unity New Input System
- **Platform**: Android (target), Desktop (test)

No CLI build commands. All build/run operations are performed inside the Unity Editor:
- **Play**: Unity Editor > Play button
- **Build**: File > Build and Run
- **Tests**: Window > General > Test Runner (Unity Test Framework)

## C# Naming Conventions

Follows Microsoft C# naming conventions.

| Target | Rule | Example |
|--------|------|---------|
| Class / Struct | PascalCase | `EntityStatData`, `AttackCollider` |
| Interface | `I` + PascalCase | `ISkillData`, `IAttackable` |
| Method | PascalCase | `TakeDamage()`, `GetGoods()` |
| Property | PascalCase | `AttackPower`, `MaxHealth` |
| public field | PascalCase | `public int AttackPower;` |
| private field | camelCase | `private int attackPower;` |
| Local variable | camelCase | `int currentHp = 0;` |
| Parameter | camelCase | `void TakeDamage(int damageAmount)` |
| Constant (`const`) | PascalCase | `const int MaxLevel = 100;` |
| enum type | PascalCase | `EntityType` |
| enum value | PascalCase | `EntityType.Player`, `EntityType.Enemy` |
| ScriptableObject class | PascalCase (no prefix) | `GoodsBase`, `SkillData` (~~`SO_Goods`~~ forbidden) |

> **Note**: Hungarian prefixes such as `SO_`, `m_`, `_` are not allowed.

## Code Architecture

### Class Hierarchy

```
MonoBehaviour
└── Entity (abstract)          — HP, AttackPower, TakeDamage(), Death()
    ├── Player                 — Implements Attack()
    │   └── Warrior            — Swordsman subclass
    └── Enemy                  — Enemy base
        └── TestEnemy          — Overrides Death()
```

### Key Design Patterns

**ScriptableObject-based data separation**
- `EntityStatData` — Stores stats: HP, AttackPower, AttackSpeed, CriticalChance, etc.
- `SkillData` (abstract) — Skill data base class
- `GoodsBase` family — Currency system (Gold, XP, SP, Mithril, UpgradeScroll)

**Combat hit detection**
- `AttackCollider` component handles damage via 2D Trigger collision
- `EntityType` enum prevents friendly fire (`EntityType.Player` hits `EntityType.Enemy` only, and vice versa)
- Damage = attacker's `AttackPower` applied directly

**Goods (Currency) system**
- `GoodsBase` abstract class: `Get()`, `Increase()`, `Decrease()`
- XP cannot call `Decrease()` (marked `[Obsolete]`)
- Range checks prevent overspending

### Directory Structure

```
Assets/Scripts/
├── Battle/
│   ├── Entity.cs              — Combat entity base class
│   ├── AttackCollider.cs      — Attack hitbox component
│   ├── Player/                — Player classes
│   └── Enemy/                 — Enemy classes
└── Core/
    ├── EntityStatData.cs      — Stat ScriptableObject
    ├── SkillData.cs           — Skill data base
    └── Goods/                 — Currency ScriptableObject family
```

---

## Game Design Document Summary

> Use this as the reference for all implementation decisions.

### Growth Resources

| Resource | Usage |
|----------|-------|
| Gold | Buy/upgrade weapons, refresh shop |
| XP | Character level up |
| SP (Skill Point) | Upgrade skill tree; gained on level up |
| Upgrade Scroll | Weapon upgrade (separate scroll per weapon type) |
| Mithril | Weapon synthesis |

### Job System (3 types)

| Job | Traits |
|-----|--------|
| Warrior | High HP, stable combat, AoE/single-target builds |
| Archer | Low HP, crit-focused, weapon passives are key |
| Mage | Medium HP, specializes in one element: Fire / Ice / Wind |

**Mage element traits**:
- Fire: DoT damage, AoE
- Ice: Reduces enemy move/attack speed (utility)
- Wind: Increases attack speed, low cooldowns

### Weapon System

**Grades**: S > A > B > C
- A and above: provide special passives
- S grade: craftable only via synthesis (smelting)

**Job weapons**:
- Warrior: Rapier (crit), Longsword (balanced), Greatsword (attack power)
- Archer: Longbow (attack+crit), Crossbow (attack speed+utility)
- Mage: Staff (cooldown reduction), Spellbook (element boost)

**Equipment management**:
- Upgrade: Uses Upgrade Scrolls; higher grade requires more scrolls
- Synthesis: Mithril + 2 weapons → new weapon (average grade)
- Sell: Gain Gold

### Dungeon System (4 types)

| Dungeon | Description |
|---------|-------------|
| Basic Dungeon | Auto-combat; rewards Gold+XP; wave progression; elite monsters appear |
| Gold Dungeon | Mining minigame; tap screen to mine; 30–60s time limit; rare Mithril drop |
| Weapon Dungeon | Auto-combat; drops C-grade weapons; low chance for B-grade/scrolls |
| Boss Dungeon | Many powerful elites; rewards Mithril+A-grade weapon+XP on clear |

### Shop System

- **Blacksmith**: Random weapon stock; refresh with Gold; customer rank system (rank up by buying/upgrading → better weapon grades available)
- **Magic Shop**: Sells Upgrade Scrolls; refresh with Gold

### Skill Tree

Common rules: Gain SP on level up; equip up to 3 active skills; passives and actives are separate trees.

#### Warrior — Active Tree (branching)

| Tier | Skill | Type | Description |
|------|-------|------|-------------|
| Basic | Slash | Basic Attack | Hits 2 enemies, low damage, fast attack speed |
| Basic | Thrust | Basic Attack | Hits 1 enemy, high damage, slow attack speed |
| Tier 1 | Spin Slash | Active | Hits 3 enemies, low damage, normal cooldown |
| Tier 1 | Headbutt | Active | Hits 2 enemies, medium damage, long cooldown, 2s stun |
| Tier 1 | Pierce | Active | Hits 2 enemies, high damage, fairly long cooldown |
| Tier 1 | Rapid Thrust | Active | Hits 1 enemy, high damage, normal cooldown |
| Tier 2 | Sword Dance | Active | Hits 4 enemies, low damage, long cooldown |
| Tier 2 | Endure | Active | Long cooldown; reduces HP by half per enemy present |
| Tier 2 | Breath | Active | Very long cooldown; resets all skill cooldowns except itself |
| Tier 2 | One-Sword | Active | Hits 1 enemy, high damage, long cooldown; insta-kill on crit |
| Final | Final Skill | Active | (TBD) |

#### Warrior — Passive Tree (2 branches)

**Defense line**: Cloth Armor (damage reduction) → Preparation (reduce all skill CDs) → Broken Blade (recover n HP/s below 50% HP) → Tempering (max HP + attack power n%)

**Offense line**: Quick Stone (attack speed up) → Slow Starter (attack power increases per kill, resets on entering dungeon) → Broken Armor (recover n% damage dealt below 50% HP) → Tempering (shared final node)

#### Archer — TFT-style Pick 1 of 3 (3 rows)

Choose 1 option per row.

| Row | Skills |
|-----|--------|
| Row 1 | Focus (next attack deals n× damage), Arc Shot (AoE, low damage), Multi-Shot (2 shots at 75% damage), Heavy Arrow (pierce + slow debuff), Sixth Sense (bonus damage to elite monsters) |
| Row 2 | Fire (basic: single, +20% crit), Rapid Fire (5s crit window), Silver Arrow (100% on attack), Falling Shot (+n% attack speed), Headshot (n% of max HP damage), Expertise (range increase) |
| Row 3 | Spread Shot (hits 3 enemies), Snipe (single, very high damage), Adversity (+n% crit rate), Poison Arrow (3 shots, very high damage), Glory (crit damage increase) |

#### Mage — Linear Tree per Element

**Fire**: Fire Element (basic, AoE) → Burn (passive: n damage/s status) → Fireball (single) → Fire Pillar (ground AoE) → [Ash (bonus burn damage) / Explosion (AoE size up)] → Meteor (AoE, very long CD) → [Magic Mastery (active damage+) / 4th Degree Burn (burn stackable)]

**Ice**: Ice Element (basic, single, slow) → Frostbite (passive: reduces move/attack speed) → Ice Shatter (AoE) → Ice Shard (single, high damage) → [Chill Boost (stronger frostbite) / Ice Armor (damage reduction for 10s on hit)] → Blizzard (AoE, very long CD) → [Magic Mastery / Sub-Zero (frostbite stacks → immobilize)]

**Wind**: Wind Element (basic, hits 2, fast) → Gale (passive: stack 1 wound per attack, deal damage at 5 stacks) → Wind Blade (15s attack speed buff, hits 3) → Tornado (AoE, knockback effect) → [Accelerate (chance to ignore enemy attack) / Tailwind (chance for extra basic attack)] → Gust (AoE, very long CD) → [Magic Mastery / Abyss (Gale proc resets all skill CDs)]

### Game Loop

```
Clear dungeon → Earn Gold/XP → Level up + gain SP → Upgrade skill tree → Acquire/upgrade weapons → Challenge higher dungeons
```