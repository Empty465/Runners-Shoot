# Development Log

이 파일은 Unity 프로젝트의 기능 추가 및 수정 내역을 기록한다.
각 기능은 구현 범위, 변경 파일, 검증 결과, 다음 작업을 남긴다.

## 2026-09-17 - 프로젝트 초기 구조

- 내용: 2D 플랫포머 패링 액션 게임 개발을 위한 폴더 구조와 TestArena Scene을 구성했다.
- 주요 구성:
  - `Assets/Scenes/Gameplay/TestArena.unity`
  - `Assets/Scripts/Player/`
  - `Assets/Scripts/Combat/`
  - `Assets/Scripts/Enemies/`
  - `Assets/Prefabs/Player/`
  - `Assets/Prefabs/Enemies/`
  - 기타 Art, Audio, Materials, Tilemaps, UI 폴더
- 보호 대상: 기존 `SampleScene`은 수정하지 않았다.

## 2026-09-17 - Player Movement 프로토타입

- 내용: Input System과 Rigidbody2D를 사용하는 Player 이동 프로토타입을 구현했다.
- 기능:
  - 좌우 이동
  - 점프
  - Ground Check
  - 공중 무한 점프 방지
  - Inspector 조정 가능한 이동 속도, 점프 힘, Ground Check 반경
- 변경 파일:
  - `Assets/Scripts/Player/PlayerController.cs`
  - `Assets/Prefabs/Player/Player.prefab`
  - `Assets/Scenes/Gameplay/TestArena.unity`
- 검증: 기존 Input Actions의 `Player/Move`, `Player/Jump`를 재사용했다.

## 2026-09-17 - TestArena 디버그 환경

- 내용: Player 이동과 점프를 확인하기 위한 임시 디버그 비주얼과 플랫폼을 TestArena에 추가했다.
- 구성:
  - Player 디버그 비주얼
  - 긴 Ground
  - 서로 다른 높이의 플랫폼
  - 디버그 카메라 및 조명
- 변경 파일:
  - `Assets/Scenes/Gameplay/TestArena.unity`
- 보호 대상: Player 이동 로직, Input Actions, SampleScene은 변경하지 않았다.
- 검증: Play Mode에서 Player, Ground, 플랫폼 렌더링을 확인했고 Unity Console 오류와 경고가 없었다.

## 2026-09-17 - Basic Enemy Attack

- 내용: 이후 패링 테스트에 사용할 기본 Enemy 감지 및 공격 시스템을 구현했다.
- 기능:
  - Player 감지 및 방향 전환
  - Attack Range
  - Attack Windup
  - Attack Active Time
  - Attack Cooldown
  - 독립적인 Attack Hitbox
  - Windup 및 Hitbox 디버그 표시
- 변경 파일:
  - `Assets/Scripts/Enemies/BasicEnemyController.cs`
  - `Assets/Scripts/Combat/EnemyAttack.cs`
  - `Assets/Prefabs/Enemies/BasicEnemy.prefab`
  - `Assets/Scenes/Gameplay/TestArena.unity`
- 검증: Enemy 감지, 방향 전환, Windup 표시, AttackHitbox Trigger 구성을 확인했다.
- 제한: MCP Play Mode 시간 제약으로 `WaitForSeconds` 기반 Active Time 종료와 Cooldown 경과는 Editor에서 추가 확인이 필요하다.

## 2026-09-17 - 다음 작업 예정: Player Parry + Enemy Stun

- 목표: Enemy Attack Hitbox가 활성화된 짧은 시간에 Player가 우클릭 Parry를 성공하면 현재 공격을 취소하고 Enemy를 Stunned 상태로 만든다.
- 예정 파일:
  - `Assets/Scripts/Player/PlayerParry.cs`
  - `Assets/Scripts/Enemies/EnemyStun.cs`
  - `Assets/Scripts/Combat/EnemyAttack.cs`
  - 필요 시 `Assets/Scenes/Gameplay/TestArena.unity`
- 범위 제외:
  - HP
  - Damage 처리
  - Death
  - Execution
  - Knockback
  - Enemy AI 이동
- 성공 조건:
  - ParryHitbox와 AttackHitbox가 활성 상태에서 실제로 Trigger 충돌
  - 공격 1회당 Parry 성공 최대 1회
  - 성공 즉시 현재 공격 및 AttackHitbox 종료
  - Stun Duration 동안 새 공격 방지
  - Stun 종료 후 공격 재개

## 기록 규칙

새 기능 또는 수정 작업을 완료할 때마다 아래 항목을 추가한다.

- 날짜와 기능명
- 구현 또는 수정 내용
- 변경 파일 목록
- 검증 결과
- 확인하지 못한 제한 사항
- 다음 작업 또는 후속 확인 사항

