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

## 2026-09-18 - 기본 총기 탄약

- 내용: 기본 총기에 탄약 수량과 발사 시 소모 처리를 추가했다.
- 기능:
  - 최대 탄약/시작 탄약 Inspector 설정
  - 발사 성공 시 탄약 1개 소모
  - 탄약이 0이면 발사하지 않고 로그 출력
  - 탄약 회복 API와 `AmmoChanged` 이벤트 추가
  - 테스트 HUD에 탄약 수치 표시
- 변경 파일:
  - `Assets/Scripts/Player/PlayerWeapon.cs`
  - `Assets/Scripts/UI/PlayerStatusDebugCanvas.cs`
  - `Assets/Scenes/Gameplay/TestArena.unity`
  - `DEVELOPMENT_LOG.md`
- 검증: `dotnet build Assembly-CSharp.csproj` 성공. Unity MCP에서 `Ammo 30->29`, 투사체 생성, HUD `AMMO: 29/30`, Console Error 0건을 확인했다.
- 제한: 재장전, 무기별 탄약, 탄약 아이템은 아직 구현하지 않았다.

## 2026-09-18 - Enemy HP 및 총알 피해

- 내용: BasicEnemy에 EnemyHealth를 추가하고 PlayerProjectile이 적중 시 적 HP를 감소시키도록 연결했다.
- 기능:
  - Enemy HP 최대치 5 및 피해 처리
  - 투사체 피해량 Inspector 설정
  - 투사체 적중 시 EnemyHealth 피해 적용
  - Player와 Enemy가 같은 `Actors` 부모를 공유하는 구조에 맞춘 소유자 충돌 판정
- 변경 파일:
  - `Assets/Scripts/Enemies/EnemyHealth.cs`
  - `Assets/Scripts/Enemies/EnemyHealth.cs.meta`
  - `Assets/Scripts/Combat/PlayerProjectile.cs`
  - `Assets/Scripts/Player/PlayerWeapon.cs`
  - `Assets/Scenes/Gameplay/TestArena.unity`
  - `DEVELOPMENT_LOG.md`
- 검증: `dotnet build Assembly-CSharp.csproj` 성공. Unity MCP에서 투사체 적중 후 `Enemy HP 5->4`, `Ammo=29`, Console Error 0건을 확인했다.
- 제한: HP 0 이후 사망 상태 연출/비활성화/드롭 보상은 아직 구현하지 않았다.

## 2026-09-18 - Enemy Death

- 내용: Enemy HP가 0이 되면 적의 행동과 충돌을 중지하고 시각 오브젝트는 남기는 사망 상태를 추가했다.
- 기능:
  - EnemyDeath 컴포넌트 분리
  - EnemyAttack, BasicEnemyController, EnemyStun 비활성화
  - Enemy Collider 비활성화 및 Rigidbody2D 시뮬레이션 중지
  - 사망 상태 중복 실행 방지
- 변경 파일:
  - `Assets/Scripts/Enemies/EnemyDeath.cs`
  - `Assets/Scripts/Enemies/EnemyDeath.cs.meta`
  - `Assets/Scenes/Gameplay/TestArena.unity`
  - `DEVELOPMENT_LOG.md`
- 검증: `dotnet build Assembly-CSharp.csproj` 성공. Unity MCP에서 HP 0 후 `Dead=True`, 공격/컨트롤러/Collider 비활성화, Rigidbody 시뮬레이션 중지, Console Error 0건을 확인했다.
- 제한: 사망 애니메이션, 시체 연출, 드롭 보상은 아직 구현하지 않았다.

## 2026-09-18 - Enemy 기절 조건 정리

- 내용: Enemy HP가 최대 HP의 30% 이하로 내려가면 자동으로 기절하도록 기존 EnemyStun을 확장했다.
- 기능:
  - HP 임계값 비율 Inspector 설정
  - 임계값 진입 시 공격 1회 자동 기절
  - Parry가 직접 호출하는 기존 Stun 흐름 유지
  - 사망 상태에서는 임계값 기절 방지
- 변경 파일:
  - `Assets/Scripts/Enemies/EnemyStun.cs`
  - `Assets/Prefabs/Enemies/BasicEnemy.prefab`
  - `DEVELOPMENT_LOG.md`
- 검증: `dotnet build Assembly-CSharp.csproj` 성공. Unity MCP에서 HP 5에 4 피해 후 `HP=1`, `IsStunned=True`, `IsDead=False`, Console Error 0건을 확인했다.
- 제한: 기절 중 처형, 처형 보상, 적 등급별 기절 횟수는 아직 구현하지 않았다.

