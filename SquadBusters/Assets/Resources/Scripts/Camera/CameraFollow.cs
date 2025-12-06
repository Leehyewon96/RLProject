using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private GameObject target = null; // 로컬 플레이어 찾아서 넣는 코드로 변경 필요
    [SerializeField] private float delayTime = 30f;

    [SerializeField] private float m_roughness = 50f; //거칠기 정도
    [SerializeField] private float m_magnitude = 2f; //움직임 범위
    private bool cameraShake = false;

    public delegate void CameraShake(float duration);
    public CameraShake onCameraShake = null;

    private Camera cam = null;

    private Vector3 fixedPos = Vector3.zero;
    private Vector3 offsetVec = Vector3.zero;

    [Header("카메라 속성")]
    [Tooltip("타겟과의 시작 거리")]
    public float distance = 10.0f;
    [Tooltip("카메라 회전 속도")]
    public float rotationSpeed = 3.0f;
    [Tooltip("카메라 줌 속도")]
    public float zoomSpeed = 5.0f;

    [Header("거리 제한")]
    [Tooltip("가장 가까워질 수 있는 거리")]
    public float minDistance = 2.0f;
    [Tooltip("가장 멀어질 수 있는 거리")]
    public float maxDistance = 20.0f;

    [Header("각도 제한")]
    [Tooltip("카메라의 최소 수직 각도 (아래에서 위로 보는 각도)")]
    public float minYAngle = -20.0f;
    [Tooltip("카메라의 최대 수직 각도 (위에서 아래로 보는 각도)")]
    public float maxYAngle = 80.0f;

    // 현재 카메라의 회전 각도
    private float xAngle = 0.0f;
    private float yAngle = 0.0f;


    private void Start()
    {
        cam = Camera.main;
        onCameraShake += ShakeCamera;

        Vector3 angles = transform.eulerAngles;
        xAngle = angles.y;
        yAngle = angles.x;
    }

    /*private void Update()
    {
        if(target == null)
        {
            return;
        }

        if (cameraShake)
        {
            return;
        }

        FollowTarget();

    }*/

    void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        if (cameraShake)
        {
            return;
        }

        // 마우스 우클릭 드래그로 회전 각도 계산
        if (Input.GetMouseButton(1))
        {
            xAngle += Input.GetAxis("Mouse X") * rotationSpeed;
            yAngle -= Input.GetAxis("Mouse Y") * rotationSpeed;

            // yAngle(수직 각도)의 범위를 제한하여 카메라가 땅 밑으로 파고들거나 하늘 위로 뒤집히는 것을 방지
            yAngle = ClampAngle(yAngle, minYAngle, maxYAngle);
        }

        // 마우스 스크롤 휠로 거리 계산
        distance -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        // 계산된 각도와 거리를 기반으로 카메라의 위치와 회전을 설정
        Quaternion rotation = Quaternion.Euler(yAngle, xAngle, 0);
        Vector3 position = rotation * new Vector3(0, 0, -distance) + target.transform.position;

        cam.transform.rotation = rotation;
        cam.transform.position = position;
    }

    /// <summary>
    /// 각도가 최소값과 최대값 사이에 있도록 제한하는 함수
    /// </summary>
    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360) angle += 360;
        if (angle > 360) angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }

    private void FollowTarget()
    {
        fixedPos = offsetVec + target.transform.position;
        cam.transform.position = Vector3.Lerp(cam.transform.position, fixedPos, Time.deltaTime * delayTime);
    }

    public void SetTarget(GameObject inTarget)
    {
        target = inTarget;
        //offsetVec = cam.transform.position - target.transform.position;
        offsetVec = new Vector3(0, 9.3f, -7.43f);
    }

    private void ShakeCamera(float duration)
    {
        if(cameraShake)
        {
            return;
        }
        Debug.Log("cameraShake");
        cameraShake = true;
        StartCoroutine(Shake(duration, transform.position));
    }


    private IEnumerator Shake(float duration, Vector3 originPos)
    {
        float halfDuration = duration / 2;
        float elapsed = 0f;
        float tick = Random.Range(-10f, 10f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime / halfDuration;

            tick += Time.deltaTime * m_roughness;
            transform.position = originPos +  new Vector3(
                Mathf.PerlinNoise(tick, 0) - .5f,
                Mathf.PerlinNoise(0, tick) - .5f,
                Mathf.PerlinNoise(0, tick) - .5f) * m_magnitude * Mathf.PingPong(elapsed, halfDuration);

            yield return null;
        }

        cameraShake = false;
    }
}
