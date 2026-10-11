# Development Log

이 파일은 Unity 프로젝트의 기능 추가 및 수정 내역을 기록한다.
각 기능은 구현 범위, 변경 파일, 검증 결과, 다음 작업을 남긴다.

## 2026-10-11 - 체력/피해 수치 100 기준 스케일링

- 내용: 모든 체력·피해 수치를 기존 대비 ×20으로 바꿔 플레이어 최대 체력을 100으로 맞췄다(기존 5).
- 변경: 플레이어 체력 100 / 방어구 100(시작 100); 적 체력 Slag 60, Pawn 80, Knight 100, Archer 90, Baron 240; 적 피해 Slag 20, Pawn 15, Knight 25, Archer 20, Baron 32; 무기 SMG 20, 샷건 펠릿 11, 레일건 80, 강화 +10(레일건 +10), 화상 틱 5, 화상 방어구 보상 2; 근접 일반 30 / 강화 80; 처형 체력 보상 40. 코드 기본값, `Player.prefab`, `BasicEnemy.prefab`, TestArena 씬의 Player 인스턴스 값을 모두 갱신. 표시 형식(`:0`)은 그대로 정수로 보인다.
- 검증(Unity MCP, Play Mode): 적 체력/공격 피해(Archer 90/20, Knight 100/25), 플레이어 100/100·방어구 100/100, 25 피해 시 방어구 75, 적에 20 피해 시 100→80 확인.
- 미검증: 전체 전투 밸런스(상대 비율은 유지됨), 투사체 패링 피해, 화상/업그레이드 실전 동작. README의 옛 수치 서술은 갱신하지 않음.

## 2026-10-11 - 저체력 기절 반복 발동

- 내용: 체력이 30% 이하인 적은 기절(처형 가능, 이동/공격 정지)에 들어가고, 3초(`EnemyStun.lowHealthStunDuration`) 뒤 풀린다. 풀린 뒤 다시 피해를 받으면 조건(체력 ≤30%)이 맞아 다시 기절한다. 기존에는 1회만 발동했다. 기절 중 피해는 기절 시간을 연장하지 않는다. 스킬/강화 근접 스턴은 기존 1.5초(`stunDuration`) 유지.
- 검증(Unity MCP, Play Mode): 체력 1.4로 만들자 기절 true, 기절 중 추가 피해에도 유지, 약 2초 시점 기절 중, 4.7초 시점 해제, 이후 피해 1회로 즉시 재기절 확인. 정확한 3.0초 경계는 측정하지 않음.

## 2026-10-11 - 플레이어 근접 공격 / 처형 / 강화 근접

- 내용: `PlayerMelee` 신규(Player 프리팹에 추가). E 키를 `Keyboard.current.eKey`로 직접 읽는다. 탭=처형 가능하면 처형, 아니면 일반 근접(포인트 25 소모, 피해 1.5, 사거리 1.8, 쿨 0.4s, 탄약 +2). 홀드 0.4초+포인트 100=강화 근접(피해 4, 사거리 2.6, 반경 5 광역 스턴, 포인트 전부 소모, 쿨 0.8s). 포인트: 초당 +4, 패링 성공 +30, 회피 성공 +20. 방향은 마우스 x 기준. 짧은 슬래시 VFX.
- 변경: `PlayerExecution`은 입력 처리를 제거하고 `public bool TryExecute()` 헬퍼로 변경(기절 + 체력 ≤30% → 즉사, 체력 +2, 탄약 +5). HUD COMBAT 패널에 `MELEE (E)` 행(포인트/FULL) 추가. `Player.prefab`에 `PlayerMelee` 추가.
- 검증(Unity MCP, Play Mode, 메서드 직접 호출): 컴파일 성공. 일반 근접 명중 시 적 체력 5→3.5, 포인트 50→25. 포인트 10일 때는 발동 안 됨. 기절+체력 1.4인 적에 포인트 0으로 탭 시 처형(사망), SMG 탄약 10→15. 강화 근접: Archer 체력 4.5→0.5, 기절 적용, 포인트 100→0.
- 미검증: 실제 E 키 입력(탭/홀드 구분), 일반 근접의 탄약 +2, 처형 체력 +2(플레이어 체력이 가득 차 있어 확인 못 함), 패링/회피 포인트 충전, 다수 적 광역 스턴, HUD 표시 모양, 일반 근접으로 인한 스턴 후 처형 연계.
- 참고: 씬을 저장하지 않았다(Player 프리팹에 추가했으므로 인스턴스는 상속). 커밋 전 TestArena 저장 여부 확인 필요.

## 2026-10-11 - 적 종류별 프리팹 (직접 배치)

- 내용: `Assets/Prefabs/Enemies/Enemy_Slag|Pawn|Knight|Archer|Baron.prefab`(BasicEnemy 변형)을 추가했다. 씬에 끌어다 놓으면 해당 종류로 동작한다. 감지 범위는 인스펙터의 `detectionRange`(프리팹 기본 Knight/Baron 8, 나머지 10)로 조절한다.
- 변경: `BasicEnemyController`의 TestArena 자동 스폰/감지범위 4.5 강제는 `Enemies` 아래 적이 1개(Knight)일 때만 적용된다. 직접 배치해 2개 이상이면 자동 스폰이 꺼지고 인스펙터 값이 유지된다.
- 검증(Unity MCP): 재컴파일 성공, 5개 프리팹의 archetype 값 확인, Play Mode에서 Archer/Slag 프리팹을 `Enemies` 아래 인스턴스화해 종류와 감지 범위 10 유지 확인, 새 콘솔 에러 없음.
- 수정(색/체력 없음): 원인은 기본 `BasicEnemy` 프리팹에 `EnemyHealth`/`EnemyDeath`/`EnemyStagger`/`EnemyStatusDebugDisplay`가 없고 씬 인스턴스에만 추가돼 있던 것. 이 4개를 기본 프리팹에 적용해 5개 변형이 상속하도록 했다. 재확인(Play Mode, Baron 프리팹 인스턴스화): 체력 5, 체력바·라벨(`TEST: BARON (PARRY OK)`) 생성, 색 (0.85,0.2,0.24) 적용.
- 수정(배치하자마자 죽음): 씬에 직접 배치한 `Enemy_Knight`가 Play 시작 때 `EnemyDeath.dead=True`(체력은 5)로 확인됐다. 원인은 `EnemyDeath.OnEnable`이 `EnemyHealth.Awake`보다 먼저 실행되어 `CurrentHealth`가 0으로 읽히던 초기화 순서 문제. `EnemyHealth`가 처음 읽을 때 지연 초기화하도록 수정했다. 재확인(Play Mode): 직접 배치한 `Enemy_Knight`를 포함한 모든 적이 dead=False, 체력 정상.
- 수정(수동 배치 시 자동 스폰이 안 꺼짐): 기존 판정이 `Enemies` 부모 아래 개수만 세서 루트 등 다른 곳에 배치한 적을 무시했다. 씬 전체의 `BasicEnemyController`가 1개일 때만 자동 스폰하도록 변경. 재확인(Play Mode): 루트에 배치한 `Enemy_Knight`/`Enemy_Baron`이 있을 때 `Test_*` 자동 스폰이 생기지 않음.
- 미검증: 각 프리팹을 직접 배치했을 때의 전투 동작.

