using UnityEngine;

public class DuckPatrol : MonoBehaviour
{
    public Sprite[] walkFrames;
    public float speed = 1.5f;
    public float leftX = -6.5f;
    public float rightX = 6.5f;
    public float frameRate = 10f;

    private SpriteRenderer spriteRenderer;
    private int direction = 1;
    private int frameIndex = 0;
    private float frameTimer = 0f;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (walkFrames != null && walkFrames.Length > 0)
        {
            spriteRenderer.sprite = walkFrames[0];
        }
    }

    void Update()
    {
        Move();
        Animate();
    }

    private void Move()
    {
        Vector3 pos = transform.position;
        pos.x += direction * speed * Time.deltaTime;

        if (pos.x >= rightX)
        {
            pos.x = rightX;
            direction = -1;
        }
        else if (pos.x <= leftX)
        {
            pos.x = leftX;
            direction = 1;
        }

        transform.position = pos;
        spriteRenderer.flipX = direction < 0;
    }

    private void Animate()
    {
        if (walkFrames == null || walkFrames.Length == 0) return;

        frameTimer += Time.deltaTime;
        if (frameTimer < 1f / frameRate) return;

        frameTimer = 0f;
        frameIndex = (frameIndex + 1) % walkFrames.Length;
        spriteRenderer.sprite = walkFrames[frameIndex];
    }
}
