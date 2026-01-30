using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public static ItemSpawner Instance;

    [Header("Settings")]
    public GameObject itemPrefab;
    public Vector2 xRange = new Vector2(-1.5f, 1.5f);
    public Vector2 yRange = new Vector2(-3.5f, 3.5f);
    
    private GameObject currentItem;
    private BirdController bird;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        bird = FindObjectOfType<BirdController>();
        // SpawnInitialItem(); // 초기 생성은 GameManager에서 게임 시작 시 호출
    }

    public void SpawnInitialItem()
    {
        if (currentItem != null) return;
        SpawnAtRandom(0); 
    }

    public void OnItemCollected()
    {
        currentItem = null;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddBerry();
        }

        if (bird != null)
        {
            int side = bird.GetCurrentFacingDir(); 
            SpawnAtRandom(side);
        }
        else
        {
            SpawnAtRandom(0);
        }
    }

    private void SpawnAtRandom(int side)
    {
        float xPos;
        if (side == 1) // Heading Right, spawn in right area
            xPos = Random.Range(0.5f, xRange.y);
        else if (side == -1) // Heading Left, spawn in left area
            xPos = Random.Range(xRange.x, -0.5f);
        else // Middle
            xPos = Random.Range(xRange.x, xRange.y);

        float yPos = Random.Range(yRange.x, yRange.y);
        Vector3 spawnPos = new Vector3(xPos, yPos, 0);

        if (itemPrefab != null)
        {
            currentItem = Instantiate(itemPrefab, spawnPos, Quaternion.identity);
            
            // Set Stage Berry Sprite
            if (GameManager.Instance != null)
            {
                Sprite s = GameManager.Instance.GetCurrentBerrySprite();
                CollectableItem itemScript = currentItem.GetComponent<CollectableItem>();
                if (itemScript != null && s != null)
                {
                    itemScript.SetSprite(s);
                }
            }
        }
    }
}
