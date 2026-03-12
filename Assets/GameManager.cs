using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Objets du jeu")]
    public GameObject player1Obj;
    public GameObject player2Obj;
    public GameObject ballObj;
    public GameObject scoreP1TensObj;    // Dizaines Player1
    public GameObject scoreP1UnitsObj;   // Unités Player1
    public GameObject scoreP2TensObj;    // Dizaines Player2
    public GameObject scoreP2UnitsObj;   // Unités Player2
    public GameObject pointTextObj;      // Message point
    public GameObject groundObj;

    [HideInInspector] public GameObject servingPlayerObj; // joueur qui sert

    [Header("Paramètres du jeu")]
    public float ballForce = 8f;
    public Vector2 player1StartPos = new Vector2(-5, -3);
    public Vector2 player2StartPos = new Vector2(5, -3);
    public float messageDuration = 1.5f;

    [Header("Score")]
    public int player1Score = 0;
    public int player2Score = 0;

    // Composants internes
    private Transform player1;
    private Transform player2;
    private Transform ball;
    private Rigidbody2D ballRb;
    private TMP_Text scoreP1Tens;
    private TMP_Text scoreP1Units;
    private TMP_Text scoreP2Tens;
    private TMP_Text scoreP2Units;
    private TMP_Text pointText;
    private BallController ballController;

    private bool waitingForServe = true;

    void Start()
    {
        // Récupération des composants
        player1 = player1Obj.transform;
        player2 = player2Obj.transform;
        ball = ballObj.transform;
        ballRb = ballObj.GetComponent<Rigidbody2D>();

        scoreP1Tens = scoreP1TensObj.GetComponent<TMP_Text>();
        scoreP1Units = scoreP1UnitsObj.GetComponent<TMP_Text>();
        scoreP2Tens = scoreP2TensObj.GetComponent<TMP_Text>();
        scoreP2Units = scoreP2UnitsObj.GetComponent<TMP_Text>();

        pointText = pointTextObj.GetComponent<TMP_Text>();
        pointText.text = "";

        ballController = ballObj.GetComponent<BallController>();
        ballController.gameManager = this;
        ballController.groundObj = groundObj;

        UpdateScore();
        ResetRound(player1Obj); // Player1 commence
    }

    // Quand la balle touche le sol
    public void OnBallHitGround(float ballX)
    {
        if (ballX < 0)
        {
            Player2Scores();
            ShowPointMessage("Player 2 marque !");
            ResetRound(player1Obj); // perdant sert
        }
        else
        {
            Player1Scores();
            ShowPointMessage("Player 1 marque !");
            ResetRound(player2Obj); // perdant sert
        }
    }

    // Gestion des scores
    public void Player1Scores()
    {
        player1Score++;
        UpdateScore();
    }

    public void Player2Scores()
    {
        player2Score++;
        UpdateScore();
    }

    void UpdateScore()
    {
        // Player1
        int tens = player1Score / 10;
        int units = player1Score % 10;
        if (scoreP1Tens != null) scoreP1Tens.text = tens.ToString();
        if (scoreP1Units != null) scoreP1Units.text = units.ToString();

        // Player2
        tens = player2Score / 10;
        units = player2Score % 10;
        if (scoreP2Tens != null) scoreP2Tens.text = tens.ToString();
        if (scoreP2Units != null) scoreP2Units.text = units.ToString();
    }

    // Reset de la round
    void ResetRound(GameObject nextServer)
    {
        // Repositionner les joueurs
        player1.position = player1StartPos;
        player2.position = player2StartPos;
        player1Obj.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        player2Obj.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

        // Définir le serveur
        servingPlayerObj = nextServer;

        // Placer la balle sur la tête du serveur
        ball.position = servingPlayerObj.transform.position + Vector3.up * 1f;

        // Bloquer la balle jusqu'au premier contact
        ballController.ResetBall();

        waitingForServe = true;
    }

    // Affichage message point
    private void ShowPointMessage(string message)
    {
        if (pointText != null)
        {
            pointText.text = message;
            CancelInvoke(nameof(ClearPointMessage));
            Invoke(nameof(ClearPointMessage), messageDuration);
        }
    }

    private void ClearPointMessage()
    {
        if (pointText != null)
            pointText.text = "";
    }
}