기존 항목은 삭제하거나 임의로 다시 작성하지 않고, 최신 작업을 파일 마지막에 추가한다.

## 새 세션용 작업 가이드

세션 기록이 동기화되지 않은 환경에서도 이 문서만 읽고 작업을 이어갈 수 있도록 다음 규칙을 적용한다.

### 작업 시작 전

- 이 파일의 최신 항목과 `다음 작업 예정`을 먼저 확인한다.
- 현재 Unity Scene, 관련 스크립트, Prefab의 실제 상태를 먼저 확인한다.
- 기존 시스템과 중복되는 기능이 있는지 확인한 뒤 가장 가까운 구현 지점을 선택한다.
- 작업 범위와 수정 예정 파일을 먼저 분명히 한다.

### 구현 원칙

- 한 번에 하나의 기능만 구현한다.
- 요청하지 않은 파일은 수정하지 않는다.
- 기존 Player Movement와 Input System 구조를 유지한다.
- `SampleScene`은 요청이 없는 한 수정하지 않는다.
- Package, Project Settings, Input Actions는 명시적인 요청과 필요한 근거가 있을 때만 변경한다.
- 전투 기능은 `PlayerController.cs`에 몰아넣지 않고 기능별 스크립트로 분리한다.
- 기존 동작을 전면 수정하기보다 필요한 최소 연결만 추가한다.
- HP, Damage, Death, Execution 등 아직 범위에 포함되지 않은 기능을 임의로 구현하지 않는다.

### 검증 원칙

- 수정 후 Unity Console의 Error와 C# 진단 오류를 확인한다.
- 가능하면 TestArena를 Play Mode로 실행해 변경된 기능을 직접 확인한다.
- 시간 기반 동작은 시작, 진행 중, 종료 상태를 각각 확인한다.
- 자동 검증이 제한되면 확인하지 못한 항목과 사용자가 Editor에서 확인할 항목을 기록한다.
- 검증 전에는 작업 완료로 표시하지 않는다.

### 작업 완료 후

- 이 파일 마지막에 새 날짜별 항목을 추가한다.
- 구현 내용, 변경 파일, 검증 결과, 제한 사항, 다음 작업을 기록한다.
- 다음 세션에서 바로 사용할 수 있도록 현재 작업의 시작점을 한두 문장으로 남긴다.
- 이전 기록은 삭제하거나 다시 작성하지 않는다.

## 현재 세션 없이 이어가기 위한 시작점

현재 구현의 다음 작업은 `Player Parry + Enemy Stun`이다. 먼저 `EnemyAttack.cs`, `BasicEnemy.prefab`, Player Prefab과 기존 Input Actions를 확인한 뒤 구현을 시작한다. 목표 범위는 Parry 성공, 현재 공격 취소, Enemy Stun, Stun 종료 후 공격 재개까지이며 HP, Damage, Death, Execution은 포함하지 않는다.

## 2026-09-18 - Player HP + Enemy Damage

- 내용: Player에 독립적인 HP 컴포넌트를 추가하고 BasicEnemy의 AttackHitbox가 Player에게 피해를 주도록 연결했다.
- 기능:
  - Player HP 최대치 5 및 현재 HP 초기화
  - EnemyAttack의 attackDamage를 PlayerHealth에 전달
  - HP 0 도달 시 사망 상태 및 로그 처리
  - 패링 중에는 EnemyAttack의 피해 적용 방지
- 변경 파일:
  - `Assets/Scripts/Player/PlayerHealth.cs`
  - `Assets/Scripts/Player/PlayerHealth.cs.meta`
  - `Assets/Scripts/Combat/EnemyAttack.cs`
  - `Assets/Prefabs/Player/Player.prefab`
  - `DEVELOPMENT_LOG.md`
- 검증: PlayerHealth C# 진단 오류 없음. Unity Editor Play Mode에서 AttackHitbox의 HP 감소와 패링 중 피해 방지는 추가 확인이 필요하다.
- 제한: 이번 작업에서는 Armor, Death 연출/입력 잠금, HUD를 구현하지 않았다.
- 다음 작업: Unity Console 오류를 확인한 뒤 Player HP 피해 동작을 Play Mode에서 검증한다.

## 2026-09-18 - 임시 Player HP 표시

- 내용: TestArena에서 Player HP를 확인할 수 있도록 화면 좌측 상단에 임시 텍스트를 추가했다.
- 변경 파일:
  - `Assets/Scripts/UI/PlayerHealthDebugDisplay.cs`
  - `Assets/Scripts/UI/PlayerHealthDebugDisplay.cs.meta`
  - `Assets/Scenes/Gameplay/TestArena.unity`
  - `DEVELOPMENT_LOG.md`
