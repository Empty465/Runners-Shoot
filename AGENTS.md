# AGENTS.md

새 세션이나 개발 시작 시 이 파일, `README.md`, `DEVELOPMENT_LOG.md`를 먼저 읽는다.

## Unity MCP 사용 원칙

- Unity MCP(`unity` 서버)는 **토큰 소모가 크므로 필요할 때만 최소한으로 사용한다.** 기본은 코드 수정 후 재컴파일/Console 에러 확인 정도이며, Play Mode 검증·스크린샷·긴 콘솔 출력은 사용자가 요청하거나 꼭 필요할 때만 한다.
- 콘솔을 읽을 때는 `tail`/`level`을 좁게 지정한다.
- MCP를 쓰려 했는데 사용할 수 없으면(도구 미노출, 연결·타임아웃·에디터 미응답 등) 조용히 넘어가지 말고 그 사실과 오류 문구를 그대로 보고한다.
- MCP로 확인하지 못한 내용은 "검증됨"으로 쓰지 않는다. README/DEVELOPMENT_LOG에는 실제로 확인한 내용만 반영하고, 미검증 항목은 미검증이라고 적는다.

## MCP 사용 시 참고 (검증된 사항)

- TestArena 씬 경로: `Scenes/Gameplay/TestArena` (`Assets/Scenes/Gameplay/TestArena.unity`).
- 에디터가 비활성(포커스 없음)일 때 Play 루프가 멈출 수 있다. 검증용으로 `Application.runInBackground = true`를 eval로 설정하되, `ProjectSettings`에 저장된 값이 남지 않았는지 `git status`로 확인한다.
- 가상 키보드/마우스 입력은 하네스에서 불안정하다. 필요하면 입력 상태를 큐에 넣은 뒤 해당 컴포넌트의 `Update`를 리플렉션으로 호출해 검증하고, 그 방식이었음을 기록에 남긴다.
- Play Mode 중에는 `add_scene_to_build` 등 일부 명령이 거부된다. 먼저 `editor_stop`을 호출한다.
