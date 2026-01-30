using UnityEngine;
using System.Collections;

public class BirdController : MonoBehaviour
{
    [Header("Base Settings")]
    public float baseJumpForce = 8f;
    public float baseSideJumpForce = 7f;
    public float speedIncreasePerStage = 0.1f; 
    
    public float gravityScale = 3f;
    public float horizontalDamping = 1.0f;

    [Header("Sprites")]
    public Sprite idleSprite;
    public Sprite flapSprite;
    public Sprite deadSprite; // 기존 단일 스프라이트 (Fallback용)
    
    [Header("Animation Settings")]
    public Sprite[] deadAnimationSprites; // 드래그 앤 드롭으로 여러장 넣으세요
    public float deadAnimDelay = 0.1f;    // 프레임 간 시간 간격

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private bool isDead = false;
    private bool hasStarted = false;
    private int currentFacingDir = -1;

    void Start()
    {
        currentFacingDir = -1;
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
        
        rb.gravityScale = 0; // 시작 전에는 중력 없음 (동동 떠 있음)
        rb.velocity = Vector2.zero;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        
        gameObject.tag = "Player";
        if (idleSprite != null) sr.sprite = idleSprite;
        FlipSprite(currentFacingDir);
    }

    void Update()
    {
        if (isDead) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (!hasStarted)
            {
                hasStarted = true;
                rb.gravityScale = gravityScale;
                // 최초 탭 시 GameManager에도 시작 알림
                if (GameManager.Instance != null) GameManager.Instance.StartGameFromTitle();
            }
            Jump();
        }

        // 공중에 떠 있을 때 부드럽게 위아래로 움직이는 효과 (시작 전)
        if (!hasStarted)
        {
            float y = Mathf.Sin(Time.time * 3f) * 0.1f;
            transform.position = new Vector3(transform.position.x, y, transform.position.z);
        }

        if (hasStarted && rb.velocity.y < 0)
        {
            if (idleSprite != null) sr.sprite = idleSprite;
        }

        sr.color = Color.white; // 색상 고정 (쉴드 연출 제거)
    }

    void FixedUpdate()
    {
        if (isDead || !hasStarted) return;

        Vector2 vel = rb.velocity;
        if (Mathf.Abs(vel.x) > 0.01f)
        {
            vel.x = Mathf.MoveTowards(vel.x, 0, horizontalDamping * Time.fixedDeltaTime * 5f);
        }
        rb.velocity = vel;
    }

    void Jump()
    {
        float stageMultiplier = 1f;
        if (GameManager.Instance != null)
        {
            stageMultiplier = 1f + ((GameManager.Instance.currentStage - 1) * speedIncreasePerStage);
        }

        rb.velocity = new Vector2(currentFacingDir * baseSideJumpForce * stageMultiplier, baseJumpForce * stageMultiplier);
        FlipSprite(currentFacingDir);
        
        if (flapSprite != null) sr.sprite = flapSprite;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead || !hasStarted) return;

        string objName = collision.gameObject.name.ToLower();
        bool isWall = collision.gameObject.CompareTag("Wall") || objName.Contains("wall");

        if (isWall)
        {
            bool hitRightWall = collision.contacts[0].point.x > 0;
            currentFacingDir = hitRightWall ? -1 : 1;
            
            float stageMultiplier = 1f;
            if (GameManager.Instance != null)
                stageMultiplier = 1f + ((GameManager.Instance.currentStage - 1) * speedIncreasePerStage);

            rb.velocity = new Vector2(currentFacingDir * (baseSideJumpForce * 0.7f) * stageMultiplier, rb.velocity.y + 1f);
            FlipSprite(currentFacingDir);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore();
                GameManager.Instance.SpawnSpikes(currentFacingDir == 1);
            }
        }
        else if (collision.gameObject.CompareTag("Spike") || objName.Contains("spike") || objName.Contains("bottom"))
        {
            Die(); // 쉴드 체크 없이 즉시 사망
        }
    }

    public int GetCurrentFacingDir() { return currentFacingDir; }

    void FlipSprite(int dir)
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * dir; // 유저 수정 사항 반영
        transform.localScale = scale;
    }


    void Die()
    {
        if (isDead) return;
        isDead = true;
        rb.velocity = Vector2.zero;

        // 우선 애니메이션 재생 시도, 없으면 단일 스프라이트 사용
        if (deadAnimationSprites != null && deadAnimationSprites.Length > 0)
        {
            StartCoroutine(PlayDeadAnimCoroutine());
        }
        else if (deadSprite != null) 
        {
            sr.sprite = deadSprite;
        }

        if (GameManager.Instance != null) GameManager.Instance.GameOver();
    }

    IEnumerator PlayDeadAnimCoroutine()
    {
        // Loop through all frames once (or loop if needed, here it plays once)
        for (int i = 0; i < deadAnimationSprites.Length; i++)
        {
            sr.sprite = deadAnimationSprites[i];
            yield return new WaitForSeconds(deadAnimDelay);
        }
    }
}