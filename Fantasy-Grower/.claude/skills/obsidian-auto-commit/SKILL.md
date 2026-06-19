---
name: obsidian-auto-commit
description: 옵시디언 레포의 변경사항(추가/수정/삭제)을 자동으로 감지하고 커밋 + GitHub push하는 스킬. 사용자가 "옵시디언 커밋", "노트 변경사항 저장", "obsidian push", "옵시디언 동기화", "변경된 노트 커밋" 등을 언급하면 반드시 이 스킬을 사용한다. 옵시디언 볼트 경로가 Git 레포인 상황에서 변경사항을 GitHub에 올리고 싶을 때 항상 트리거된다.
---

# Obsidian Auto-Commit Skill

옵시디언 레포의 모든 변경사항(추가/수정/삭제)을 자동으로 커밋하고 GitHub에 push하는 스킬.

## 워크플로우

### 1. 레포 경로 파악

사용자가 경로를 명시하지 않은 경우:
```bash
# 일반적인 옵시디언 볼트 위치 탐색
find ~ -name ".obsidian" -type d 2>/dev/null | head -5
```

경로가 확인되면 해당 디렉토리로 이동.

### 2. 변경사항 확인

```bash
cd <VAULT_PATH>
git status --short
```

변경사항이 없으면: "변경된 파일이 없습니다." 출력 후 종료.

### 3. 커밋 메시지 자동 생성

변경된 파일 목록을 기반으로 커밋 메시지를 구성:

```bash
cd <VAULT_PATH>
git status --short
```

출력 결과를 파싱하여 아래 규칙으로 메시지 생성:
- 추가된 파일 (`?? ` 또는 `A `): `Add: <파일명>`
- 수정된 파일 (`M `): `Update: <파일명>`
- 삭제된 파일 (`D `): `Delete: <파일명>`
- 여러 파일이면: 대표 파일명 + ` 외 N개`

**예시:**
- `Add: 프로젝트 계획.md`
- `Update: 일간노트/2026-04-17.md 외 2개`
- `Add: 새노트.md, Delete: 구노트.md`

### 4. 스테이징 + 커밋 + Push

```bash
cd <VAULT_PATH>
git add -A
git commit -m "<자동생성된 커밋 메시지>"
git push
```

### 5. 결과 출력

성공 시:
```
✅ 커밋 완료: <커밋 메시지>
📤 GitHub push 완료
변경된 파일: N개
```

실패 시 에러 메시지를 그대로 출력하고 원인 설명.

## 주의사항

- `git push` 실패 시 (인증 문제 등) 커밋은 유지하고 push 실패 사유를 안내
- `.obsidian/` 폴더 변경사항도 포함됨 (플러그인 설정 등)
- 충돌(conflict) 발생 시 자동 해결하지 않고 사용자에게 알림
- 레포에 remote origin이 설정되어 있어야 push 가능

## 사용자가 경로를 지정하는 경우

"~/Documents/MyVault 커밋해줘" 처럼 경로를 명시하면 해당 경로를 바로 사용.