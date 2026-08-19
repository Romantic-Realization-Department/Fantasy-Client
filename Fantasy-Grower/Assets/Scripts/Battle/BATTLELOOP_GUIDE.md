# 전투 루프 시스템 사용 가이드

## 개요

방치형 자동 전투 루프 시스템.
플레이어는 `AutoAttackController`를 통해 `AttackSpeed` 기반으로 자동 공격을 수행하며, 적은 `EnemyAI`를 통해 플레이어를 공격합니다.
전투는 `BattleManager`가 제어하는 상태 머신을 따르며, 던전은 웨이브 단위로 구성됩니다.

```
BattleManager (상태 머신)
├── WaveController       — 적 스폰/생존 추적 및 OnAllEnemiesDead 이벤트
├── AutoAttackController — 플레이어 자동 공격 루프 (IAttackEvent 구현)
├── AttackTargetsSensing — 사거리 내 타겟 감지 및 공격 시작/중지 트리거
└── 데이터 SO
    ├── DungeonData      — 던전 전체 (웨이브 목록, 유형, 클리어 보상)
    ├── WaveData         — 단일 웨이브 (적 프리팹 × 수량)
    └── EnemyRewardData  — 적 사망 시 기본 보상 (Gold, XP)
```

---

## 1. 데이터 에셋 만들기

### 1-1. EnemyRewardData SO 생성
`Assets > Create > Battle > EnemyRewardData`
- `GoldAmount`: 처치 시 지급될 골드 수량
- `XpAmount`: 처치 시 지급될 경험치 수량

### 1-2. WaveData SO 생성
`Assets > Create > Battle > WaveData`
- `Entries`: 스폰할 적 프리팹과 해당 마리수 목록

### 1-3. DungeonData SO 생성
`Assets > Create > Battle > DungeonData`
- `DungeonType`: 던전의 성격 (Basic, Gold, Weapon, Boss)
- `Waves`: 진행할 웨이브 SO 목록
- `BonusGoldReward`: 던전 전체 클리어 시 보너스 골드
- `BonusXpReward`: 던전 전체 클리어 시 보너스 경험치
- `MithrilRewardAmount`: 보스 던전 등에서 지급할 미스릴 수량
- `IsExistEnd`: 마지막이 존재하는 던전인지 여부
- `IsChangeable`: 성과에 따라 보상을 동적으로 변경할 수 있는지 여부 (예: 골드 던전)

---

## 2. 캐릭터(엔티티) 프리팹 구성

모든 전투 단위(Player, Enemy)는 `Entity`를 상속받으며 다음 구조를 가집니다.

| 컴포넌트 | 용도 |
|----------|------|
| `Player` / `Enemy` | 핵심 엔티티 로직 및 스탯 적용 |
| `AttackTargetsSensing` | 사거리(Collider2D Trigger) 내 타겟 감지 |
| `AutoAttackController` / `EnemyAI` | 공격 주기를 관리하는 루프 (`IAttackEvent`) |
| `EntityAnimation` | `PlayerState` 변경에 따른 SPUM 애니메이션 재생 |
| `Rigidbody2D` | 적의 이동(Move) 등에 필요 |

**공격 판정 흐름:**
1. `AttackTargetsSensing`이 타겟을 감지하면 `IAttackEvent.StartAttacking()` 호출.
2. 루프(코루틴) 내에서 `Entity.Attack()` 호출.
3. `Warrior`나 `TestEnemy`의 `Attack()` 오버라이드 메서드 내에서 `DamageCalculator`를 사용하여 실제 피해 입힘.

---

## 3. 씬 구성 및 연결

### BattleManager (오케스트레이터)
- `player`: 씬 내의 Player 오브젝트 연결
- `autoAttack`: Player의 AutoAttackController 연결
- `waveController`: 씬 내의 WaveController 연결
- `spawnPoints`: 적이 생성될 Transform 지점들
- `dungeonData`: 진행할 던전 데이터 SO

**※ 중요:** 현재 재화 시스템은 `GoodsManager.Instance`를 사용하므로 Inspector에서 개별 재화 SO를 연결할 필요가 없습니다.

---

## 4. 전투 루프 상태 흐름 (BattleState)

1. **Idle**: 던전 시작 전 대기 상태.
2. **WaveStart**: `WaveController`를 통해 적 스폰, UI에 웨이브 알림.
3. **Fighting**: 적과 플레이어가 서로 공격하는 실전 상태.
4. **WaveCleared**: 모든 적 처치 시 진입. 1.5초 후 다음 웨이브 혹은 클리어로 전환.
5. **DungeonCleared**: 모든 웨이브 종료. 보너스 보상 지급(`GoodsManager`).
6. **PlayerDead**: 플레이어 사망 시 진입. 던전 중단 및 실패 처리.

---

## 5. 데미지 계산 및 스탯 적용

- **데미지**: `DamageCalculator.Calculate`를 통해 공격력, 방어력, 크리티컬 확률을 계산합니다.
- **스탯 반영**: 패시브 스킬 등이 해금되면 `Entity.ApplyStatModifier`를 호출하여 런타임 스탯을 갱신합니다.

---

## 6. 클래스 의존 관계 (최신)

```
[DungeonData] ──▶ [WaveData] ──▶ EnemyPrefab
      │
      ▼
[BattleManager] (상태 관리)
  ├── [AutoAttackController] (공격 루프) ◀── [AttackTargetsSensing] (타겟 감지)
  │         │                                      │
  │    Entity.Attack() ◀───────────────────────────┘
  │         │
  │   [DamageCalculator] ──▶ Entity.TakeDamage()
  │
  └── [WaveController] (스폰/추적) ──▶ Entity.OnDied

[Enemy/Player] ──▶ GoodsManager.Instance (보상/재화 처리)
```
