# Project AI Rules (AGENTS.md)

## 1. C# Naming Conventions
- **Private Fields, Locals, Parameters**: Strictly use `camelCase`. **NEVER** use Hungarian notation or prefixes like `_` or `m_` (e.g., use `private int attackPower;`).
- **Public Fields, Classes, Structs, Methods, Properties**: Strictly use `PascalCase` (e.g., `public class MyClass`, `public void MyMethod()`).
- **ScriptableObjects**: Strictly use `PascalCase` with NO prefixes. (e.g., `GoodsBase`. NEVER use `SO_Goods`).

## 2. Unity Component Validation
- Use `OnValidate()` to catch missing Inspector references or setup errors, and log explicit `Debug.LogError` messages. Do not use hardcoded workarounds for missing references.

## 3. Code Documentation
- Write detailed comments in **Korean** when adding or modifying logic (especially architecture or core APIs) so human developers can easily understand the intent.

## 4. Scalability & SOLID Principles
- Strictly adhere to SOLID principles (especially SRP and OCP).
- Avoid temporary workarounds or hardcoding. Design modular, highly scalable architectures to accommodate massive numbers of future skills/items with minimal code changes.

## 5. Project Characteristics (Idle RPG)
- **Genre**: 2D Pixel Art Fantasy Idle RPG (Unity 6.0.0+, URP 2D, New Input System).
- **Core Loop**: Auto-combat driven by FSM-based `Entity` components and a global `GameManager`.
- **Direction**: Code must dynamically handle practically infinite scaling of stats, skills, equipment, and passives.

## 6. Combat Architecture
- **Hit Detection**: Must be executed using 2D Triggers via the `AttackCollider` component.
- **Friendly Fire**: Strictly use the `EntityType` Enum (`Player` vs `Enemy`) to prevent friendly fire.
- **Damage**: Apply the attacker's `AttackPower` directly into the target's `TakeDamage()` method.

## 7. Currency System Constraints
- **XP Modification**: Never call `Decrease()` on XP currency (it is marked `[Obsolete]`).
- **Overspending**: Always perform a Range Check (balance validation) before consuming any currency to prevent negative balances.

## 8. Workflow & Environment
- **NO CLI Builds**: Do not use command-line build scripting. Perform all testing and running exclusively inside the Unity Editor (Play button, Test Runner).

## 9. GDD (Game Design) Context
- **Skill Trees**:
  - Warrior: Free branching tree.
  - Archer: TFT-style "Pick 1 of 3" per row.
  - Mage: Linear tree separated by elements (Fire, Ice, Wind).
- **Dungeons**:
  - Basic: Idle wave auto-combat.
  - Gold: 30~60s click/tap mining minigame. DO NOT implement standard combat here.
  - Weapon/Boss: Specialized reward pools (C-grade weapons vs Mithril + A-grade weapons).
- **Weapon Grades**: S > A > B > C. The highest 'S' grade cannot be dropped; it is ONLY craftable via the Synthesis system using Mithril.

## 10. Code Formatting
- **CSharpier**: All C# code must be formatted strictly following the **CSharpier** formatting style. Do not use custom formatting or spacing that contradicts CSharpier conventions.