## 2026-09-18 - Enemy Stagger/Stun 분리 및 상태 표시

- 내용: 짧은 Stagger와 긴 Stun을 별도 상태로 분리하고 TestArena에서 상태를 확인할 수 있는 표시를 추가했다.
- 기능:
  - `EnemyStagger` 컴포넌트와 짧은 경직 지속 시간
  - 일반 피해 시 `STAGGER`, HP 30% 이하 진입 시 `STUN`
  - Stun 시작 시 진행 중인 Stagger 취소
  - Stagger/Stun 중 EnemyAttack 방지
  - 적 머리 위에 `STAGGER`(노란색), `STUN`(빨간색) 표시
- 변경 파일:
  - `Assets/Scripts/Enemies/EnemyStagger.cs`
  - `Assets/Scripts/Enemies/EnemyStun.cs`
  - `Assets/Scripts/Enemies/EnemyStatusDebugDisplay.cs`
  - `Assets/Scripts/Combat/EnemyAttack.cs`
  - `Assets/Scenes/Gameplay/TestArena.unity`
  - `DEVELOPMENT_LOG.md`
- 검증: `dotnet build Assembly-CSharp.csproj` 성공. MCP에서 일반 피해 후 `Stagger=True, Stun=False`, 임계값 피해 후 `Stagger=False, Stun=True`, 상태 텍스트 `STUN`, Console Error 0건을 확인했다.
- 제한: Stun 상태에서의 Execution과 적 등급별 Stagger 횟수 규칙은 아직 구현하지 않았다.

## 2026-09-18 - Execution

- 내용: E 입력으로 가까운 Stun 상태의 저체력 적을 처형하는 기능을 추가했다.
- 조건:
  - 실행 범위 내 적
  - EnemyStun 활성 상태
  - Enemy HP가 최대 HP의 30% 이하
- 보상:
  - PlayerHealth 회복
  - PlayerWeapon 탄약 회복
- 변경 파일:
  - `Assets/Scripts/Player/PlayerExecution.cs`
  - `Assets/Scripts/Player/PlayerExecution.cs.meta`
  - `Assets/Scenes/Gameplay/TestArena.unity`
  - `DEVELOPMENT_LOG.md`
- 검증: `dotnet build Assembly-CSharp.csproj` 성공. MCP에서 적 HP 1/Stun 상태를 E 실행 경로로 처형해 `HP=0`, `Dead=True`, 탄약 `29->30`, Console Error 0건을 확인했다.
- 제한: 처형 애니메이션, 처형 전용 근접 공격 판정, 보상 연출은 아직 구현하지 않았다.

## 2026-09-18 - Player 기본 총기

- 내용: 기존 `Player/Attack` 좌클릭 입력을 사용해 마우스 방향으로 기본 투사체를 발사하는 기능을 추가했다.
- 기능:
  - 발사 쿨다운과 투사체 속도/수명/크기 Inspector 설정
  - 마우스 커서 방향 조준
  - 런타임 투사체 생성 및 이동
  - 투사체 충돌/수명 종료 처리
  - MainCamera 태그가 없는 TestArena를 위한 카메라 fallback
- 변경 파일:
  - `Assets/Scripts/Player/PlayerWeapon.cs`
  - `Assets/Scripts/Player/PlayerWeapon.cs.meta`
  - `Assets/Scripts/Combat/PlayerProjectile.cs`
  - `Assets/Scripts/Combat/PlayerProjectile.cs.meta`
  - `Assets/Scenes/Gameplay/TestArena.unity`
  - `DEVELOPMENT_LOG.md`
- 검증: `dotnet build Assembly-CSharp.csproj` 성공. Unity MCP에서 발사 메서드를 실행해 `PlayerProjectile` 생성 확인, Console Error 0건.
- 제한: 이번 단계에서는 탄약, 무기 교체, 적 HP/피해 적용을 구현하지 않았다.

## 2026-09-20 - Player 회피 대시 프로토타입

- 내용: 기존 `Player/Sprint` 입력(Left Shift)을 재사용해 Player 회피 대시를 추가했다.
- 기능:
  - 좌우 입력 방향으로 대시
  - 입력 방향이 없으면 마지막 방향 또는 기본 오른쪽 방향 사용
  - 공중에서도 사용 가능
  - 대시 지속 시간과 쿨다운 Inspector 설정
  - 대시 중 Rigidbody2D 수평 속도 고정
- 변경 파일:
  - `Assets/Scripts/Player/PlayerDodge.cs`
  - `Assets/Scripts/Player/PlayerDodge.cs.meta`
  - `Assets/Prefabs/Player/Player.prefab`
  - `DEVELOPMENT_LOG.md`
