using UnityEngine;
using AppsInToss;
using System.Threading.Tasks;

[RequireComponent(typeof(Camera))]
public class TossCameraSafeArea : MonoBehaviour
{
    private Camera cam;
    private CameraAspectController aspectController;

    void Awake()
    {
        cam = GetComponent<Camera>();
        aspectController = GetComponent<CameraAspectController>();
    }

    async void Start()
    {
        await ApplyCameraSafeArea();
    }

    public async Task ApplyCameraSafeArea()
    {
        try
        {
            // 1. 토스 SDK에서 Safe Area 정보 가져오기
            var insets = await AIT.SafeAreaInsetsGet();
            
            // 2. 화면 전체 사이즈 (픽셀)
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            if (insets != null)
            {
                // CSS 픽셀 단위를 Device 픽셀로 변환 (Unity 좌표계에 맞게)
                double dpr = AIT.GetDevicePixelRatio();
                float top = (float)(insets.Top * dpr);
                float bottom = (float)(insets.Bottom * dpr);
                float left = (float)(insets.Left * dpr);
                float right = (float)(insets.Right * dpr);

                // 3. 카메라가 렌더링할 Rect 계산 (0~1 사이 값)
                // 가려지는 영역만큼 카메라가 화면 안쪽으로 그려지게 함
                Rect safeRect = new Rect(
                    left / screenWidth,
                    bottom / screenHeight,
                    (screenWidth - left - right) / screenWidth,
                    (screenHeight - top - bottom) / screenHeight
                );

                cam.rect = safeRect;

                // 4. Safe Area가 적용된 후, 벽 위치 계산을 다시 수행하도록 유도
                // (SpikeSpawner 등에서 카메라 사이즈를 참조하므로 중요)
                Debug.Log($"[Toss SDK] Camera Viewport Adjusted to Safe Area: {safeRect}");
                
                // AspectController가 있다면 다시 한 번 호출해서 정합성 유지
                if (aspectController != null)
                {
                    aspectController.AdjustCamera();
                }
            }
        }
        catch (AITException ex)
        {
            Debug.LogError($"[Toss SDK] 카메라 Safe Area 적용 실패: {ex.Message}");
        }
    }
}
