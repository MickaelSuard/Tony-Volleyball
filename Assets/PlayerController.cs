using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float speed = 8f;
    public float jumpForce = 12f;

    public Key left;
    public Key right;
    public Key jump;

    Rigidbody2D rb;
    SpriteRenderer sr;

    bool grounded;

    Vector3 originalScale;
    Vector3 targetScale;

    public float animationSpeed = 6f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    void Update()
    {
        float move = 0;

        if (Keyboard.current[left].isPressed)
            move = -1;

        if (Keyboard.current[right].isPressed)
            move = 1;

        rb.linearVelocity = new Vector2(move * speed, rb.linearVelocity.y);

        // tourner le personnage
        if (move < 0)
            sr.flipX = true;

        if (move > 0)
            sr.flipX = false;

        // saut
        if (Keyboard.current[jump].wasPressedThisFrame && grounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

            grounded = false;

            // petite animation de saut
            targetScale = new Vector3(originalScale.x * 0.95f, originalScale.y * 1.05f, 1);
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.name == "Ground")
        {
            grounded = true;

            // petite compression à l'atterrissage
            targetScale = new Vector3(originalScale.x * 1.05f, originalScale.y * 0.95f, 1);

            Invoke(nameof(ResetScale), 0.15f);
        }
    }

    void ResetScale()
    {
        targetScale = originalScale;
    }
}