## 2026-10-11 - 패링 체감 개선

- 내용: 패링 판정 시간과 입력 버퍼, 공격 전조 색상 단서를 추가했다.
- 변경: `PlayerParry` 패링 지속 0.32→0.45초(TestArena 씬 값도 변경), 쿨다운 종료 0.15초 전 입력 버퍼(`inputBufferTime`); `EnemyAttack` 전조 색상(패링 불가=보라, 패링 가능=주황→빨강, 선딜 마지막 0.4초(`parryCueTime`)=초록/흰색 점멸).
- 검증(Unity MCP, Play Mode, Knight): 전조 색상이 선딜 초반 주황, 마지막 0.4초 초록으로 바뀜을 확인. 타격 0.40초 전에 패링 시작 시 4회 연속 `[Parry] SUCCESS`와 공격 취소 확인. 쿨다운 0.10초 남은 시점의 입력이 버퍼링되어 0.10초 뒤 패링이 시작됨을 확인. 콘솔 에러 0.
- 미검증: 실제 마우스 입력(하니스에서 전조/입력은 코드 호출로 시뮬레이션), 보라색 전조(Pawn/Slag), Archer/Baron 패링.

## 2026-10-11 - 적 테스트 아레나 확장

- 내용: TestArena의 전투 공간을 확장하고 다섯 적 Archetype을 한 번에 테스트하도록 배치했다.
- 기능:
  - 바닥과 상단/측면 프레임 폭을 20유닛에서 32유닛으로 넓히고 테스트 카메라 범위를 조정했다.
  - Play Mode 시작 시 기존 Knight 인스턴스의 전투 구성요소를 복제해 Slag, Pawn, Archer, Baron을 각각 테스트 위치에 추가한다.
  - 모든 테스트 적의 감지 범위를 4.5유닛으로 제한하고 Knight도 시작 위치에서 물려 배치해, 시작 직후 공격으로 플레이어 조작이 막히지 않게 했다. 플레이어가 가까이 가면 각 적 AI가 활성화된다.
  - 모든 적 머리 위에 색상별 `TEST: <이름>` 라벨을 표시해 임시로 구분한다.
- 변경 파일:
  - `Assets/Scenes/Gameplay/TestArena.unity`
  - `Assets/Scripts/Enemies/BasicEnemyController.cs`
  - `Assets/Scripts/Enemies/EnemyStatusDebugDisplay.cs`
  - `Assets/Scripts/Player/PlayerDeathHandler.cs`
  - `Assets/Scripts/Level/TestArenaDebugKeys.cs`
  - `Assets/Scripts/Combat/EnemyAttack.cs`
  - `README.md`
- 검증:
  - `dotnet build "Assembly-CSharp.csproj" --no-restore` 성공, 경고/오류 0건.
  - `git diff --check` 통과. TestArena 바닥/프레임 폭 32, 카메라 크기 9.5 직렬화 값을 확인했다.
  - Unity Editor 로그에서 플레이어가 적 공격으로 Armor/HP를 잃고 사망 후 조작이 비활성화된 기록을 확인했다.
- 원인/수정:
  - Play Mode 진입 직후 `PlayerDeathHandler.OnEnable`이 `PlayerHealth.Awake` 이전에 `IsDead`(CurrentHealth 0)를 읽어 즉시 사망 처리했고, 모든 플레이어 컴포넌트(PlayerController 등)가 비활성화되어 '조작이 안 된다'가 발생했다. 초기 사망 확인을 `Start`로 옮겨 수정했다.
- Play Mode 검증 (Unity MCP, 2026-10-11):
  - Console 에러 0건, 재컴파일 성공. 수정 후 플레이어 컴포넌트가 모두 enabled 상태임을 확인했다.
  - 스폰된 SLAG(-12)/PAWN(-8)/KNIGHT(7)/ARCHER(8)/BARON(12) 5종과 `TEST: 이름` 라벨, HUD(HP 5/5, Armor 5/5)를 게임 뷰 캡처로 확인했다. Knight(7)와 Archer(8)는 겹쳐 보인다.
  - 가상 키보드 입력으로 A 이동(x 1.59 → -3.98)과 Space 점프(y 1.42 → 2.20)를 확인했다. D 이동은 x 0 → 1.59로 확인했다.
  - 아레나 개선(2026-10-11): 적 배치를 Slag -14 / Pawn -8.5 / Knight 7 / Archer 11 / Baron 14.5로 바꿔 Knight·Archer 겹침을 해소하고, 카메라 크기를 11로 올려 5개 라벨이 모두 보임을 게임 뷰 캡처로 확인했다. 신규 `Assets/Scripts/Level/TestArenaDebugKeys.cs`: F2 HP/Armor 회복(5/5 확인), F3 씬 재로드(적 5종 초기 위치 복원, 디버그 키 유지 확인). 키 입력은 하네스 한계로 Update 직접 호출로 검증했다.
  - 패링 조사/수정(2026-10-11): Knight 공격에 맞춰 TryStartParry 호출 시 `[Parry] SUCCESS`(공격 취소, 경직, 강화 포인트 +1) 확인. Slag/Pawn은 설계상 `parryable=false`라 패링되지 않으며(Pawn 투사체 피격 확인), 이를 알 수 있도록 TestArena 라벨에 `(PARRY OK)/(NO PARRY)` 추가(`EnemyAttack.IsParryable`, `EnemyStatusDebugDisplay`). 실제 우클릭 입력 경로는 `Mouse.rightButton` 주입 후 Update 호출로 `IsParryActive=True` 확인. Archer/Baron 패링은 미검증.
  - Player의 중복 PlayerHealth(씬 인스턴스 추가분, startingArmor 0)를 제거해 프리팹 하나만 남김(HP 5, 컴포넌트 1개 확인). TestArena를 Build Settings에 추가하고 사망 후 R 재시작 동작(씬 재로드, 컨트롤 복구) 확인.
  - 시작 시 즉사 없음. Pawn은 4.0 거리에서 제자리 사격(약 2.5초 간격, 투사체 1발 0.75). 플레이어가 4.5 밖에 있는 적은 이동하지 않았다.
  - Knight는 3.0~3.2 거리에서 7.0 → 5.3까지 접근해 약 1.8초 간격으로 1.25씩 피해를 줬고, 무방비 상태에서 약 7초 만에 사망했다. Archer는 8.0 → 8.3으로 후퇴했다.