- 검증: UI 스크립트 C# 진단 오류 없음. `dotnet build Assembly-CSharp.csproj` 성공. Play Mode에서 화면 표시 위치와 HP 감소에 따른 텍스트 갱신은 Editor 확인이 필요하다.
- 제한: 정식 Canvas/HUD, HP 게이지, Armor 표시는 아직 구현하지 않았다.

## 2026-09-18 - 임시 HP HUD MCP 검증 및 수정

- 문제: 수동으로 추가한 Unity YAML FileID가 signed 64-bit 범위를 초과했고, `GUI.skin`을 `Awake`에서 호출해 HUD가 표시되지 않았다.
- 수정:
  - Player Prefab과 TestArena UI의 FileID를 유효한 값으로 교체
  - PlayerHealth와 PlayerHealthDebugDisplay를 Unity MCP로 Scene 오브젝트에 직접 연결하고 Scene 저장
  - GUI 스타일 생성을 `OnGUI` 내부로 이동
- 검증: Unity MCP로 TestArena Play Mode 실행, Console Error 0건, Game 화면에서 `HP: 5/5` 표시 확인.

## 2026-09-18 - Player Armor

- 내용: Player 피해 처리에 Armor를 추가했다. Armor가 남아 있으면 HP 대신 Armor가 피해량만큼 감소한다.
- 기능:
  - 최대 Armor 및 시작 Armor 설정
  - Armor 우선 피해 처리
  - Armor 회복 API와 `ArmorChanged` 이벤트 추가
  - 임시 HUD에 HP와 Armor 동시 표시
- 변경 파일:
  - `Assets/Scripts/Player/PlayerHealth.cs`
  - `Assets/Scripts/UI/PlayerHealthDebugDisplay.cs`
  - `Assets/Prefabs/Player/Player.prefab`
  - `DEVELOPMENT_LOG.md`
- 검증: `dotnet build Assembly-CSharp.csproj` 성공. Unity MCP 런타임 평가에서 첫 피해 후 `HP 5->5, Armor 5->4`, Armor 소진 후 다음 피해에서 `HP 5->4`를 확인했다.
- 제한: 기존 임시 HUD는 현재 Game 화면 캡처에서 HP만 표시되어 Armor 텍스트 반영은 추가 확인이 필요하다.
- 제한: Armor 아이템, 정식 HUD, Armor 파괴 연출은 아직 구현하지 않았다.

## 2026-09-18 - HP/Armor 임시 HUD 갱신 수정

- 문제: 기존 IMGUI 임시 표시가 HP와 Armor 상태 변경을 화면에 안정적으로 반영하지 않았다.
- 수정:
  - `PlayerStatusDebugCanvas`를 추가해 런타임 Canvas/Text로 상태를 표시
  - `HealthChanged`와 `ArmorChanged` 이벤트를 구독해 피해 즉시 텍스트 갱신
  - HP와 Armor를 한 줄의 테스트 표시로 통합
- 변경 파일:
  - `Assets/Scripts/UI/PlayerStatusDebugCanvas.cs`
  - `Assets/Scripts/UI/PlayerStatusDebugCanvas.cs.meta`
  - `Assets/Scenes/Gameplay/TestArena.unity`
  - `DEVELOPMENT_LOG.md`
- 검증: Unity MCP 런타임에서 피해 적용 후 실제 Text 값이 `HP: 5/5 | ARMOR: 4/5`로 갱신되는 것을 확인했고 Console Error는 0건이다.
- 제한: 현재 HUD는 테스트용이며 정식 HP/Armor 게이지는 아직 구현하지 않았다.

## 2026-09-18 - 임시 HUD 중복 표시 제거

- 문제: 기존 IMGUI HP/Armor 표시와 Canvas 기반 HUD가 동시에 활성화되어 텍스트가 겹쳤다.
- 수정: TestArena `/UI` 오브젝트에서 `PlayerHealthDebugDisplay`와 `PlayerArmorDebugDisplay`를 제거하고 `PlayerStatusDebugCanvas`만 유지했다.
- 검증: Unity MCP Game 화면에서 `HP: 5/5 | ARMOR: 0/5` 한 줄만 표시되고 Console Error 0건을 확인했다.

## 2026-09-18 - HUD 초기값 및 갱신 재수정

- 문제: HUD Text 생성 직후 초기 문자열을 설정하지 않아 첫 프레임에 이전 HP/Armor 값이 보일 수 있었다.
- 수정: Canvas 생성 직후 `UpdateStatusText()`를 호출해 현재 PlayerHealth 값을 즉시 반영했다.
- 검증: Unity MCP에서 초기 Text `HP: 5/5 | ARMOR: 5/5`, 피해 적용 후 `HP: 5/5 | ARMOR: 4/5`를 확인했다. Console Error 0건.
