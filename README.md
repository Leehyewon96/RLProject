# RLProject

# Squad Busters AI Agent Project

**Unity ML-Agents와 강화학습(PPO)을 활용한 스쿼드 전투 AI 에이전트 개발 프로젝트**

이 프로젝트는 '스쿼드 버스터즈' 스타일의 게임 환경에서 다수의 유닛(스쿼드)이 적과 전투하고 아이템을 획득하며 생존하는 AI를 학습시키는 것을 목표로 합니다.

## 1. 팀원 정보 (Team Info)
* **이름**: [이혜원]
* **학번**: [A72077]
* **전공**: [데이터사이언스인공지능]

---

## 2. 주요 자료 다운로드 (Downloads)
**과제 제출 요건에 따라 학습된 모델 파일과 결과 보고서를 첨부합니다.**

* **[학습된 모델 다운로드 (Trained Model)](./final_model.zip)**
    * 위 링크를 클릭하면 PPO로 학습된 신경망 모델 파일(.zip)을 다운로드할 수 있습니다.
* **[프로젝트 결과 보고서 (Project Report)](./이혜원_AI활용보고서.pdf)**
    * 설계 내용, 구현 방법, 실험 결과 분석이 포함된 상세 보고서입니다.

---

## 3. 개발 환경 (Environment)
* **Engine**: Unity 2022.3.x (ML-Agents Package 2.0.1)
* **Language**: C#, Python 3.10
* **Library**:
    * mlagents-envs
    * stable-baselines3 (PPO Algorithm)
    * shimmy (Gymnasium Compatibility)
    * gymnasium

---

## 4. 프로젝트 실행 방법 (How to Run)

### 1) 유니티 환경 설정
1. 이 저장소(RLProject)를 클론(Clone)하거나 다운로드합니다.
2. Unity Hub를 통해 프로젝트 폴더를 엽니다.
3. Assets/Scenes/GameScene(또는 작업하신 씬 이름)을 엽니다.

### 2) 파이썬 가상환경 및 라이브러리 설치
터미널에서 다음 명령어를 입력하여 필수 라이브러리를 설치합니다.
```bash
pip install mlagents stable-baselines3 shimmy gymnasium
