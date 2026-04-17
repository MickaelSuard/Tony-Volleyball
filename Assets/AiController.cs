using UnityEngine;

public class AIController : MonoBehaviour
{
    public float speed = 9f; // Augmenté pour être "imbattable"
    public float jumpForce = 12f;
    public Transform ball;
    private Rigidbody2D ballRb;

    [Header("Zone IA")]
    public float minX = 0.5f;
    public float maxX = 8f;

    [Header("Position repos")]
    public float centerX = 4f;

    [Header("Comportement")]
    public float followOffset = 0.2f; // Plus précis
    public float jumpCooldown = 0.3f;

    private Rigidbody2D rb;
    private bool grounded;
    private float jumpTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (ball != null) ballRb = ball.GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (ball == null) return;
        jumpTimer -= Time.deltaTime;

        float move = 0;
        bool ballOnAISide = ball.position.x > 0;
        float targetX;

        if (!ballOnAISide)
        {
            targetX = centerX;
        }
        else
        {
            // --- LE SECRET DE L'IA IMBATTABLE : L'ANTICIPATION ---
            // Si la balle bouge (pas au service), on prédit où elle va tomber
            if (Mathf.Abs(ballRb.linearVelocity.x) > 0.1f)
            {
                // On calcule le temps avant que la balle n'atteigne la raquette de l'IA
                float timeToReach = Mathf.Abs((ball.position.y - transform.position.y) / (ballRb.linearVelocity.y - 0.1f));
                targetX = ball.position.x + (ballRb.linearVelocity.x * timeToReach);
            }
            else
            {
                // AU SERVICE : On garde strictement ta logique (coller à la position X de la balle)
                targetX = ball.position.x;
            }

            targetX = Mathf.Clamp(targetX, minX, maxX);

            // Déplacement
            if (targetX < transform.position.x - followOffset) move = -1;
            else if (targetX > transform.position.x + followOffset) move = 1;

            rb.linearVelocity = new Vector2(move * speed, rb.linearVelocity.y);

            // --- SAUT ---
            bool ballAbove = ball.position.y > transform.position.y + 1f;
            bool ballCloseX = Mathf.Abs(ball.position.x - transform.position.x) < 2f;

            // On ajoute une vérification : l'IA ne saute que si la balle ne monte pas trop vite
            if (grounded && jumpTimer <= 0 && ballAbove && ballCloseX && ballRb.linearVelocity.y < 2f)
            {
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                grounded = false;
                jumpTimer = jumpCooldown;
            }
        }

        // Retour au centre si balle côté adverse
        if (!ballOnAISide)
        {
            if (targetX < transform.position.x - followOffset) move = -1;
            else if (targetX > transform.position.x + followOffset) move = 1;
            rb.linearVelocity = new Vector2(move * speed, rb.linearVelocity.y);
        }
    }

    void LateUpdate()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        transform.position = pos;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        // Plus flexible si le sol s'appelle "Ground" ou a le tag
        if (col.gameObject.CompareTag("GroundTrigger"))
        {
            grounded = true;
        }
    }
}