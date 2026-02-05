using UnityEngine;
using System.Collections.Generic;

public class SpikeSpawner : MonoBehaviour
{
    [Header("Basic Settings")]
    [Tooltip("가시로 사용할 프리팹")]
    public GameObject spikePrefab; 
    [Tooltip("최대 가시 생성 개수 (오브젝트 풀링용)")]
    public int poolSize = 60;
    
    [Header("Layout Positions")]
    [Tooltip("좌우 벽의 X 좌표")]
    public float sideWallX; 
    [Tooltip("상하 벽의 Y 좌표")]
    public float topBottomYHeight = 5.2f; 
    
    [Header("Spike Spacing (간격)")]
    [Tooltip("상하 정적 가시 사이의 좌우 간격")]
    public float staticSpikeHorizontalGap = 0.6f;
    [Tooltip("옆면 랜덤 가시 사이의 수직 간격 (최소 거리)")]
    public float sideSpikeVerticalGap = 0.5f;
    [Tooltip("새가 지나갈 수 있는 안전한 틈새의 크기")]
    public float safeGapSize = 1.6f; 

    [Header("Spike Offset (밀착도)")]
    [Tooltip("옆면 가시가 벽 밖으로 얼마나 튀어나올지 (작을수록 벽에 밀착)")]
    public float sideSpikeWallOffset = 0.4f; 
    [Tooltip("상하 가시가 벽 밖으로 얼마나 튀어나올지 (작을수록 벽에 밀착)")]
    public float staticSpikeWallOffset = 0.2f; 
    
    private List<GameObject> spikePool = new List<GameObject>();

    void Start()
    {
        transform.position = Vector3.zero;
        CalculateWallPositions();

        if (spikePrefab == null) return;

        InitializePool();
        SpawnStaticSpikes();
    }

    void CalculateWallPositions()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            float screenAspect = (float)Screen.width / Screen.height;
            float camHeight = cam.orthographicSize * 2;
            float camWidth = camHeight * screenAspect;
            sideWallX = (camWidth / 2f);
            topBottomYHeight = cam.orthographicSize;
        }

        GameObject rw = GameObject.Find("Right Wall");
        if (rw != null) sideWallX = Mathf.Abs(rw.transform.position.x);
        
        GameObject tw = GameObject.Find("Top Wall");
        if (tw != null) topBottomYHeight = Mathf.Abs(tw.transform.position.y);
    }

    void InitializePool()
    {
        foreach(var s in spikePool) if(s != null) Destroy(s);
        spikePool.Clear();

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(spikePrefab);
            obj.transform.SetParent(this.transform);
            
            var sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null) 
            {
                sr.sortingLayerName = "Default";
                sr.sortingOrder = 10;
            }

            obj.SetActive(false);
            spikePool.Add(obj);
        }
    }

    void SpawnStaticSpikes()
    {
        float totalWidth = sideWallX * 2f;
        int count = Mathf.FloorToInt(totalWidth / staticSpikeHorizontalGap);
        float actualSpikesWidth = count * staticSpikeHorizontalGap;
        float startX = -sideWallX + (totalWidth - actualSpikesWidth) / 2f;
        
        for (int i = 0; i <= count; i++) {
            float x = startX + (i * staticSpikeHorizontalGap);
            ActivateSpike(new Vector3(x, topBottomYHeight - staticSpikeWallOffset, 0), 180f);
            ActivateSpike(new Vector3(x, -topBottomYHeight + staticSpikeWallOffset, 0), 0f);
        }
    }

    public void SpawnPattern(bool onRightWall)
    {
        DeactivateExistingSideSpikes();

        float xPos = onRightWall ? sideWallX - sideSpikeWallOffset : -sideWallX + sideSpikeWallOffset;
        float rotZ = onRightWall ? 90f : -90f;

        SpawnSideSpikes(xPos, rotZ);
    }

    void DeactivateExistingSideSpikes()
    {
        foreach(var s in spikePool) 
        {
            if (!s.activeSelf) continue;

            float targetTopY = topBottomYHeight - staticSpikeWallOffset;
            float targetBottomY = -topBottomYHeight + staticSpikeWallOffset;

            float distToTop = Mathf.Abs(s.transform.position.y - targetTopY);
            float distToBottom = Mathf.Abs(s.transform.position.y - targetBottomY);

            if (distToTop > 0.05f && distToBottom > 0.05f)
            {
                s.SetActive(false);
            }
        }
    }

    void SpawnSideSpikes(float xPos, float rotZ)
    {
        // 1. 가시가 생성될 수 있는 모든 Y 좌표 리스트 생성
        List<float> possibleY = new List<float>();
        float startY = -topBottomYHeight + 1.2f;
        float endY = topBottomYHeight - 1.2f;

        for (float y = startY; y <= endY; y += sideSpikeVerticalGap) 
        {
            possibleY.Add(y);
        }

        if (possibleY.Count == 0) return;

        // 2. 안전 구역(새가 지나갈 구멍) 정하기
        // 전체 구간 중 임의의 위치를 안전 구역 중심으로 잡음
        float safeZoneCenter = Random.Range(startY + (safeGapSize/2f), endY - (safeGapSize/2f));
        
        // 3. 안전 구역 내에 포함되는 좌표들은 후보에서 제외
        possibleY.RemoveAll(y => Mathf.Abs(y - safeZoneCenter) < (safeGapSize / 2f));

        // 4. 나머지 후보 중에서 랜덤하게 가시 생성
        int spikeCount = Random.Range(4, 7); // 난이도 조절: 4~7개 생성
        spikeCount = Mathf.Min(spikeCount, possibleY.Count);

        for (int i = 0; i < spikeCount; i++)
        {
            int randomIndex = Random.Range(0, possibleY.Count);
            float spawnY = possibleY[randomIndex];
            possibleY.RemoveAt(randomIndex);
            
            ActivateSpike(new Vector3(xPos, spawnY, 0), rotZ);
        }
    }

    void ActivateSpike(Vector3 pos, float rotZ)
    {
        foreach(var s in spikePool) {
            if (!s.activeSelf) {
                s.transform.position = new Vector3(pos.x, pos.y, 0f);
                s.transform.rotation = Quaternion.Euler(0, 0, rotZ);
                s.SetActive(true);
                return;
            }
        }
    }
}