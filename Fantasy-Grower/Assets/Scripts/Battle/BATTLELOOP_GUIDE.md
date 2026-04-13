# 전투 루프 시스템 사용 가이드

## 개요

방치형 자동 전투 루프 시스템.
플레이어는 AttackSpeed 기반으로 적을 자동 공격하고, 적은 EnemyAI로 플레이어를 공격한다.
던전은 웨이브 단위로 구성되며, 모든 웨이브 클리어 시 보상을 지급한다.

```
BattleManager (상태 머신)
├── WaveController       — 적 스폰/생존 추적
├── AutoAttackController — 플레이어 자동 공격 코루틴
├── DamageCalculator     — 데미지 계산 (DamageReduction + CriticalPercentage)
└── 데이터 SO
    ├── DungeonData      — 던전 전체 (웨이브 목록 + 클리어 보상)
    ├── WaveData         — 단일 웨이브 (적 프리팹 × 수량)
    └── EnemyRewardData  — 적 사망 보상 (Gold, XP)
```

---

## 1. 데이터 에셋 만들기

### 1-1. EnemyRewardData SO 생성

`Assets > Create > Battle/EnemyRewardData`

| 필드 | 설명 |
|------|------|
| `goldAmount` | 적 사망 시 지급되는 Gold 양 |
| `xpAmount`   | 적 사망 시 지급되는 XP 양  |

### 1-2. WaveData SO 생성

`Assets > Create > Battle/WaveData`

| 필드 | 설명 |
|------|------|
| `entries[]` | 스폰할 적 프리팹과 수량 목록 |

**예시 (웨이브 1):**
- entries[0]: TestEnemyPrefab × 3
- entries[1]: EliteEnemyPrefab × 1

### 1-3. DungeonData SO 생성

`Assets > Create > Battle/DungeonData`

| 필드 | 설명 |
|------|------|
| `dungeonType` | `Basic` / `Gold` / `Weapon` / `Boss` |
| `waves[]` | 웨이브 SO 목록 (순서대로 진행) |
| `bonusGoldReward` | 던전 클리어 보너스 Gold |
| `bonusXpReward`   | 던전 클리어 보너스 XP |
| `mithrilAsset`    | Boss 던전 전용 — Mithril SO 연결 |
| `mithrilRewardAmount` | Boss 클리어 시 Mithril 지급량 |

---

## 2. 적 프리팹 구성하기

Enemy 프리팹에는 다음 컴포넌트가 있어야 한다:

| 컴포넌트 | 용도 |
|----------|------|
| `Enemy` (또는 서브클래스) | 전투 엔티티, 사망 보상 지급 |
| `EnemyAI` | 자동 공격 코루틴 |
| `EntityStatData` SO 연결 | HP/공격력/공격속도 등 스탯 |
| `EnemyRewardData` SO 연결 | 사망 보상 |
| `SO_Gold` / `SO_XP` SO 연결 | 보상 지급 대상 |

**AttackCollider가 있는 경우 (물리 판정):**
- 프리팹 하위에 `AttackHitbox` 오브젝트 추가
- `AttackCollider.cs` 부착, `type = Enemy`, Collider2D(trigger) 부착
- EnemyAI는 `Attack()` → 충돌로 피해 처리

**AttackCollider가 없는 경우 (직접 호출):**
- EnemyAI가 `DamageCalculator`를 사용하여 `_player.TakeDamage()` 직접 호출

---

## 3. 씬 구성하기

```
BattleScene
├── BattleManager       [BattleManager.cs]
├── WaveController      [WaveController.cs]
├── SpawnPoints/
│     ├── SpawnPoint_1  (빈 GameObject)
│     └── SpawnPoint_2
└── Warrior             [Warrior.cs] [AutoAttackController.cs]
      └── AttackHitbox  [AttackCollider.cs] type=Player
```

**BattleManager Inspector 연결:**

| 필드 | 연결 대상 |
|------|-----------|
| `_player` | Warrior GameObject |
| `_autoAttack` | Warrior의 AutoAttackController 컴포넌트 |
| `_waveController` | WaveController GameObject |
| `_spawnPoints` | SpawnPoint_1, SpawnPoint_2 ... |
| `_dungeonData` | 위에서 만든 DungeonData SO |
| `_gold / _xp / _mithril` | 프로젝트의 재화 SO 에셋 |

**AutoAttackController Inspector 연결:**

| 필드 | 연결 대상 |
|------|-----------|
| `_waveController` | WaveController GameObject |

---

## 4. 전투 루프 상태 흐름

```
Idle
  ↓ StartDungeon() 호출
WaveStart  ← (1.5초 딜레이 후 다음 웨이브)
  ↓ 적 스폰 + EnemyAI 초기화
Fighting
  ↓ OnAllEnemiesDead (WaveController → BattleManager)
WaveCleared
  ↓ 마지막 웨이브였다면
DungeonCleared  → 보너스 보상 지급
  ↓ 중간 웨이브였다면 1.5초 후 WaveStart 반복
  
(언제든) 플레이어 HP = 0
PlayerDead  → RetryDungeon() 또는 Exit 처리
```

---

## 5. 데미지 계산 공식

`DamageCalculator.Calculate(rawAttackPower, targetDamageReduction, attackerCriticalPercentage)`

```
감소 데미지 = Max(1, RoundToInt(공격력 × (1 - DamageReduction)))
크리티컬 여부 = Random(0~100) < CriticalPercentage
최종 데미지 = 크리티컬 ? 감소 데미지 × 2 : 감소 데미지
```

| 스탯 | 범위 | 예시 |
|------|------|------|
| `DamageReduction` | 0.0 ~ 1.0 | `0.2` = 20% 피해 감소 |
| `CriticalPercentage` | 0 ~ 100 | `25` = 25% 크리티컬 확률 |

---

## 6. 런타임 사용법

### 던전 시작 (UI 버튼)
```csharp
BattleManager battleManager = FindObjectOfType<BattleManager>();
battleManager.StartDungeon();
```

### 재시도 (사망 화면 버튼)
```csharp
battleManager.RetryDungeon();
```

### 상태 변화 구독 (UI 패널 전환)
```csharp
battleManager.OnStateChanged += state =>
{
    switch (state)
    {
        case BattleState.Fighting:      ShowBattleUI();   break;
        case BattleState.DungeonCleared: ShowClearUI();   break;
        case BattleState.PlayerDead:    ShowDeadUI();     break;
    }
};

battleManager.OnWaveChanged += waveIndex =>
{
    waveText.text = $"Wave {waveIndex + 1}";
};
```

---

## 7. 새 던전 타입 추가 방법

1. `DungeonType` 열거형에 새 타입 추가
2. `DungeonData` SO 생성 후 `dungeonType` 선택
3. `BattleManager.StartDungeon()`에 분기 추가 (Gold 던전처럼 별도 씬이 필요한 경우)
4. `EnterDungeonCleared()`에 타입별 특수 보상 로직 추가

---

## 8. 클래스 의존 관계

```
[DungeonData] ──► [WaveData] ──► EnemyPrefab
      │
      ▼
[BattleManager]
  ├── [AutoAttackController] ──► [WaveController]
  │         │                         │
  │    Player.Attack()           SpawnWave()
  │         │                    OnAllEnemiesDead
  │         ▼                         │
  │   [AttackCollider] ──► [DamageCalculator] ◄── [EnemyAI]
  │                              ▲
  │                         Entity.TakeDamage()
  └── Entity.OnDied ──► HandlePlayerDied / OnEnemyDied

[Enemy] ──► [EnemyRewardData]
         ──► SO_Gold / SO_XP  (사망 시 Increase)
```
