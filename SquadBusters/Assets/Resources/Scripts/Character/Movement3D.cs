using UnityEngine;

public class Movement3D : MonoBehaviour
{
    [HideInInspector] public CharacterController characterController = null;

    public float moveSpeed { get; private set; } = 7.5f;
    protected Vector3 moveDirection = Vector3.zero;
    Transform cameraTransform;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        cameraTransform = Camera.main.transform;
    }

    public void Move(float x, float z)
    {
        if(x == 0 && z == 0)
        {
            return;
        }

        //moveDirection = new Vector3(x, 0, z);

        //characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
        //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDirection), 0.1f);

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize(); // 벡터의 길이를 1로 만들어 속도 일관성 유지
        camRight.Normalize();

        //입력값과 카메라 방향을 조합하여 최종 이동 방향 계산
        moveDirection = (camForward * z) + (camRight * x);

        // 캐릭터 이동
        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);

        // 이동 방향이 있을 때만 캐릭터 회전
        if (moveDirection != Vector3.zero)
        {
            // Quaternion.LookRotation을 사용하여 목표 방향을 바라보는 회전값 생성
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

            // Quaternion.Slerp를 사용하여 현재 회전에서 목표 회전으로 부드럽게 전환
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.1f);
        }
    }

    public void UpdateMoveSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }
}
