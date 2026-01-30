using UnityEngine;

public class CollectableItem : MonoBehaviour
{
    [Header("Visuals")]
    public float rotateSpeed = 50f;
    public float floatAmplitude = 0.2f;
    public float floatSpeed = 2f;
    
    private Vector3 startOffset;
    private float startTime;

    void Start()
    {
        startOffset = transform.position;
        startTime = Time.time;
    }

    public void SetSprite(Sprite sprite)
    {
        if (sprite == null) return;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.sprite = sprite;
    }

    void Update()
    {
        // Gentle floating animation based on start position
        float newY = startOffset.y + Mathf.Sin((Time.time - startTime) * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(startOffset.x, newY, 0);
        
        // Rotation
        transform.Rotate(Vector3.forward, rotateSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Collect();
        }
    }

    public void Collect()
    {
        if (ItemSpawner.Instance != null)
        {
            ItemSpawner.Instance.OnItemCollected();
        }
        
        // Add potential audio playback here later
        Destroy(gameObject);
    }
}