- 제한:
  - 자동 입력으로 확인하지 못한 것: 대쉬/패링/사격 입력, Slag/Baron 접근·공격(감지 범위 밖에서만 관찰), Archer 사격.
  - 에디터가 백그라운드일 때는 Play 루프가 멈추고 키 입력이 무시되어 검증 중에만 `Application.runInBackground`와 Input System 백그라운드 설정을 임시 변경했다(프로젝트 파일에는 남기지 않음).

## 2026-10-11 - 적 종류별 이동 AI 및 공격

- 내용: 정지 상태에서 공격만 하던 적에게 접근/거리 유지 이동을 추가하고 README의 다섯 적 종류별 전투 프로필을 구현했다.
- 기능:
  - Slag은 천천히 접근해 느린 근접 공격을 하고, Pawn은 느린 비패링 투사체로 공격하며 거리를 유지한다.
  - Knight는 빠르게 접근하는 패링 가능 근접 적이다. Archer는 거리를 벌리며 빠른 패링 가능 투사체를 발사한다.
  - Baron은 높은 체력과 빠른 근접전을 갖고, 멀리 떨어진 플레이어에게 패링 가능한 투사체를 발사한 뒤 접근한다.
  - 적별 체력/이동/공격/투사체 프로필, 색상과 이름표를 추가했다. Baron은 3회 피격 또는 패링 누적 후 경직된다.
  - 패링 가능 투사체를 패링 또는 회피하면 피해를 받지 않고, 패링 성공 보상도 기존 시스템으로 지급된다.
- 변경 파일:
  - `Assets/Scripts/Enemies/BasicEnemyController.cs`
  - `Assets/Scripts/Enemies/EnemyHealth.cs`
  - `Assets/Scripts/Enemies/EnemyStagger.cs`
  - `Assets/Scripts/Enemies/EnemyStatusDebugDisplay.cs`
  - `Assets/Scripts/Combat/EnemyAttack.cs`
  - `Assets/Scripts/Player/PlayerParry.cs` (런타임 적 투사체 포함)
  - `Assets/Prefabs/Enemies/BasicEnemy.prefab`
  - `README.md`
- 검증:
  - Unity C# 프로젝트 빌드 성공, 경고/오류 0건.
  - `git diff --check` 통과.
  - 실시간 이동/투사체/패링 Play Mode 검증은 별도 확인이 필요하다.
- 제한:
  - 적 공격권 공유/동시 공격 제한은 이번 범위에 포함하지 않았다.

## 2026-10-11 - 강화 포인트 무기 강화 및 총기 3종

- 내용: 강화 포인트를 현재 장착 무기의 강화에 사용하고 README의 SMG, Shotgun, Railgun 발사 동작을 구현했다. 아래의 10초 임시 강화 변경 전 동작을 기록한다.
- 기능:
  - HUD에 현재 무기 탄약/이름과 강화 상태를 표시하고 포인트 부족 시 R키 강화가 거부된다.
  - `1`/`2`/`3` 키로 무기를 교체하며 무기별 탄약과 발사 쿨다운을 독립적으로 유지한다.
  - SMG는 빠른 투사체 연사, 강화 시 연사·피해 증가, 적 화상 피해와 명중당 소량의 Armor 회복을 제공한다.
  - Shotgun은 7발 산탄을 퍼뜨리고, 강화 중 탄약 2발로 빠른 2연사를 한다. 탄약이 1발이면 단발 발사한다.
  - Railgun은 히트스캔으로 명중 지점을 계산하며, 강화 중 범위 피해 폭발을 발생시킨다.
- 변경 파일:
  - `Assets/Scripts/Player/PlayerWeapon.cs`
  - `Assets/Scripts/Combat/PlayerProjectile.cs`
  - `Assets/Scripts/Enemies/EnemyBurnEffect.cs`
  - `Assets/Scripts/UI/PlayerStatusDebugCanvas.cs`
  - `README.md`
  - `DEVELOPMENT_LOG.md`
- 검증 결과:
  - Unity MCP 컴파일 성공, compilation failure 0건
  - Play Mode에서 포인트 없이 강화가 거부되고, 포인트 소비 및 강화 단계 갱신을 확인했다.
  - Railgun 히트스캔으로 적 HP가 5에서 0.5로 감소하고 강화 폭발이 발생하는 것을 확인했다.
  - SMG 명중 처리로 적 화상 효과가 시작되고, 화상 틱에서 HP가 3.5에서 3.25로 감소하며 Armor 0.1을 얻는 것을 확인했다.
  - Shotgun 산탄 7발과 강화 후속 산탄 7발, 한 발 남았을 때 단발 발사, 세 무기의 독립 탄약(29/6/3)을 확인했다.
  - HUD에서 `SHOTGUN AMMO`, 강화 단계 및 포인트 표시를 확인했으며 Unity Console 오류/경고 0건, Play Mode 종료 상태 및 `git diff --check` 통과를 확인했다.
- 제한 사항:
  - MCP 평가 중 게임 프레임/입력 장치 진행이 제한되어 `1`/`2`/`3`, R, 좌클릭의 실제 키 입력 주입은 검증하지 않았다. 각 키에 연결된 선택/강화/발사 경로는 런타임 메서드로 실행했다.
  - SMG 투사체 명중 콜백과 화상 틱은 런타임 평가에서 직접 실행했다. 일반 플레이 중의 물리 이동·연속 애니메이션은 별도 수동 체감 확인이 필요하다.
- 다음 작업:
  - 실제 플레이 입력으로 무기 교체, 조준, 발사 및 무기별 강화 체감을 확인한다.

## 2026-10-11 - SMG 연속 사격 및 Shotgun 사거리 상향

- 내용: SMG는 좌클릭 유지 시 자동 연사하고, Shotgun 투사체의 속도/비행 시간과 퍼짐을 조정해 사거리를 늘렸다.
- 변경:
  - SMG 발사는 `WasPressedThisFrame` 대신 `Attack` 액션의 `IsPressed` 상태와 기존 발사 간격을 사용한다. 다른 총기는 클릭당 한 발 동작을 유지한다.
  - Shotgun 투사체 속도를 11에서 16으로, 비행 시간을 2에서 3초로 조정하고 산탄 범위를 32도에서 26도로 좁혔다.
  - TestArena의 PlayerWeapon 직렬화 값을 새 Shotgun 설정과 일치시켰다.
  - 탄약이 비었을 때 같은 무기의 로그가 발사 입력마다 반복되지 않도록 보정했다.
- 변경 파일:
  - `Assets/Scripts/Player/PlayerWeapon.cs`
  - `Assets/Scenes/Gameplay/TestArena.unity`
  - `README.md`
  - `DEVELOPMENT_LOG.md`
