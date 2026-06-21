using UnityEngine;

public class BallController : MonoBehaviour
{
    public GameManager gameManager;
    public GameObject groundObj;

    private Rigidbody2D rb;
    private bool firstHit = false;

    public float maxSpeed = 18f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        ResetBall();
    }

    void FixedUpdate()
    {
        if (rb.bodyType == RigidbodyType2D.Dynamic)
        {
            // rotation selon vitesse
            rb.angularVelocity = -rb.linearVelocity.x * 40f;

            // limiter vitesse pour garder le contrôle
            if (rb.linearVelocity.magnitude > maxSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject == gameManager.player1Obj || collision.gameObject == gameManager.player2Obj)
        {
            if (!firstHit)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                firstHit = true;
            }

            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();

            Vector2 contact = collision.contacts[0].point;
            Vector2 dir = ((Vector2)transform.position - contact).normalized;
            if (Mathf.Abs(dir.x) < 0.2f)
            {
                float side = collision.transform.position.x < 0 ? 1 : -1;
                // player gauche → envoie à droite
                // player droite → envoie à gauche

                dir.x = side * 0.5f;
                dir.y = Mathf.Abs(dir.y);

                dir = dir.normalized;
            }

            float power = gameManager.ballForce + playerRb.linearVelocity.magnitude * 0.3f;

            rb.linearVelocity = Vector2.zero;
            rb.AddForce(dir * power, ForceMode2D.Impulse);
        }

        if (collision.gameObject == groundObj)
        {
            gameManager.OnBallHitGround(transform.position.x);
        }
    }

    public void ResetBall()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        firstHit = false;
    }

    public void LaunchServe(Vector2 velocity)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = velocity;
        firstHit = true;
    }
}
