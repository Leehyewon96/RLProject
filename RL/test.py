import os
import gymnasium as gym
from mlagents_envs.environment import UnityEnvironment
from mlagents_envs.envs.unity_gym_env import UnityToGymWrapper
from stable_baselines3 import PPO

BUILD_FOLDER = "../unity_build"
BUILD_NAME = "SquadGame"   # train.py와 동일하게 설정
MODEL_PATH = "final_model" # 불러올 모델 파일 이름 (.zip 제외)

def main():
    build_path = os.path.join(BUILD_FOLDER, BUILD_NAME)
    if os.path.exists(build_path + ".exe"):
        build_path += ".exe"

    # 테스트 할 때는 그래픽이 보여야 하므로 no_graphics=False
    unity_env = UnityEnvironment(file_name=build_path, worker_id=1, no_graphics=False)
    env = UnityToGymWrapper(unity_env, allow_multiple_obs=False)

    # 모델 로드
    if not os.path.exists(MODEL_PATH + ".zip"):
        print(f"❌ 모델 파일({MODEL_PATH}.zip)이 없습니다. train.py를 먼저 실행하세요.")
        return

    model = PPO.load(MODEL_PATH)
    print("✅ 모델 로드 완료. 테스트를 시작합니다.")

    # 게임 루프
    obs, _ = env.reset()
    for i in range(10000): # 10000 프레임 동안 실행
        action, _states = model.predict(obs, deterministic=True)
        obs, reward, terminated, truncated, info = env.step(action)
        
        if terminated or truncated:
            obs, _ = env.reset()

    env.close()
    unity_env.close()

if __name__ == "__main__":
    main()