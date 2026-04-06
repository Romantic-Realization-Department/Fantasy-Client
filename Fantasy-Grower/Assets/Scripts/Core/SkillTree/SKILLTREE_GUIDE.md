# 스킬 트리 시스템 사용 가이드

## 개요

스킬 트리 시스템은 ScriptableObject 기반 데이터 설정 + 전략 패턴 기반 해금 규칙으로 구성된다.
직업별 트리 방식(검사 분기형 / 궁수 행-택1 / 법사 선형)을 코드 수정 없이 에디터에서 선택할 수 있다.

```
SkillData (추상)
├── ActiveSkillData (추상)  → 구체 스킬 클래스 상속
└── PassiveSkillData (추상) → 구체 스킬 클래스 상속

SkillNodeData (SO)  → 트리의 단일 노드, 에디터에서 연결
SkillTreeData (SO)  → 직업별 트리 전체, StrategyType 선택
SkillTreeComponent  → 플레이어에 붙는 런타임 컴포넌트
```

---

## 1. 스킬 클래스 만들기

### 1-1. 액티브 스킬

`ActiveSkillData`를 상속하고 `UseSkill()`을 구현한다.
`[CreateAssetMenu]`를 붙여 에디터에서 에셋 생성이 가능하게 한다.

```csharp
[CreateAssetMenu(menuName = "ScriptableObjects/SkillData/Warrior/WhirlSlash")]
public class WhirlSlashData : ActiveSkillData
{
    public int HitCount = 3;

    public override void UseSkill()
    {
        // 회선 베기: 전방 3마리 공격
        Debug.Log($"회선 베기 발동! {HitCount}마리 공격");
    }
}
```

### 1-2. 패시브 스킬

`PassiveSkillData`를 상속하고 `ApplyPassive()`에서 `EntityStatModifier`를 수정한다.
`UseSkill()`은 구현하지 않아도 된다 (부모에서 빈 구현으로 봉인).

```csharp
[CreateAssetMenu(menuName = "ScriptableObjects/SkillData/Warrior/SpeedUp")]
public class SpeedUpData : PassiveSkillData
{
    public float BonusAttackSpeed = 0.3f;

    public override void ApplyPassive(ref EntityStatModifier modifier)
    {
        modifier.BonusAttackSpeed += BonusAttackSpeed;
    }
}
```

---

## 2. 에디터에서 노드 구성하기

### 2-1. SkillNodeData 에셋 생성

`Assets > Create > ScriptableObjects/SkillTree/Node`

| 필드 | 설명 | 예시 |
|---|---|---|
| `Skill` | 이 노드가 담는 스킬 에셋 참조 | WhirlSlash 에셋 |
| `Prerequisites` | 이 노드 해금에 필요한 선행 노드 목록 | 기본 베기 노드 |
| `TierIndex` | 티어 번호 (0 = 기본 평타, 1 = 1티어 ...) | `1` |
| `SlotIndex` | 같은 티어 내 분기 그룹 구분 | `0` |
| `AttributeTag` | 법사 속성 구분 (법사 전용) | `"Fire"` |

**검사 예시 구성:**

```
[TierIndex=0, SlotIndex=0] 기본 베기 (Prerequisites: 없음)
[TierIndex=0, SlotIndex=0] 기본 찌르기 (Prerequisites: 없음)
    ↓ (둘 중 하나만 선택 가능)
[TierIndex=1, SlotIndex=1] 회선 베기 (Prerequisites: [기본 베기])
[TierIndex=1, SlotIndex=2] 머리치기 (Prerequisites: [기본 베기])
[TierIndex=1, SlotIndex=3] 관통 (Prerequisites: [기본 찌르기])
[TierIndex=1, SlotIndex=4] 연속 찌르기 (Prerequisites: [기본 찌르기])
```

> **검사 분기 규칙**: 같은 `TierIndex + SlotIndex` 조합을 가진 노드끼리는 하나만 선택 가능.
> 평타 두 개(`SlotIndex=0`)는 서로 배타적이다.

### 2-2. SkillTreeData 에셋 생성

`Assets > Create > ScriptableObjects/SkillTree/Tree`

