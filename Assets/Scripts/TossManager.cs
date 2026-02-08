using UnityEngine;
using AppsInToss; // 공식 SDK 네임스페이스
using System.Threading.Tasks;

public class TossManager : MonoBehaviour
{
    public static TossManager Instance;

    private async void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Toss 게임 최적화 설정
        try 
        {
            // 1. iOS 스와이프 뒤로가기 제스처 비활성화 (게임 중 실수방지)
            await AIT.SetIosSwipeGestureEnabled(new SetIosSwipeGestureEnabledOptions { IsEnabled = false });
            
            // 2. 화면 항상 켜짐 모드 활성화
            await AIT.SetScreenAwakeMode(new SetScreenAwakeModeOptions { Enabled = true });
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[Toss SDK] 초기 설정 적용 실패: {ex.Message}");
        }
    }

    /// <summary>
    /// 게임 로그인을 수행합니다. 사용자의 동의 절차 없이 식별자를 가져옵니다.
    /// </summary>
    public async Task<string> Login()
    {
        try 
        {
            // 1. 사용자 식별 키(유저별 고유 해시) 가져오기
            var keyResult = await AIT.GetUserKeyForGame();
            
            if (keyResult.IsSuccess) 
            {
                var hash = keyResult.GetSuccess().Hash;
                Debug.Log($"[Toss SDK] 게임 로그인 성공 (UserKey: {hash})");
                
                // 2. 선택사항: 게임 센터 프로필(닉네임, 이미지) 가져오기
                await FetchGameProfile();
                
                return hash;
            }
            else 
            {
                Debug.LogWarning($"[Toss SDK] 게임 로그인 실패: {keyResult.GetErrorCode()}");
                return null;
            }
        }
        catch (AITException ex) 
        {
            Debug.LogError($"[Toss SDK] 게임 로그인 중 오류: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 사용자의 토스 게임 센터 프로필을 가져옵니다.
    /// </summary>
    public async Task FetchGameProfile()
    {
        try 
        {
            var profile = await AIT.GetGameCenterGameProfile();
            if (profile != null && profile.StatusCode == "SUCCESS" && !string.IsNullOrEmpty(profile.Nickname)) 
            {
                Debug.Log($"[Toss SDK] 프로필 조회 성공: {profile.Nickname}");
                // GameManager에 닉네임 전달
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SetNickname(profile.Nickname);
                }
            }
        }
        catch (AITException ex) 
        {
            Debug.LogError($"[Toss SDK] 프로필 조회 실패: {ex.Message}");
        }
    }

    /// <summary>
    /// 토스 게임센터 리더보드에 점수를 제출합니다.
    /// </summary>
    public async void ReportScore(int score)
    {
        try 
        {
            // SDK 공식 API: 점수를 문자열로 전송
            var result = await AIT.SubmitGameCenterLeaderBoardScore(new SubmitGameCenterLeaderBoardScoreParams {
                Score = score.ToString()
            });

            if (result != null && result.StatusCode == "SUCCESS") {
                Debug.Log($"[Toss SDK] 점수 제출 성공: {score}");
            } else {
                Debug.LogWarning($"[Toss SDK] 점수 제출 실패: {result?.StatusCode}");
            }
        }
        catch (AITException ex) 
        {
            Debug.LogError($"[Toss SDK] 점수 제출 중 오류: {ex.Message} (Code: {ex.ErrorCode})");
        }
    }

    /// <summary>
    /// 토스 게임센터 리더보드 웹뷰를 엽니다.
    /// </summary>
    public async void ShowLeaderboard()
    {
        try 
        {
            Debug.Log("[Toss SDK] 리더보드 웹뷰 호출 성공");
            await AIT.OpenGameCenterLeaderboard();
           
        }
        catch (AITException ex) 
        {
            Debug.LogError($"[Toss SDK] 리더보드 호출 실패: {ex.Message}");
        }
    }

    // 공식 SDK에서는 닉네임을 토스 앱에서 관리하므로 별도 동기화가 필요 없을 수 있으나,
    // 필요 시 아래와 같이 추가 API를 활용할 수 있습니다.
    public void SyncNickname(string nickname)
    {
        Debug.Log($"[Toss SDK] {nickname}님 환영합니다! (토스 프로필 연동 권장)");
    }
}
