# Protein Fruit

Unity로 개발한 뱀서라이크 2D 서바이벌 게임입니다.

## 장르 및 테마
과일 vs 채소 / 생존 시간에 따라 재화(단백질) 획득

## 사용 기술
- Unity 2022.3 LTS / C#
- New Input System

## 구현 내용

### 아키텍처
- DontDestroyOnLoad 기반 매니저 싱글톤 구조 (GameManager, PoolManager, UIManager 등)
- GameState FSM으로 씬 전환 및 TimeScale 제어
- ScriptableObject 기반 데이터 주도 설계 — PlayerDataSO, SkillDataSO, BossAttackPatternSO, WaveDataSO 등으로 코드 수정 없이 밸런싱 가능

### 주요 시스템
- 오브젝트 풀링 — 적, 투사체, 경험치, 히트 이펙트 전체 풀링 적용
- 스킬 시스템 — ProjectileSkill, AreaSkill, AuraSkill, OrbitSkill 계층 구조
- 보스 패턴 시스템 — Charge / AreaAttack / Projectile / Summon 4종 패턴 + 페이즈 관리
- 무한 맵 — 3x3 청크 재배치 방식
- 재화(단백질) 시스템 — CurrencyManager가 캐릭터 해금/강화 비용 처리
- 캐릭터 강화/해금 시스템 — PlayerPrefs 기반 영구 저장

### 성능 최적화
- OverlapCircleNonAlloc + Collider2D[32] 버퍼로 GC 방지
- Animator 파라미터 해시 캐싱
- 이벤트 기반 UI 갱신 (폴링 방지)
- 풀링 ON/OFF 디버그 토글 제공 — 풀링 유무에 따른 GC/성능 차이 비교 가능

## 리팩토링 예정
- MapManager 청크 오브젝트 풀링 적용
- Boss 클래스 Composition 패턴 전환 고려

## 개발 현황
핵심 시스템 위주로 구현된 포트폴리오 프로젝트입니다.