| 필드 | 설명 |
|---|---|
| `JobClassName` | `"Warrior"` / `"Archer"` / `"Mage"` |
| `StrategyType` | `Branching` / `RowSelect` / `Linear` |
| `AllNodes` | 위에서 만든 모든 SkillNodeData 에셋 등록 |
| `MaxActiveSkillSlots` | 장착 가능한 액티브 스킬 수 (기본 3) |

---

## 3. 플레이어에 컴포넌트 연결하기

1. 씬의 Warrior 오브젝트 선택
2. `Add Component > SkillTreeComponent` 추가
3. 인스펙터에서:
   - `Tree Data` → 위에서 만든 `SkillTreeData` 에셋 연결
   - `Sp Resource` → 프로젝트의 `SO_SP` 에셋 연결

---

## 4. 런타임 사용법

### 4-1. 노드 해금

```csharp
SkillTreeComponent skillTree = player.GetComponent<SkillTreeComponent>();

// 해금 가능 여부 확인 (SP 조건 + 전략 조건 통합)
if (skillTree.CanUnlock(whirlSlashNode))
{
    skillTree.TryUnlockNode(whirlSlashNode);
    // → SP 자동 차감, 패시브라면 Entity 스탯 자동 재계산
}
```

### 4-2. 액티브 스킬 장착

```csharp
// 슬롯 0에 장착 (해금된 스킬만 장착 가능)
skillTree.TryEquipActiveSkill(whirlSlashData, slotIndex: 0);

// 슬롯 해제
skillTree.UnequipActiveSkill(slotIndex: 0);

// 현재 장착된 스킬 조회
ActiveSkillData equipped = skillTree.GetEquippedSkill(0);
equipped?.UseSkill();
```

### 4-3. 전투 루프에서 스킬 발동

```csharp
// 장착된 모든 액티브 스킬 순회 (쿨타임 관리는 별도 구현 필요)
foreach (var skill in skillTree.GetEquippedActives())
{
    if (skill != null)
        skill.UseSkill();
}
```

---

## 5. 직업별 트리 설정 요약

### 검사 — `StrategyType: Branching`

- 같은 `TierIndex + SlotIndex` 그룹에서 하나만 선택
- 선행 노드 중 **하나라도** 해금되면 다음 노드 진행 가능
- 패시브 트리도 동일한 Branching 전략 사용 (방어 라인 / 공격 라인 분기)

### 궁수 — `StrategyType: RowSelect`

- 각 `TierIndex`(행)에서 단 하나만 선택
- 이전 행(`TierIndex - 1`)에서 선택이 완료되어야 다음 행 선택 가능
- `TierIndex: 0` → 1행, `TierIndex: 1` → 2행, `TierIndex: 2` → 3행

### 법사 — `StrategyType: Linear`

- 첫 번째 노드(`TierIndex == 0`) 해금 시 `AttributeTag`(속성)가 자동 확정
- 이후 확정된 속성의 `AttributeTag`를 가진 노드만 해금 가능
- 모든 선행 노드가 해금되어야 다음 노드 진행 가능 (선형)

---

## 6. 신규 직업 추가 방법

1. `Player`를 상속하는 클래스 생성 (예: `Archer`)
2. 해당 직업의 스킬 클래스들 작성 (`ActiveSkillData` / `PassiveSkillData` 상속)
3. 에디터에서 `SkillNodeData` 에셋 구성
4. `SkillTreeData` 에셋 생성 후 `StrategyType` 선택
5. 플레이어 오브젝트에 `SkillTreeComponent` 추가 후 에셋 연결

---

## 7. 클래스 의존 관계

```
[SO_SP]──────────────────────────────────┐
                                         │
[EntityStatData]──►[Entity]◄─────────────┤
                      ▲                  │
         RequireComponent                │
[SkillTreeData]──►[SkillTreeComponent]───┘
      │                    │
      │ CreateStrategy()   │ TryUnlockNode()
      ▼                    ▼
[ISkillTreeStrategy]  [SkillTreeValidator]
   ▲      ▲      ▲
   │      │      │
Branch  Row   Linear
Strategy Strategy Strategy

[SkillNodeData]──►[SkillData]
                     ▲    ▲
              [Active] [Passive]
                            │ ApplyPassive()
                            ▼
                    [EntityStatModifier]
                            │
                            ▼
                        [Entity]
                   ApplyStatModifier()
```