- 검증 결과:
  - Unity 컴파일 성공, compilation failure 0건
  - SMG 입력 검증 경로를 `WasPressedThisFrame`에서 누르고 있는 동안 참인 `IsPressed`로 변경했다.
  - TestArena Play Mode에서 Shotgun 7발 발사, 속도 16 × 비행 시간 3초로 계산한 최대 사거리 약 48 게임 단위, 26도 산탄 범위와 직렬화 설정을 확인했다.
  - Unity Console의 검증 구간 오류/경고 0건, Play Mode 종료 상태 및 `git diff --check` 통과를 확인했다.
- 제한 사항:
  - Shotgun의 사거리는 투사체의 이론상 최대 비행 거리다. 적/벽 충돌 또는 화면 경계에 닿으면 그 전에 사라질 수 있다.
  - MCP에서 왼쪽 버튼 상태를 주입했지만 InputAction이 pressed 상태로 전환되지 않아 실제 누르고 있는 입력이 연속 발사하는지는 검증하지 못했다.
- 다음 작업:
  - 실제 플레이 입력으로 SMG 장시간 연사와 Shotgun의 실전 체감 사거리를 확인한다.

## 2026-10-11 - Shotgun 투사체 조기 소멸 수정

- 원인: Shotgun 산탄은 한꺼번에 생성되어 발사 직후 서로 겹치는데, 투사체가 다른 투사체의 Trigger를 장애물 충돌처럼 처리해 바로 파괴될 수 있었다.
- 변경:
  - `PlayerProjectile`이 다른 `PlayerProjectile`과의 Trigger 충돌은 무시하게 해 각 산탄이 적/장애물에 맞거나 수명이 끝날 때까지 유지되도록 했다.
  - README에 산탄끼리 충돌하지 않으며 적중 또는 3초 수명 종료까지 비행한다고 명시했다.
- 검증:
  - 동일 타입 투사체와의 Trigger가 파괴 처리보다 먼저 무시되는 코드 경로를 확인했다.
  - `git diff --check` 통과. 이번 수정에 대한 Unity Play Mode 검증은 수행하지 않았다.

## 2026-10-11 - R 무기 강화 임시 효과화

- 내용: R키 강화가 중첩 누적되지 않고 10초 후 기본 상태로 돌아오도록 변경했다.
- 변경:
  - 강화 포인트 1개로 현재 무기를 10초간 강화하며, 강화 중 다시 사용하면 포인트를 소모하고 해당 무기의 지속 시간만 갱신한다.
  - 무기별 타이머를 독립적으로 관리하고, 만료 시 강화 수치를 0으로 되돌려 기본 발사 동작을 복구한다.
  - HUD에 강화 남은 시간을 표시하고, 만료 후 다시 강화할 수 있도록 갱신한다.
  - README의 강화 규칙과 Railgun 효과 설명을 업데이트했다.
- 검증:
  - Unity C# 프로젝트 빌드 성공, 경고/오류 0건.
  - `git diff --check` 통과.
  - Play Mode에서 강화 만료 및 재사용 흐름은 아직 검증하지 않았다.

## 2026-10-11 - 저체력 경고 HUD

- 내용: 플레이어 체력이 최대치의 20% 이하일 때 화면 상단에 맥동하는 `CRITICAL HEALTH` 경고를 표시한다.
- 기능:
  - 체력이 임계치 이하이고 생존 중일 때 경고 배경과 문구를 맥동시킨다.
  - 체력이 임계치 위로 회복되거나 사망하면 경고를 숨긴다.
  - 임계치는 HUD Inspector에서 5~50% 범위로 조정 가능하다.
- 변경 파일:
  - `Assets/Scripts/UI/PlayerStatusDebugCanvas.cs`
  - `DEVELOPMENT_LOG.md`
- 검증 결과:
  - Unity MCP Recompile 성공, 컴파일 실패 0건
  - TestArena Play Mode에서 HP 1/5일 때 경고가 표시되고, 화면 캡처로 상단 HUD 배치와 기존 HUD 공존을 확인했다.
  - HP를 2/5로 회복하면 경고가 숨고, HP 0으로 사망해도 경고가 숨는 것을 확인했다.
  - Unity Console ground-truth 오류/경고 0건, Play Mode 종료 상태 및 `git diff --check` 통과를 확인했다.
- 다음 작업:
  - 플레이어 사망 시 전투 입력 및 상태를 안전하게 종료하고 패배 피드백을 표시한다.

## 2026-10-11 - 플레이어 사망 시 전투 종료

- 내용: 플레이어가 사망하면 조작과 전투를 중단하고 HUD에 패배 화면을 표시한다.
- 변경:
  - PlayerHealth가 HP 0 도달 시 `Died` 이벤트를 한 번 발생시킨다.
  - PlayerDeathHandler가 입력 및 플레이어 동작 컴포넌트를 비활성화하고 Rigidbody를 정지/고정한다.
  - HUD는 저체력 경고를 숨기고 화면 중앙에 패배 오버레이를 표시한다.
  - Player Prefab에 사망 핸들러를 추가해 해당 Prefab을 사용하는 씬에 적용한다.
- 검증:
  - Unity C# 프로젝트 빌드 성공, 경고/오류 0건.
  - `git diff --check` 통과.
  - Unity Play Mode에서 실제 사망 입력/피드백은 확인하지 않았다.

## 2026-10-11 - 패배 후 씬 재시작

- 내용: 플레이어 사망 화면에서 R 키를 누르면 현재 씬을 다시 로드한다.
- 변경:
  - 패배 상태에서만 R 키가 재시작 입력으로 처리되며, 생존 중의 R 무기 강화와 충돌하지 않는다.
  - 패배 화면에 `PRESS R TO RETRY` 안내를 표시한다.
  - TestArena를 Build Settings에 등록해 해당 씬에서도 재시작할 수 있도록 했다.
  - 현재 씬이 Build Settings에 없을 경우 재시작 실패를 명시적으로 Console 오류로 알린다.
- 검증:
  - Unity C# 프로젝트 빌드 성공, 경고/오류 0건.
  - `git diff --check` 통과.
  - Unity Play Mode에서 사망 후 씬 재로드는 아직 검증하지 않았다.

## 2026-10-11 - 플레이어 피격 화면 플래시

- 내용: 체력 또는 방어구가 감소할 때 화면 전체에 짧고 투명한 컬러 플래시를 표시한다.
- 기능:
  - 체력 피해는 붉은색, 방어구 피해는 주황색으로 구분
  - 회복으로 수치가 증가할 때는 플래시를 재생하지 않음
  - 약 0.38초 동안 알파를 감쇠하며 HUD 위에 배치하고 UI 입력은 가로채지 않음
- 변경 파일:
  - `Assets/Scripts/UI/PlayerStatusDebugCanvas.cs`
  - `DEVELOPMENT_LOG.md`