- 검증: C# 진단 오류 없음. `dotnet build Assembly-CSharp.csproj --no-restore` 성공. Unity MCP에서 Assets Refresh 후 TestArena Play Mode 진입 및 종료를 확인했고 Console Error가 없었다.
- 제한: 이번 단계에서는 회피 무적 판정, 회피 성공 시 슬로우/강화 포인트 보상, 회피 전용 연출을 구현하지 않았다.
- 다음 작업: 회피 무적 판정과 패링/회피 성공 보상 시스템을 별도 기능으로 연결한다.

## 2026-09-20 - Player 회피 거리 및 속도 조정

- 내용: 회피 대시가 더 빠르고 멀리 이동하도록 기본 튜닝값을 조정했다.
- 변경:
  - `dodgeSpeed`: 14 -> 22
  - `dodgeDuration`: 0.16초 -> 0.22초
  - `dodgeCooldown`: 0.75초 -> 0.7초
  - 예상 대시 거리: 약 2.24 -> 4.84 유닛
- 변경 파일:
  - `Assets/Scripts/Player/PlayerDodge.cs`
  - `Assets/Prefabs/Player/Player.prefab`
  - `DEVELOPMENT_LOG.md`
- 검증: C# 진단 오류 없음. `dotnet build Assembly-CSharp.csproj --no-restore` 성공. Unity Assets Refresh 및 Play Mode 진입 후 Console Error 없이 종료를 확인했다.

## 2026-09-21 - Unity MCP 사용자 경로 이식성 수정

- 문제: `.vscode/mcp.json`이 데스크톱 사용자 `ytnor`의 절대 경로를 사용해 노트북 사용자 `admin` 환경에서 동일한 설정을 재사용할 수 없었다.
- 수정:
  - Unity MCP 실행 경로를 `${userHome}\\AppData\\Local\\Unity\\bin\\unity.exe`로 변경
  - 컴퓨터별 Windows 사용자명을 설정 파일에 직접 기록하지 않도록 수정
- 변경 파일:
  - `.vscode/mcp.json`
  - `DEVELOPMENT_LOG.md`
- 검증: JSON 파싱 성공, `${userHome}`이 현재 `C:\\Users\\admin`으로 해석됨, 실제 Unity MCP 실행 파일 존재 확인.
- 제한: 데스크톱에서도 Unity MCP 실행 파일이 사용자 홈의 동일한 상대 경로에 설치되어 있어야 한다.
- 다음 작업: 데스크톱 복귀 후 VS Code에서 `Developer: Reload Window` 또는 `MCP: Restart Server`를 한 번 실행하고 MCP 서버 상태를 확인한다.
- 제한: 실제 키 입력에 따른 이동 거리 체감은 Editor에서 직접 플레이하며 추가 조정할 수 있다.

## 2026-09-20 - Player 회피 방향 입력 개선

- 내용: 회피 방향을 이동 키와 마우스 커서 기준으로 선택하도록 수정했다.
- 동작:
  - 이동 키를 누르고 있으면 해당 이동 벡터 방향으로 대시
  - 이동 키 입력이 없으면 Player에서 마우스 커서가 있는 월드 방향으로 대시
  - 카메라 또는 커서 방향을 계산할 수 없으면 기본 방향으로 대시
- 변경 파일:
  - `Assets/Scripts/Player/PlayerDodge.cs`
  - `DEVELOPMENT_LOG.md`
- 검증: C# 진단 오류 없음. `dotnet build Assembly-CSharp.csproj --no-restore` 성공. Unity Assets Refresh 후 Play Mode 진입, 컴파일 완료 및 Console Error 없음 확인.
- 제한: 실제 키보드/마우스 입력에 따른 대시 방향 체감은 Editor에서 직접 플레이하며 추가 확인이 필요하다.

## 2026-09-20 - Player 회피 수평 이동 제한

- 내용: 마우스 커서 방향을 사용하더라도 회피는 수평으로만 이동하도록 수정했다.
- 동작:
  - 좌우 이동 키 입력이 있으면 입력 방향으로 수평 대시
  - 좌우 입력이 없으면 커서의 Player 기준 좌우 위치로 수평 대시
  - 마우스의 높이는 대시 방향에 영향을 주지 않음
- 변경 파일:
  - `Assets/Scripts/Player/PlayerDodge.cs`
  - `DEVELOPMENT_LOG.md`
- 검증: C# 진단 오류 없음. `dotnet build Assembly-CSharp.csproj --no-restore` 성공. Unity Play Mode 진입 후 Console Error 없이 종료를 확인했다.

