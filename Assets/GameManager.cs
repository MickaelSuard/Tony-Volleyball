using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Objets du jeu")]
    public GameObject player1Obj;
    public GameObject player2Obj;
    public GameObject ballObj;
    public GameObject scoreP1TensObj;
    public GameObject scoreP1UnitsObj;
    public GameObject scoreP2TensObj;
    public GameObject scoreP2UnitsObj;
    public GameObject pointTextObj;
    public GameObject groundObj;

    [HideInInspector] public GameObject servingPlayerObj;

    [Header("Paramètres du jeu")]
    public float ballForce = 8f;
    public Vector2 player1StartPos = new Vector2(-5, -3);
    public Vector2 player2StartPos = new Vector2(5, -3);
    public float messageDuration = 1.5f;
    public int maxScore = 10; // On définit la limite ici

    [Header("Score")]
    public int player1Score = 0;
    public int player2Score = 0;

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
    private bool gameOver = false;

    public bool soloMode = true;

    [Header("UI Touches (Groupe de Tiles)")]
    // Glisse le Panel "ZQSD_Group" complet ici dans l'Inspecteur
    public GameObject zqsdGroup; 
    public GameObject arrowGroup; 
    public float displayDuration = 5f;

    public void InitializeGame(bool isSolo)
    {
        soloMode = isSolo;

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

        // Gestion Solo vs Multi
        AIController ai = player2Obj.GetComponent<AIController>();
        PlayerController p2Control = player2Obj.GetComponent<PlayerController>();

        if (soloMode)
        {
            if (p2Control != null) p2Control.enabled = false;
            if (ai == null) ai = player2Obj.AddComponent<AIController>();
            ai.ball = ballObj.transform;
            ai.enabled = true;
        }
        else
        {
            if (ai != null) ai.enabled = false;
            if (p2Control != null) p2Control.enabled = true;
        }

        if (zqsdGroup != null)
        {
            StartCoroutine(ShowControlsCoroutine());
        }

        UpdateScore();
        ResetRound(player1Obj);
    }
    IEnumerator ShowControlsCoroutine()
    {
        // On active le groupe complet d'un coup
        zqsdGroup.SetActive(true); 
        arrowGroup.SetActive(false);
        if(!soloMode && arrowGroup != null) arrowGroup.SetActive(true);
        
        // On attend la durée définie (3 secondes)
        yield return new WaitForSeconds(displayDuration); 
        
        // On cache le groupe complet
        zqsdGroup.SetActive(false); 
        arrowGroup.SetActive(false);
    }

    public void OnBallHitGround(float ballX)
    {
        if (gameOver) return; // Si c'est fini, on ne fait plus rien

        if (ballX < 0)
        {
            Player2Scores();
            if (!gameOver) // On vérifie si ce point n'était pas le dernier
            {
                ShowPointMessage("Player 2 marque !");
                ResetRound(player1Obj);
            }
        }
        else
        {
            Player1Scores();
            if (!gameOver)
            {
                ShowPointMessage("Player 1 marque !");
                ResetRound(player2Obj);
            }
        }
    }

    public void Player1Scores()
    {
        player1Score++;
        UpdateScore();
        CheckWin();
    }

    public void Player2Scores()
    {
        player2Score++;
        UpdateScore();
        CheckWin();
    }

    void UpdateScore()
    {
        int tens = player1Score / 10;
        int units = player1Score % 10;
        if (scoreP1Tens != null) scoreP1Tens.text = tens.ToString();
        if (scoreP1Units != null) scoreP1Units.text = units.ToString();

        tens = player2Score / 10;
        units = player2Score % 10;
        if (scoreP2Tens != null) scoreP2Tens.text = tens.ToString();
        if (scoreP2Units != null) scoreP2Units.text = units.ToString();
    }

    // --- LOGIQUE DE VICTOIRE ---
    void CheckWin()
    {
        if (player1Score >= maxScore)
        {
            EndGame("PLAYER 1 GAGNE !");
        }
        else if (player2Score >= maxScore)
        {
            EndGame("PLAYER 2 GAGNE !");
        }
    }

    void EndGame(string winnerMessage)
    {
        gameOver = true;
        pointText.text = winnerMessage;

        // On arrête la balle
        ballRb.linearVelocity = Vector2.zero;
        ballRb.simulated = false;

        // Optionnel : Relancer la partie après 3 secondes
        Invoke(nameof(RestartLevel), 3f);
    }

    void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void ResetRound(GameObject nextServer)
    {
        if (gameOver) return;

        player1.position = player1StartPos;
        player2.position = player2StartPos;
        player1Obj.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        player2Obj.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

        servingPlayerObj = nextServer;
        ball.position = servingPlayerObj.transform.position + Vector3.up * 1f;
        ballController.ResetBall();
    }

    private void ShowPointMessage(string message)
    {
        if (pointText != null && !gameOver)
        {
            pointText.text = message;
            CancelInvoke(nameof(ClearPointMessage));
            Invoke(nameof(ClearPointMessage), messageDuration);
        }
    }

    private void ClearPointMessage()
    {
        if (pointText != null && !gameOver)
            pointText.text = "";
    }
}