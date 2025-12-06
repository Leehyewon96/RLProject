import os
import gymnasium as gym
from mlagents_envs.environment import UnityEnvironment
from mlagents_envs.envs.unity_gym_env import UnityToGymWrapper
from stable_baselines3 import PPO
from stable_baselines3.common.callbacks import CheckpointCallback

# 유니티 빌드 파일 이름
BUILD_FOLDER = "../GameBuild"
BUILD_NAME = "SquadGame"

def main():
    # 빌드 파일 경로 찾기
    build_path = os.path.join(BUILD_FOLDER, BUILD_NAME)
    if os.path.exists(build_path + ".exe"):
        build_path += ".exe"
    elif os.path.exists(build_path + ".x86_64"):
        build_path += ".x86_64"
    
    print(f"Checking Build Path: {build_path}")
    
    # 유니티 환경 로드
    # file_name=None으로 하면 유니티 에디터의 Play 버튼과 연결
    try:
        #unity_env = UnityEnvironment(file_name=build_path, worker_id=0, no_graphics=True)
        unity_env = UnityEnvironment(file_name=None, worker_id=0, no_graphics=False)
    except Exception as e:
        print("Error: 유니티 환경을 찾을 수 없습니다. 경로를 확인하거나 유니티 에디터에서 Play를 눌러주세요.")
        print(e)
        return

    # Gym Wrapper 적용 (Unity -> Gymnasium 변환)
    # allow_multiple_obs=True: 시각(카메라) + 벡터 정보를 동시에 쓸 경우
    #env = UnityToGymWrapper(unity_env, allow_multiple_obs=False)
    env = UnityToGymWrapper(unity_env, allow_multiple_obs=False)


    # PPO 알고리즘 설정 (하이퍼파라미터)
    # MlpPolicy: 벡터 관측(Raycast 등)일 때 사용 / CnnPolicy: 화면(Camera) 관측일 때 사용
    policy_type = "MlpPolicy" 
    
    model = PPO(
        policy_type,
        env,
        verbose=1,
        learning_rate=3e-4,     # 학습률
        n_steps=2048,           # 업데이트 주기
        batch_size=64,
        gamma=0.99,
        tensorboard_log="./tensorboard_logs/"  # 보고서용 그래프 저장소
    )

    # 학습 실행
    print("학습을 시작합니다... (중단하려면 Ctrl+C)")
    
    # 중간 저장 (10,000 스텝마다 모델 저장)
    checkpoint_callback = CheckpointCallback(
        save_freq=10000, 
        save_path='./saved_models/', 
        name_prefix='rl_model'
    )

    try:
        # total_timesteps
        model.learn(total_timesteps=100000, callback=checkpoint_callback)
    except KeyboardInterrupt:
        print("학습이 강제 중단되었습니다. 현재까지의 모델을 저장합니다.")
    finally:
        # 저장 및 종료
        model.save("final_model")
        print("✅ 모델 저장 완료: final_model.zip")
        env.close()
        unity_env.close()

if __name__ == "__main__":
    main()