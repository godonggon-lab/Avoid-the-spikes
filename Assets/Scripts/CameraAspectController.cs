using UnityEngine;

public class CameraAspectController : MonoBehaviour
{
    [Header("Design Settings")]
    public float targetWidth = 9f;
    public float targetHeight = 16f;

    [Header("Individual View Control")]
    [Tooltip("위쪽 시야를 추가로 확장합니다.")]
    public float topStretch = 0f;
    [Tooltip("아래쪽 시야를 추가로 확장합니다.")]
    public float bottomStretch = 0f;

    private Camera cam;
    private float initialSize;

    void Awake()
    {
        cam = GetComponent<Camera>();
        initialSize = cam.orthographicSize;
        AdjustCamera();
    }

    void Update()
    {
#if UNITY_EDITOR
        AdjustCamera();
#endif
    }

    public void AdjustCamera()
    {
        if (cam == null) return;

        float targetAspect = targetWidth / targetHeight;
        float currentAspect = (float)Screen.width / Screen.height;

        // 1. 기본 화면 비율에 따른 기본 사이즈 계산
        float baseSize = initialSize;
        if (currentAspect < targetAspect)
        {
            float multiplier = targetAspect / currentAspect;
            baseSize = initialSize * multiplier;
        }

        // 2. 위/아래 개별 수치 적용
        // 전체 높이 = (기본 높이) + 위쪽 확장 + 아래쪽 확장
        float totalHeight = (baseSize * 2f) + topStretch + bottomStretch;
        
        // 새로운 Orthographic Size (절반 높이)
        cam.orthographicSize = totalHeight / 2f;

        // 새로운 Y 위치 (위쪽이 더 크면 카메라가 위로, 아래쪽이 더 크면 아래로 이동)
        float newY = (topStretch - bottomStretch) / 2f;
        transform.position = new Vector3(0, newY, -10f);
    }
}