- 검증 결과:
  - Unity MCP Recompile 성공, 컴파일 오류 없음
  - Play Mode에서 방어구 감소 시 주황색, 방어구 소진 후 체력 감소 시 붉은색 플래시를 확인했다.
  - 체력/방어구 회복은 피격 플래시를 새로 시작하지 않는 것을 확인했다.
  - HUD 합성 Game View에서 기존 패널과의 공존을 확인했으며, 현재 Unity Console 오류 및 경고 0건, Play Mode 종료 상태를 확인했다.
- 다음 작업:
  - 실제 적 공격으로 방어구/체력이 감소할 때 플래시 강도와 HUD 대비를 플레이 확인한다.

## 2026-10-11 - 적 머리 위 체력 바

- 내용: TestArena 적의 남은 체력을 즉시 읽을 수 있도록 머리 위에 얇은 HP 바를 표시한다.
- 기능:
  - 체력 변경 이벤트에 따라 왼쪽 기준으로 바 길이를 갱신
  - 체력이 높을 때 청록색, 낮을수록 적색으로 색상 변경
  - 체력 0 또는 상태 표시 컴포넌트 비활성화 시 바를 숨겨 사망 연출과 겹치지 않게 처리
  - 기존 STAGGER/STUN 텍스트보다 낮은 정렬 순서로 바를 표시
- 변경 파일:
  - `Assets/Scripts/Enemies/EnemyStatusDebugDisplay.cs`
  - `DEVELOPMENT_LOG.md`
- 검증 결과:
  - Unity MCP Recompile 성공, 컴파일 오류 없음
  - TestArena Game View에서 체력 바와 기존 STAGGER 표시의 공존을 확인했다.
  - Play Mode에서 HP 5에서 3으로 피해를 주었을 때 채움 비율이 1.0에서 0.6으로 줄고 색상도 체력에 맞게 갱신되는 것을 확인했다.
  - Unity Console의 현재 컴파일 오류/경고 0건, Play Mode 종료 상태 및 `git diff --check` 통과를 확인했다.
- 다음 작업:
  - 실제 투사체 전투 중 체력 바 가독성을 확인하고, 필요하면 상태 텍스트와 위치 간격을 조정한다.

## 2026-10-11 - 적 사망 시 넘어짐/축소 연출

- 내용: 체력이 0이 된 적의 전투 비주얼을 짧게 넘어뜨리고 축소한 뒤 숨기도록 했다.
- 기능:
  - 사망 시 기존 공격, 이동, 충돌 비활성화에 더해 Stun/Stagger 상태 표시를 정리
  - 적 시각 오브젝트를 바라보는 방향으로 눕히고 아래로 내리며 축소한 후 비활성화
  - 연출 지속 시간, 기울기, 축소량, 낙하 거리를 Inspector에서 조정 가능
- 변경 파일:
  - `Assets/Scripts/Enemies/EnemyDeath.cs`
  - `Assets/Scenes/Gameplay/TestArena.unity`
  - `DEVELOPMENT_LOG.md`
- 검증 결과:
  - Unity 컴파일 성공, 오류 없음
  - TestArena Play Mode에서 실제 Physics2D 트리거에 투사체 5개를 통과시켜 HP 5에서 0까지 감소시키고 EnemyDeath 발동, 적 충돌 비활성화 및 사망 연출 코루틴 시작을 확인했다.
  - Unity Console의 현재 컴파일 오류 및 경고 0건, Play Mode 종료 상태를 확인했다.
- 제한 사항:
  - MCP Play Mode 검증은 트리거와 사망 코루틴 시작까지 확인했다. 원격 Editor에서 코루틴이 여러 렌더 프레임 동안 진행하는 마지막 비주얼 상태는 별도 수동 플레이로 확인할 수 있다.
- 다음 작업:
  - TestArena에서 실제 플레이로 적 사망 넘어짐/축소 타이밍과 화면상 가독성을 확인한다.

## 2026-10-11 - 적 피격 순간 색상 피드백

- 내용: 적이 피해를 받을 때 짧은 흰색 플래시를 주어 명중 순간을 알아보기 쉽게 했다.
- 기능:
  - EnemyHealth의 체력 감소 이벤트에 반응해 적 시각 오브젝트만 플래시
  - MaterialPropertyBlock으로 원래 색상에 돌아오도록 처리해 공유 머티리얼 에셋을 변경하지 않음
  - 연속 피격 시 플래시를 다시 시작하고, 컴포넌트 비활성화 시 색상을 복구
  - BasicEnemy 프리팹에 피드백 컴포넌트 추가
- 변경 파일:
  - `Assets/Scripts/Enemies/EnemyHitFeedback.cs`
  - `Assets/Scripts/Enemies/EnemyHitFeedback.cs.meta`
  - `Assets/Prefabs/Enemies/BasicEnemy.prefab`
  - `DEVELOPMENT_LOG.md`
- 검증 결과:
  - Unity MCP Recompile 성공, 컴파일 오류 없음
  - TestArena Play Mode에서 피해 이벤트 후 렌더러 색이 흰색으로 바뀌고, 공유 머티리얼의 원래 주황색은 그대로인 것을 확인했다.
  - 색상 복구 루틴 실행 후 렌더러가 원래 주황색으로 복구되는 것을 확인했다.
  - Unity Game View 캡처에서 적의 흰색 피격 표시를 확인했고, 현재 Console 오류/경고 0건 및 Play Mode 종료 상태를 확인했다.
- 다음 작업:
  - 적 사망 피드백과 피격 타이밍을 실제 투사체 충돌로 플레이 확인한다.

## 2026-10-11 - 대쉬/패링 성공 및 보상 흐름 런타임 검증

- 내용: TestArena Play Mode에서 공격 취소, 성공 피드백, 강화 포인트 보상, HUD 쿨다운 표시가 함께 동작하는지 검증했다.
- 검증 결과:
  - 공격 활성 상태를 구성하고 PlayerParry의 입력 시작 루틴과 충돌 콜백을 호출해 패링 성공, 적 Stun, 공격 히트박스 취소 및 포인트 1점 지급을 확인했다.
  - 같은 공격을 다시 처리해도 보상이 중복 지급되지 않음을 확인했다.
  - 대쉬 중 상태에서 EnemyAttack의 실제 Update 판정을 호출해 회피 성공, 공격 취소 및 포인트 추가 지급을 확인했다.
  - HUD 피드백 `PARRY SUCCESS!` / `DODGE SUCCESS!`와 대쉬 쿨다운 `0 / 1  0.7s`를 확인했다.
  - Unity Console의 현재 컴파일 오류 및 경고 0건, Play Mode 종료 상태를 확인했다.
