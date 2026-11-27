# 전략 탐색 검증

저장소 루트에서 실행한다.

```powershell
dotnet build Tests/StrategicSearchChecks -c Release --no-restore -m:1
dotnet Tests/StrategicSearchChecks/bin/Release/net10.0/StrategicSearchChecks.dll --baseline
dotnet Tests/StrategicSearchChecks/bin/Release/net10.0/StrategicSearchChecks.dll --arena
```

기본 검사는 작은 그래프에서 독립 판정기와 합법 간선 전체, 최선의 수, 확정 점수의 부호와 승패까지의 거리를 비교한다. 판정기는 실제 단어 하나를 제거한 사본에서 재귀한다. 교대 대전은 같은 시작 국면에서 구형과 신형의 선후공을 교환한다. 넓은 인공 그래프와 보호막 출구 검사는 일반 사전의 승률 측정과 별개다.

시간 측정은 준비 실행 이후 국면당 8회, 수당 700ms로 진행한다. 95백분위는 올림 순위로 계산하므로 표본 8개에서는 최댓값과 같다. 노드는 양쪽 모두 본체 재귀 호출 횟수다. 구형 평가 시간은 별도 재귀 평가 함수의 시간, 신형 평가 시간은 깊이 한계에서 합법 수와 평가를 구하는 시간이다. 나머지 시간에는 후보 준비, 노드 전개, 정렬과 복원이 포함된다. 간선 캐시를 만드는 시간과 최종 단어 조회 시간은 실제 대국의 진단 파일에서 확인한다.

## 대국 한 턴 기록하기

게임 프로세스를 시작하기 전에 환경 변수 `KKUTU_STRATEGIC_TRACE`에 쓸 수 있는 폴더 경로를 지정한다. 해당 모드에서만 턴마다 압축 사전과 사용 단어를 포함하는 JSON을 저장한다. `Observations`에는 깊이와 방문 순서, 간선, 선택자 관점의 점수가 있다. ‘정확’과 ‘동점’은 그 탐색 깊이에서의 결과다. ‘상한’인 후보의 실제 점수는 기록 이하이고, 시간 초과 위치 이후에는 해당 반복에서 비교하지 않은 후보가 남을 수 있다. 진단 중에는 동점과 상한을 구분하기 위해 루트의 탐색 창을 한 점 넓히므로 완료 깊이와 시간에 차이가 생길 수 있다. 파일 기록 비용은 별도로 발생한다.

```powershell
dotnet Tests/StrategicSearchChecks/bin/Release/net10.0/StrategicSearchChecks.dll --replay C:\temp\turn.json
```

저장한 그래프와 국면을 사용하므로 재실행에는 데이터베이스가 필요 없다. 시간 제한 탐색은 컴퓨터 부하에 따라 마지막 완료 깊이가 달라질 수 있다. 자체 압축 그래프도 같은 형식으로 읽는다.

```json
{"Name":"예시","Initial":"벽","Shield":0,"Used":["벽널"],"Kills":[],"Losses":[],"Edges":[{"First":"벽","Last":"널","Amount":2},{"First":"널","Last":"벽","Amount":1}]}
```

`Amount`는 사용 단어를 빼기 전의 간선별 단어 수다. `Totals`를 생략하면 간선을 합산한다. 실제 대국 파일에서는 보호막 검사에서 세는 전체 단어 수와 제출 가능한 한글 단어 수의 차이를 보존하려고 원래 합계도 저장한다. 데이터베이스가 연결된 환경에서는 `--db`로 일부 사용 기록을 넣어 정상 진입점의 단어 선택과 승인을 검사할 수 있다.
