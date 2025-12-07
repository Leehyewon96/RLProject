import os
import gymnasium as gym
from mlagents_envs.environment import UnityEnvironment
from mlagents_envs.envs.unity_gym_env import UnityToGymWrapper
from mlagents_envs.side_channel.stats_side_channel import StatsSideChannel  # ★ 추가됨
from stable_baselines3 import PPO
from stable_baselines3.common.callbacks import BaseCallback, CheckpointCallback

# =========================================================
# [설정 영역] 유니티 빌드 파일 경로
# =========================================================
BUILD_FOLDER = "../GameBuild"   # 빌드 폴더명 (본인 경로에 맞게 수정)
BUILD_NAME = "SquadGame"        # 실행 파일 이름 (.exe 제외)
# =========================================================

class UnityStatsCallback(BaseCallback):
    """
    유니티(C#)에서 StatsRecorder로 보낸 데이터를 받아서 
    텐서보드에 'Unity/...' 그래프로 그려주는 콜백 클래스
    """
    def __init__(self, stats_channel, verbose=0):
        super().__init__(verbose)
        self.stats_channel = stats_channel

    def _on_step(self) -> bool:
        # 유니티에서 넘어온 통계 데이터 가져오기
        stats = self.stats_channel.get_and_reset_stats()
        
        # 텐서보드에 기록 (키: "AI_Decision/Action_Choice" 등)
        for key, value in stats.items():
            if isinstance(value, tuple):
                # (집계방식, 값, 횟수) 형태인 경우 값만 추출
                self.logger.record(f"Unity/{key}", value[1]) 
            else:
                self.logger.record(f"Unity/{key}", value)

        return True

def main():
    # ---------------------------------------------------------
    # 1. 빌드 파일 경로 찾기 및 환경 설정
    # ---------------------------------------------------------
    build_path = os.path.join(BUILD_FOLDER, BUILD_NAME)
    if os.path.exists(build_path + ".exe"):
        build_path += ".exe"
    elif os.path.exists(build_path + ".x86_64"):
        build_path += ".x86_64"
    
    print(f"Checking Build Path: {build_path}")

    # ★ [핵심 1] 통계 채널 생성 (이게 있어야 그래프 그려짐)
    stats_channel = StatsSideChannel()

    # ★ [핵심 2] 유니티 환경 로드 (side_channels 등록 필수!)
    try:
        unity_env = UnityEnvironment(
            file_name=build_path,       # 에디터에서 실행하려면 None으로 변경
            worker_id=0, 
            no_graphics=True,           # 그래픽 없이 빠르게 (확인용이면 False)
            side_channels=[stats_channel] 
        )
    except Exception as e:
        print("Error: 유니티 환경을 찾을 수 없습니다. 경로를 확인하거나 유니티 에디터에서 Play를 눌러주세요.")
        print(e)
        return

    # ★ [핵심 3] Wrapper 설정 (Tuple 에러 해결을 위해 False 필수!)
    env = UnityToGymWrapper(unity_env, allow_multiple_obs=False)

    # ---------------------------------------------------------
    # 2. PPO 모델 설정 (하이퍼파라미터)
    # ---------------------------------------------------------
    print("PPO 모델을 초기화합니다...")
    model = PPO(
        "MlpPolicy",
        env,
        verbose=1,
        learning_rate=3e-4,     # 학습률 (비교 실험 때 1e-2 등으로 변경)
        n_steps=2048,           # 업데이트 주기
        batch_size=64,
        gamma=0.99,             # 할인율 (미래 보상 중요도)
        tensorboard_log="./tensorboard_logs/"
    )

    # ---------------------------------------------------------
    # 3. 학습 시작 (콜백 등록)
    # ---------------------------------------------------------
    print("🚀 학습을 시작합니다... (중단하려면 Ctrl+C)")
    
    # 체크포인트 저장 (1만 스텝마다)
    checkpoint_callback = CheckpointCallback(
        save_freq=10000, 
        save_path='./saved_models/', 
        name_prefix='rl_model'
    )

    # 통계 그래프 콜백
    stats_callback = UnityStatsCallback(stats_channel)

    try:
        # 총 10만~30만 스텝 학습 권장
        model.learn(
            total_timesteps=300000, 
            callback=[checkpoint_callback, stats_callback] # 두 콜백 모두 등록
        )
    except KeyboardInterrupt:
        print("학습이 강제 중단되었습니다. 현재까지의 모델을 저장합니다.")
    finally:
        # ---------------------------------------------------------
        # 4. 저장 및 종료
        # ---------------------------------------------------------
        model.save("final_model")
        print("✅ 모델 저장 완료: final_model.zip")
        env.close()
        unity_env.close()

if __name__ == "__main__":
    main()