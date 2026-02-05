using UnityEngine;
using AppsInToss; // 공식 SDK 네임스페이스
using System.Threading.Tasks;

public class TossManager : MonoBehaviour
{
    public static TossManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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