- 제한 사항:
  - MCP 입력 주입 및 수동 플레이를 통한 실제 마우스/키보드 입력과 Unity 물리 충돌 타이밍은 이 검증에 포함되지 않았다. 컴포넌트 로직을 실제 상태 조합으로 실행해 기능 연결을 확인한 테스트다.
- 다음 작업:
  - Editor에서 실제 플레이 입력으로 대쉬와 패링 타이밍을 체감 확인하고 필요하면 판정 구간을 조정한다.

## 2026-10-11 - HUD 대쉬/패링 사용 가능 횟수

- 내용: HUD 우측 하단에 대쉬와 패링의 현재 사용 가능 횟수를 추가했다.
- 기능:
  - 사용 가능 시 `1 / 1 READY`, 재사용 대기 중에는 `0 / 1`과 남은 쿨다운 표시
  - 게임플레이의 대쉬/패링 준비 판정과 HUD가 같은 공개 상태 속성을 사용
  - 대쉬 또는 패링 컴포넌트가 아직 없어도 다른 HUD 값은 계속 갱신
- 변경 파일:
  - `Assets/Scripts/Player/PlayerDodge.cs`
  - `Assets/Scripts/Player/PlayerParry.cs`
  - `Assets/Scripts/UI/PlayerStatusDebugCanvas.cs`
  - `DEVELOPMENT_LOG.md`
- 검증 결과:
  - Unity MCP Recompile 성공, 컴파일 오류 없음
  - TestArena Play Mode에서 대쉬/패링 모두 `1 / 1 READY`로 표시되는 것과 쿨다운 중 `0 / 1  1.4s`로 표시되는 것을 확인했다.
  - 테스트 후 Play Mode를 종료했으며 Unity Console의 현재 컴파일 오류와 경고는 0건이다.

## 2026-09-21 - 회피 성공 강화 포인트 보상

- 내용: 회피 성공 시 강화 포인트를 실제로 지급하도록 연결했다.
- 기능:
  - `PlayerDodge.TryRegisterSuccess()`에서 회피 성공 시 1점 보상 지급
  - 보상 기여를 위해 `PlayerEnhancementPoints`를 해당 Player에 보장 초기화
  - 기존 HUD 피드백과 보상 이벤트 흐름을 유지
- 변경 파일:
  - `Assets/Scripts/Player/PlayerDodge.cs`
  - `DEVELOPMENT_LOG.md`
- 검증 결과:
  - C# 진단 오류 없음
  - `dotnet build Assembly-CSharp.csproj`로 컴파일 확인
  - 회피 성공 시 `enhancementPoints.AddPoints(1)` 경로가 연결되어 확인 가능
- 제한 사항:
  - Unity Play Mode에서 실제 적 공격 회피 후 포인트 증가 HUD 반영은 Editor에서 최종 체크가 필요하다.
- 다음 작업:
  - 회피/패링 성공 메시지와 포인트 소모/보상 흐름을 실제 전투에서 반복 검증한다.

## 2026-09-21 - 강화 포인트 사용 총기 업그레이드

- 내용: 보유한 강화 포인트를 사용해 현재 총기를 업그레이드하는 기본 로직을 추가했다.
- 기능:
  - R 키 입력 시 보유 포인트를 확인
  - 포인트가 있으면 1점 소모 후 총기 피해량/속도 상승
  - 포인트 부족 시 조용히 무시하고 로그만 출력
  - `PlayerWeapon`에 직접 강화 포인트 소모 로직 연결
- 변경 파일:
  - `Assets/Scripts/Player/PlayerEnhancementPoints.cs`
  - `Assets/Scripts/Player/PlayerWeapon.cs`
  - `DEVELOPMENT_LOG.md`
- 검증 결과:
  - C# 진단 오류 없음
  - `dotnet build Assembly-CSharp.csproj` 성공
  - `TryUpgradeWeapon()`와 `TrySpendPoints(1)` 연결 확인
- 제한 사항:
  - 업그레이드 효과의 숫자 밸런스, HUD 반영, 실제 키 입력으로 전투 중 사용 여부는 Editor에서 최종 검증이 필요하다.
- 다음 작업:
  - 강화 포인트 소모 UI와 업그레이드 효과를 테스트 Arena에서 실제로 확인하고 필요 시 튜닝한다.

## 2026-09-21 - Player Parry + Enemy Stun 완료

- 내용: 적 공격의 짧은 활성 구간에서 우클릭 패링을 성공하면 현재 공격을 즉시 취소하고 적을 Stun 상태로 전환한다. Stun 상태는 짧은 시간 동안 새 공격을 막고, 종료 후 공격을 재개할 수 있도록 구성했다.
- 구현 내용:
  - `PlayerParry`가 우클릭 입력으로 `ParryHitbox`를 활성화하고 공격 중인 `EnemyAttack`의 Trigger와 충돌 시 처리
  - `EnemyAttack.TryRegisterParry()`로 1회 공격당 최대 1번의 패링 성공만 허용
  - 패링 성공 시 `EnemyAttack.CancelCurrentAttack()`로 현재 windup/active hitbox 종료
  - `EnemyStun.Stun()`으로 적을 기절 상태로 전환하고 `StunDuration` 경과 후 복귀
  - `EnemyAttack.Update()`에서 `enemyStun.IsStunned` 또는 `enemyStagger.IsStaggered` 상태일 때 추가 공격 방지
  - `PlayerStatusDebugCanvas`에 패링 성공 텍스트 피드백 추가
- 변경 파일:
  - `Assets/Scripts/Player/PlayerParry.cs`
  - `Assets/Scripts/Combat/EnemyAttack.cs`
  - `Assets/Scripts/Enemies/EnemyStun.cs`
  - `Assets/Scripts/UI/PlayerStatusDebugCanvas.cs`
  - `DEVELOPMENT_LOG.md`
- 검증 결과:
  - VS Code C# 진단 결과: `No errors found`
  - 스크립트 흐름상 핵심 동작 확인: 파리 시작 → 공격 활성 구간 감지 → `TryRegisterParry()` true → `CancelCurrentAttack()` → `enemyStun.Stun()` → `IsStunned` 동안 공격 중지
  - Unity Console Error는 현재 진단 기준 0건으로 확인되었다.
- 확인하지 못한 제한 사항:
  - 실제 TestArena에서 마우스 우클릭 판정과 EnemyHitbox의 Trigger 충돌이 Scene에서 정확히 발생하는지 추가 Play Mode 확인이 필요하다.
  - 패링 성공 직후 HUD 텍스트와 EnemyStatusDebug 표시가 화면에 일치하는지 Editor에서 최종 검증이 필요하다.
- 다음 작업 또는 후속 확인 사항:
  - Unity MCP 또는 Unity Editor에서 TestArena Play Mode 진입 후 실제 적의 공격 타이밍과 패링 성공을 재현해 `PARRY SUCCESS!`와 적 Stun 표시를 확인한다.
  - 필요하면 패링 입력 감도, Stun 지속 시간, 패링 hitbox offset을 추가 조정한다.

