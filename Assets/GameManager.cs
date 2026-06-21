using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.InputSystem;

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
    [Header("Service")]
    public float pointRestartDelay = 3f;
    public float netX = 0f;
    public float serveSideOffset = 0f;
    public float serveBallY = 1f;
    public float serveForce = 6f;
    public float serveUpwardForce = 2f;
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
    private bool gameStarted = false;
    private bool roundResetting = false;
    private bool player1AiEnabled = false;
    private bool player2AiEnabled = false;

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

        ConfigurePlayerAI(player1Obj, false, true);
        player1AiEnabled = false;

        if (soloMode)
        {
            ConfigurePlayerAI(player2Obj, true, false);
            player2AiEnabled = true;
        }
        else
        {
            ConfigurePlayerAI(player2Obj, false, false);
            player2AiEnabled = false;
        }

        if (zqsdGroup != null)
        {
            StartCoroutine(ShowControlsCoroutine());
        }

        UpdateScore();
        gameStarted = true;
        StartCoroutine(StartRoundAfterDelay(player1Obj));
    }

    void Update()
    {
        if (!gameStarted || gameOver || Keyboard.current == null) return;

        if (Keyboard.current[Key.T].wasPressedThisFrame)
        {
            TogglePlayerAI(player1Obj, ref player1AiEnabled, true, "Player 1");
        }

        if (Keyboard.current[Key.Digit3].wasPressedThisFrame || Keyboard.current[Key.Numpad3].wasPressedThisFrame)
        {
            TogglePlayerAI(player2Obj, ref player2AiEnabled, false, "Player 2");
        }
    }

    private void TogglePlayerAI(GameObject playerObj, ref bool aiEnabled, bool playOnLeftSide, string playerName)
    {
        aiEnabled = !aiEnabled;
        ConfigurePlayerAI(playerObj, aiEnabled, playOnLeftSide);
        // ShowPointMessage(aiEnabled ? $"IA active pour {playerName}" : $"IA desactivee pour {playerName}");
    }

    private void ConfigurePlayerAI(GameObject playerObj, bool enableAI, bool playOnLeftSide)
    {
        PlayerController playerController = playerObj.GetComponent<PlayerController>();
        if (playerController != null) playerController.enabled = !enableAI;

        AIController ai = playerObj.GetComponent<AIController>();
        if (ai == null) ai = playerObj.AddComponent<AIController>();

        ai.ball = ballObj.transform;
        ai.playOnLeftSide = playOnLeftSide;

        if (playOnLeftSide)
        {
            ai.minX = -8f;
            ai.maxX = -0.5f;
            ai.centerX = -4f;
        }
        else
        {
            ai.minX = 0.5f;
            ai.maxX = 8f;
            ai.centerX = 4f;
        }

        ai.enabled = enableAI;
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
        if (gameOver || roundResetting) return; // Si c'est fini, on ne fait plus rien

        if (ballX < 0)
        {
            Player2Scores();
            if (!gameOver) // On vérifie si ce point n'était pas le dernier
            {
                StartCoroutine(ResetRoundAfterPoint(player1Obj, "Player 2 marque !"));
            }
        }
        else
        {
            Player1Scores();
            if (!gameOver)
            {
                StartCoroutine(ResetRoundAfterPoint(player2Obj, "Player 1 marque !"));
            }
        }
    }

    IEnumerator ResetRoundAfterPoint(GameObject losingPlayer, string message)
    {
        roundResetting = true;
        ShowPointMessage(message);
        CancelInvoke(nameof(ClearPointMessage));

        PrepareRound(losingPlayer);

        yield return new WaitForSeconds(pointRestartDelay);

        LaunchServe(losingPlayer);
        ClearPointMessage();
        roundResetting = false;
    }

    IEnumerator StartRoundAfterDelay(GameObject losingPlayer)
    {
        roundResetting = true;
        PrepareRound(losingPlayer);

        yield return new WaitForSeconds(pointRestartDelay);

        LaunchServe(losingPlayer);
        roundResetting = false;
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

    void PrepareRound(GameObject nextServer)
    {
        if (gameOver) return;

        player1.position = player1StartPos;
        player2.position = player2StartPos;
        player1Obj.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        player2Obj.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

        servingPlayerObj = nextServer;
        ball.position = GetServeBallPosition(nextServer);
        ballController.ResetBall();
    }

    private void LaunchServe(GameObject losingPlayer)
    {
        if (gameOver) return;

        float side = losingPlayer == player1Obj ? -1f : 1f;
        Vector2 velocity = new Vector2(side * serveForce, serveUpwardForce);
        ballController.LaunchServe(velocity);
    }

    private Vector3 GetServeBallPosition(GameObject losingPlayer)
    {
        float side = losingPlayer == player1Obj ? -1f : 1f;
        return new Vector3(netX + serveSideOffset * side, serveBallY, ball.position.z);
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
