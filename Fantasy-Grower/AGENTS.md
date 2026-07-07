# Project AI Rules (AGENTS.md)

## 0. Scope and Legacy Code Policy
- These rules apply to all code and assets newly created or modified from this point forward.
- Existing violations are **grandfathered**. Do not perform unrelated mass refactors solely to make legacy code conform.
- When editing a legacy file, make the requested change conform to these rules while preserving unrelated behavior and user changes.
- Do not rename serialized fields, move assets, or redesign public APIs merely for style compliance unless the task explicitly requires it.
- Explicit user instructions and the current GDD take precedence over this document when they conflict.

## 1. C# Naming Conventions
- **Private Fields, Locals, Parameters**: Strictly use `camelCase`. **NEVER** use Hungarian notation or prefixes like `_` or `m_` (e.g., use `private int attackPower;`).
- **Public Fields, Classes, Structs, Methods, Properties**: Strictly use `PascalCase` (e.g., `public class MyClass`, `public void MyMethod()`).
- **ScriptableObjects**: Strictly use `PascalCase` with NO prefixes. (e.g., `GoodsBase`. NEVER use `SO_Goods`).
- Prefer private `[SerializeField]` fields plus public read-only properties over mutable public fields.
- Use stable, descriptive names. Do not encode temporary implementation details in type or member names.

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

## 11. Unity and C# Compatibility
- The project targets Unity 6 and C# 9, but available BCL APIs are determined by Unity's configured API compatibility level. Do not assume that every API from modern standalone .NET is available.
- Before introducing a new runtime or BCL type, verify that Unity can compile it under the project's current API compatibility settings.
- Prefer APIs already used successfully in the project when an equivalent modern API has uncertain Unity support.
- Never edit Unity-generated `.csproj` or `.sln` files manually. Configure compatibility and packages through Unity.
- Do not add, remove, or upgrade packages without explicit user approval.

## 12. Unity Serialization and Asset Safety
- Preserve `.meta` files and Unity GUIDs. Never recreate, delete, or move assets casually.
- When renaming a serialized field, use `[FormerlySerializedAs]` or provide an explicit migration so existing scenes, prefabs, and ScriptableObjects retain their values.
- Do not change a serialized field's type without checking all affected scenes, prefabs, and assets.
- ScriptableObjects define shared game content and configuration. Do not store per-player mutable runtime state in shared ScriptableObject assets.
- Keep player progression/state separate from definitions such as weapon, skill, dungeon, wave, reward, and stat data.
- Do not directly edit Unity YAML assets unless necessary and understood; prefer Unity Editor operations for structural scene/prefab changes.

## 13. Inspector and Runtime Validation
- `OnValidate()` is an editor-time aid, not a runtime safety mechanism. Public entry points and lifecycle methods must still handle invalid or missing dependencies safely.
- Use `NaughtyAttributes` such as `ShowIf`, `HideIf`, and validation attributes when they materially improve Inspector clarity.
- Log validation errors with the component context (`this`) and enough information to identify the affected object and field.
- Do not spam logs every frame or repeatedly report the same recoverable error.
- Required component dependencies should use `[RequireComponent]` where appropriate.

## 14. Lifecycle, Events, Coroutines, and Tweening
- Every event subscription must have a matching unsubscription at the appropriate lifecycle boundary.
- Stop or kill owned coroutines and tweens when their owner is disabled or destroyed when continued execution could access stale objects.
- Before starting a replacement tween, kill the previous tween that writes to the same state or UI element.
- Guard delayed callbacks against destroyed or invalid Unity objects.
- Avoid static events unless global broadcast semantics are genuinely required; static subscribers must always unsubscribe.
- Singleton components must reject duplicates safely and must not destroy unrelated components attached to the same GameObject.

## 15. Performance and Allocation Policy
- Optimize measured or clearly hot paths such as per-frame combat, frequently refreshed UI, and repeated spawning. Do not complicate cold code for speculative micro-optimization.
- Avoid repeated LINQ, reflection, `Find*`, string interpolation, and avoidable allocations in `Update`, combat loops, and high-frequency UI callbacks.
- For allocation-sensitive TMP number displays, prefer reusable `char[]`/`Span<char>`, project `SpanExtension` helpers, and `TMP_Text.SetCharArray` when Unity compatibility is verified.
- Reuse existing caches such as `YieldInstructionCache` for repeated fixed waits.
- Object pooling is preferred for frequently spawned combat effects and entities once their spawn rate justifies it.

## 16. Persistence, Networking, and Security
- PlayerPrefs is only for non-sensitive device-local preferences. Never treat it as authoritative storage for currency, progression, inventory, equipment, skills, dungeon records, or credentials.
- Never store passwords, session secrets, or long-lived authentication tokens in plain PlayerPrefs or unencrypted JSON.
- Server-authoritative values must be validated and calculated by the server; never trust client-submitted rewards, prices, or balances.
- Persist stable string IDs for externally stored data. Do not persist Unity object references or rely on enum ordinal values as long-term network/database identifiers.
- Network mutations that grant or consume value must be idempotent and safe against duplicate requests.
- Keep DTOs separate from Unity domain objects and ScriptableObjects.

## 17. Error Handling and Data Integrity
- Validate currency balance, index bounds, null references, maximum levels, and state transitions before mutating state.
- Avoid unsigned subtraction when the minuend may be smaller; validate first to prevent underflow.
- A multi-step operation such as purchase, synthesis, upgrade, reward grant, or skill unlock must either complete fully or leave state unchanged.
- Do not silently swallow failures. Return a meaningful result or log a clear error at the correct ownership layer.
- Do not use placeholder conditions such as `if (true)` or ship test-only behavior in production paths.

## 18. Testing, Verification, and Git Safety
- Do not claim Unity compilation, Play Mode behavior, or Inspector wiring was verified unless it was actually checked in the Unity Editor.
- Add EditMode or PlayMode tests for deterministic core logic when practical, especially currency consumption, rewards, damage, skill unlock rules, and progression calculations.
- After code changes, inspect the diff and run whitespace/error checks. Report any verification that could not be performed.
- Preserve unrelated working-tree changes. Never reset, discard, overwrite, stage, or commit user work unless explicitly requested.
- Keep commits focused on one coherent concern and use Conventional Commit-style subjects such as `feat:`, `fix:`, `refactor:`, and `docs:`.
- Do not include generated caches, build output, local IDE state, credentials, or machine-specific files in commits.