## 현재 세션 이어가기 위한 시작점

현재 작업은 패링/기절이 구현된 상태이며, 마지막으로 Unity Play Mode에서 실제 충돌 타이밍과 상태 표시를 확인해 입력 감도와 hitbox 정렬을 조정하는 단계가 남아 있다. 패링 성공, 적 Stun, Stun 종료 후 재공격 흐름을 한 번 더 확인하면 기능은 안정적으로 마무리된다.
- 제한: 실제 키보드/마우스 입력에 따른 대시 방향 체감은 Editor에서 직접 플레이하며 추가 확인이 필요하다.


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


## 2026-09-21 - 패링 성공 강화 포인트

- 내용: 패링 성공 시 플레이어에게 강화 포인트를 지급하고 테스트 HUD에 현재 포인트를 표시하도록 추가했다.
- 기능:
  - 패링 성공 1회당 강화 포인트 1 지급
  - 최대 포인트 및 시작 포인트 Inspector 설정
  - 포인트 변경 이벤트 제공
  - 기존 Player에 보상 컴포넌트가 없으면 런타임에 초기화
  - 테스트 HUD에 `POINTS` 표시 및 변경 즉시 갱신
- 변경 파일:
  - `Assets/Scripts/Player/PlayerEnhancementPoints.cs`
  - `Assets/Scripts/Player/PlayerParry.cs`
  - `Assets/Scripts/UI/PlayerStatusDebugCanvas.cs`
  - `DEVELOPMENT_LOG.md`
- 검증: 세 파일 C# 진단 오류 없음. `dotnet build Assembly-CSharp.csproj` 성공.
- 제한: Unity Play Mode에서 실제 패링 성공 후 포인트 증가와 HUD 갱신은 아직 확인하지 않았다.
- 다음 작업: Unity MCP Play Mode에서 패링 성공을 재현해 포인트 `0->1`과 HUD 표시를 확인한다.


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


## 2026-09-18 - HUD 초기값 및 갱신 재수정

- 문제: HUD Text 생성 직후 초기 문자열을 설정하지 않아 첫 프레임에 이전 HP/Armor 값이 보일 수 있었다.
- 수정: Canvas 생성 직후 `UpdateStatusText()`를 호출해 현재 PlayerHealth 값을 즉시 반영했다.
- 검증: Unity MCP에서 초기 Text `HP: 5/5 | ARMOR: 5/5`, 피해 적용 후 `HP: 5/5 | ARMOR: 4/5`를 확인했다. Console Error 0건.


## 2026-09-18 - 임시 HUD 중복 표시 제거

- 문제: 기존 IMGUI HP/Armor 표시와 Canvas 기반 HUD가 동시에 활성화되어 텍스트가 겹쳤다.
- 수정: TestArena `/UI` 오브젝트에서 `PlayerHealthDebugDisplay`와 `PlayerArmorDebugDisplay`를 제거하고 `PlayerStatusDebugCanvas`만 유지했다.
- 검증: Unity MCP Game 화면에서 `HP: 5/5 | ARMOR: 0/5` 한 줄만 표시되고 Console Error 0건을 확인했다.


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


## 2026-09-18 - 임시 HP HUD MCP 검증 및 수정

- 문제: 수동으로 추가한 Unity YAML FileID가 signed 64-bit 범위를 초과했고, `GUI.skin`을 `Awake`에서 호출해 HUD가 표시되지 않았다.
- 수정:
  - Player Prefab과 TestArena UI의 FileID를 유효한 값으로 교체
  - PlayerHealth와 PlayerHealthDebugDisplay를 Unity MCP로 Scene 오브젝트에 직접 연결하고 Scene 저장
  - GUI 스타일 생성을 `OnGUI` 내부로 이동
- 검증: Unity MCP로 TestArena Play Mode 실행, Console Error 0건, Game 화면에서 `HP: 5/5` 표시 확인.


## 2026-09-18 - 임시 Player HP 표시

- 내용: TestArena에서 Player HP를 확인할 수 있도록 화면 좌측 상단에 임시 텍스트를 추가했다.
- 변경 파일:
  - `Assets/Scripts/UI/PlayerHealthDebugDisplay.cs`
  - `Assets/Scripts/UI/PlayerHealthDebugDisplay.cs.meta`
  - `Assets/Scenes/Gameplay/TestArena.unity`
  - `DEVELOPMENT_LOG.md`
- 검증: UI 스크립트 C# 진단 오류 없음. `dotnet build Assembly-CSharp.csproj` 성공. Play Mode에서 화면 표시 위치와 HP 감소에 따른 텍스트 갱신은 Editor 확인이 필요하다.
- 제한: 정식 Canvas/HUD, HP 게이지, Armor 표시는 아직 구현하지 않았다.


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

## 2026-10-11 - 패링/회피 성공 전투 이펙트

- 내용: 패링과 회피 성공 순간을 더 선명하게 읽을 수 있도록 플레이어 주변에 런타임 전투 이펙트를 추가했다.
- 기능:
  - 패링 성공 시 청록색 충격 링, 검격 아크, 방사형 스파크 표시
  - 회피 성공 시 방향을 따라가는 금색 충격 링과 잔상 표시
  - 기존 판정, 이동, 보상 흐름은 변경하지 않고 성공 지점에 시각 효과만 연결
  - 이펙트 종료 및 Player 비활성화 시 런타임 오브젝트와 머티리얼 정리
- 변경 파일:
  - `Assets/Scripts/Player/PlayerCombatFeedbackVFX.cs`
  - `Assets/Scripts/Player/PlayerCombatFeedbackVFX.cs.meta`
  - `Assets/Scripts/Player/PlayerParry.cs`
  - `Assets/Scripts/Player/PlayerDodge.cs`
  - `DEVELOPMENT_LOG.md`
- 검증 결과:
  - Unity MCP Recompile 완료, `compilationFailed=false` 및 컴파일 오류 0건을 확인했다.
  - TestArena Play Mode에서 패링/회피 이펙트 메서드를 각각 실행해 Game View 렌더링을 확인했고, 테스트 후 Play Mode를 종료했다.
  - 테스트 중 신규 컴파일/런타임 오류는 발생하지 않았다.
- 제한 사항:
  - 입력 장치 대신 Unity MCP에서 이펙트 메서드를 직접 호출해 시각 표시를 검증했다. 실제 패링/회피 성공 입력 경로는 Editor에서 별도 확인이 필요하다.
- 다음 작업:
  - TestArena에서 실제 패링/회피 성공 입력 경로와 보상 흐름을 확인한다.

## 2026-10-11 - 피격 카메라 흔들림

