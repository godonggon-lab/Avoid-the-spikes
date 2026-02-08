using UnityEngine;
using AppsInToss;
using System.Threading.Tasks;

public class TossSafeAreaAdjuster : MonoBehaviour
{
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    async void Start()
    {
        await ApplySafeArea();
    }

    public async Task ApplySafeArea()
    {
        try
        {
            // Toss SDK에서 Safe Area Insets 가져오기
            var insets = await AIT.SafeAreaInsetsGet();
            double dpr = AIT.GetDevicePixelRatio();

            if (insets != null)
            {
                // CSS 픽셀을 Unity Device 픽셀로 변환
                float top = (float)(insets.Top * dpr);
                float bottom = (float)(insets.Bottom * dpr);
                float left = (float)(insets.Left * dpr);
                float right = (float)(insets.Right * dpr);

                // RectTransform의 offset을 사용하여 패딩 적용
                // 주의: Anchor가 Full Stretch (0,0 to 1,1)인 경우에 최적임
                rectTransform.offsetMax = new Vector2(-right, -top);
                rectTransform.offsetMin = new Vector2(left, bottom);

                Debug.Log($"[Toss SDK] Safe Area Applied: T:{top}, B:{bottom}, L:{left}, R:{right}");
            }
        }
        catch (AITException ex)
        {
            Debug.LogError($"[Toss SDK] Safe Area 적용 실패: {ex.Message}");
        }
    }
}