## 2026-09-20 - 패링/회피 성공 피드백 표시

- 내용: 패링과 회피가 실제로 적 공격을 무효화했을 때 화면에 성공 메시지를 표시하도록 연결했다.
- 기능:
  - 패링 성공 시 청록색 `PARRY SUCCESS!` 표시
  - 회피 중 적 공격을 피하면 노란색 `DODGE SUCCESS!` 표시
  - 성공 메시지는 기존 테스트 HUD 상단에 0.8초 표시
  - 한 번의 대시 동안 회피 성공은 1회만 등록
  - 회피 성공 시 적 공격의 피해 적용을 중단
- 변경 파일:
  - `Assets/Scripts/Player/PlayerParry.cs`
  - `Assets/Scripts/Player/PlayerDodge.cs`
  - `Assets/Scripts/Combat/EnemyAttack.cs`
  - `Assets/Scripts/UI/PlayerStatusDebugCanvas.cs`
  - `DEVELOPMENT_LOG.md`
- 검증: C# 진단 오류 없음. `dotnet build Assembly-CSharp.csproj --no-restore` 성공. Unity Assets Refresh 후 Play Mode 진입 및 종료, Console Error 없음 확인.
- 제한: 실제 공격 타이밍에 패링/회피 성공 메시지가 화면에 뜨는 장면 확인은 Editor에서 직접 전투를 재현해 추가 확인해야 한다.

## 2026-09-20 - 패링/회피 판정 여유 조정

- 내용: 패링과 회피의 활성 판정 시간을 늘리고, 공격 히트박스가 이미 활성화된 뒤에도 회피 성공을 확인하도록 보완했다.
- 변경:
  - PlayerParry 판정 시간: `0.2초 -> 0.32초`
  - PlayerDodge 판정 시간: `0.22초 -> 0.32초`
  - EnemyAttack가 활성 공격 중 PlayerDodge 상태를 매 프레임 확인
  - 회피 중 공격을 피하면 공격을 취소하고 성공 처리
- 변경 파일:
  - `Assets/Scripts/Player/PlayerParry.cs`
  - `Assets/Scripts/Player/PlayerDodge.cs`
  - `Assets/Scripts/Combat/EnemyAttack.cs`
  - `Assets/Prefabs/Player/Player.prefab`
  - `Assets/Scenes/Gameplay/TestArena.unity`
  - `DEVELOPMENT_LOG.md`
- 검증: C# 진단 오류 없음. `dotnet build Assembly-CSharp.csproj --no-restore` 성공. Unity Refresh 후 MCP Play Mode 명령은 응답 지연으로 완료 확인이 되지 않았고, Unity 프로세스와 로그에서는 최근 컴파일/치명 오류가 발견되지 않았다.
- 제한: 실제 전투에서 판정 체감은 Unity Editor에서 직접 패링/회피를 반복해 최종 조정해야 한다.

## 2026-09-20 - 대쉬 이동 속도 덮어쓰기 수정

- 문제: 회피 판정은 성공하지만 `PlayerController.FixedUpdate()`가 일반 이동 속도로 Rigidbody2D의 대쉬 속도를 덮어써 대쉬 이동이 보이지 않았다.
- 수정:
  - PlayerController가 PlayerDodge를 참조
  - 대쉬 활성 중에는 일반 이동 및 점프 속도 적용을 건너뜀
  - PlayerDodge가 설정한 수평 대쉬 속도를 유지
- 변경 파일:
  - `Assets/Scripts/Player/PlayerController.cs`
  - `DEVELOPMENT_LOG.md`
- 검증: C# 진단 오류 없음. `dotnet build Assembly-CSharp.csproj --no-restore` 성공. Unity Play Mode 진입, 컴파일 완료, Console Error 없음 확인.

## 2026-09-20 - 대쉬 속도 및 최종 거리 재조정

- 내용: 대쉬가 더 빠르게 느껴지면서 최종 이동 거리는 줄어들도록 수치를 조정했다.
- 변경:
  - `dodgeSpeed`: 22 -> 28
  - `dodgeDuration`: 0.32초 -> 0.18초
  - 예상 이동 거리: 약 7.04 -> 5.04 유닛
- 변경 파일:
  - `Assets/Scripts/Player/PlayerDodge.cs`
  - `Assets/Prefabs/Player/Player.prefab`
  - `DEVELOPMENT_LOG.md`
- 검증: C# 진단 오류 없음. `dotnet build Assembly-CSharp.csproj --no-restore` 성공. Unity Assets Refresh 및 Play Mode 진입 후 Console Error 없이 종료를 확인했다.