- 내용: 기존 피격 화면 플래시에 카메라 흔들림 피드백을 추가했다.
- 동작:
  - 방어구 피해는 주황색, 체력 피해는 붉은색 플래시와 짧은 카메라 흔들림을 발생시킨다.
  - 회복은 피격 흔들림을 발생시키지 않는다.
  - 흔들림 종료 시 카메라의 원래 로컬 위치로 복구한다.
- 변경 파일:
  - `Assets/Scripts/Player/PlayerHealth.cs`
  - `Assets/Scripts/UI/PlayerStatusDebugCanvas.cs`
  - `DEVELOPMENT_LOG.md`
- 검증 결과:
  - Unity MCP Play Mode에서 방어구 및 체력 피해 이벤트가 각각 주황색/붉은색 플래시와 흔들림 타이머를 시작하는 것을 확인했다.
  - 결정적 흔들림 평가에서 카메라가 원점에서 이동했고, 종료 시 원래 위치로 정확히 복귀했다.
  - 회복은 진행 중인 흔들림 타이머를 재시작하지 않았다.
  - TestArena에서 적 공격 히트박스를 실제 Physics2D 트리거 시뮬레이션으로 플레이어에 충돌시켜 방어구 감소, 주황색 플래시 및 흔들림 이벤트가 함께 발생하는 것을 확인했다.
  - Unity 컴파일 실패 0건, Console ground-truth 오류/경고 0건을 확인하고 Play Mode를 종료했다.
- 제한 사항:
  - MCP 평가 환경에서 자연적인 프레임 진행과 실제 키보드/마우스 입력은 확인하지 못했다. 카메라의 흔들림 변위와 복귀는 결정적 런타임 평가로, 적 공격 피해 연결은 Physics2D 트리거 시뮬레이션으로 각각 확인했다.
- 다음 작업:
  - TestArena를 실제 플레이해 피격 피드백의 연속 프레임 타이밍과 체감을 확인한다.

## 2026-10-11 - HUD 게이지 및 전투 자원 패널

- 내용: 한 줄 디버그 수치 표시를 좌측 하단 생존 상태 게이지와 우측 하단 전투 자원 패널로 교체했다.
- 기능:
  - HP와 Armor를 색상 게이지와 현재/최대 수치로 표시
  - 탄약과 강화 포인트를 분리된 패널에 표시하고, 탄약이 적을 때 숫자를 강조
  - 기준 해상도에 맞춰 Canvas UI 크기가 화면 비율에 따라 조정되도록 설정
  - 씬의 Player에는 PlayerHealth 컴포넌트가 중복되어 있어, 임의의 타입 검색 대신 Player 오브젝트의 컴포넌트를 사용해 HUD와 게임플레이 상태를 일치시켰다.
- 변경 파일:
  - `Assets/Scripts/UI/PlayerStatusDebugCanvas.cs`
  - `DEVELOPMENT_LOG.md`
- 검증 결과:
  - Unity MCP Recompile 성공, 컴파일 오류 없음
  - TestArena Play Mode에서 피해와 강화 포인트 변경 후 HUD의 HP `3/5`, Armor `0/5`, 강화 포인트 `7`이 실제 Player 상태와 일치하는 것을 확인했다.
  - Play Mode 종료 후 Unity Console의 현재 컴파일 오류 및 경고 0건을 확인했다.
- 제한 사항:
  - 근접 공격 포인트를 제공하는 런타임 컴포넌트가 없어 이 HUD에는 포함하지 않았다.
- 다음 작업:
  - TestArena의 실제 패링/회피 성공 입력 경로와 보상 흐름을 확인한다.

## 2026-10-11 - TestArena 네온 전투장 연출

- 내용: TestArena의 단색 테스트 화면에 전투 공간을 읽기 쉽게 해주는 어두운 아레나 배경과 색상 포인트를 추가했다.
- 기능:
  - 어두운 배경 위에 도시 실루엣과 청록색 상단 조명, 경기장 프레임을 배치
  - 바닥과 발판을 어두운 재질로 정리하고, 플레이어는 청록색, 적은 호박색으로 구분
  - 변경은 TestArena 씬 및 이 씬 전용 재질에 한정하고 기존 충돌 지오메트리는 유지
- 변경 파일:
  - `Assets/Scenes/Gameplay/TestArena.unity`
  - `Assets/Art/Arena/Materials/`
  - `DEVELOPMENT_LOG.md`
- 검증 결과:
  - Unity MCP에서 씬 저장 후 Game View를 캡처해 배경, 발판, 색상 구분이 표시되는 것을 확인했다.
  - Play Mode 종료 후 TestArena가 저장된 상태이며 Unity Console의 현재 컴파일 오류 및 경고는 0건이다.
- 제한 사항:
  - TestArena용 시각 개선이며 다른 게임 씬은 변경하지 않았다.
- 다음 작업:
  - 적 공격 예고가 패링/회피 성공 및 공격 취소 흐름에서도 올바르게 복구되는지 확인한다.

## 2026-10-11 - 적 공격 예고 및 타격 포즈

- 내용: TestArena에서 적의 공격 준비와 공격 순간을 알아보기 쉽도록 기존 적 색상에 맞춘 예고 바와 짧은 포즈 연출을 추가했다.
- 기능:
  - 공격 준비 중 예고 바가 주황색에서 적색으로 변하며 크기가 맥동
  - windup 동안 적이 몸을 낮추고 뒤로 기울고, 공격 활성 시간에는 짧게 앞으로 기울어 타격 순간을 구분
  - 공격 취소 및 비활성화 시 예고 표시, 시각 변형과 재질 색상을 원래대로 복구
  - 기존 공격 타이밍, 히트박스 및 피해 판정은 유지
- 변경 파일:
  - `Assets/Scripts/Combat/EnemyAttack.cs`
  - `Assets/Scenes/Gameplay/TestArena.unity`
  - `DEVELOPMENT_LOG.md`
- 검증 결과:
  - Unity MCP Recompile 성공, 컴파일 오류 없음
  - TestArena Game View에서 적 windup 예고와 선행 포즈를 확인했다.
  - Play Mode에서 공격 활성 포즈(스케일/기울기) 분기를 직접 미리 보고, 종료 시 원래 포즈로 복구되는 것을 확인했다.
  - Play Mode에서 공격 windup 및 표시 활성 상태를 확인하고, 공격 취소 후 예고/히트박스 비활성화와 적 시각 스케일 복구를 확인했다.
  - 테스트 종료 후 Play Mode를 중단했으며 Unity Console의 현재 컴파일 오류 및 경고는 0건이다.
- 제한 사항:
  - 공격 타격 타이밍과 실제 플레이어 입력을 이용한 패링/회피 성공 검증은 별도 작업으로 남아 있다.
- 다음 작업:
  - TestArena에서 실제 패링/회피 성공 입력 경로와 보상 흐름을 확인한다.
