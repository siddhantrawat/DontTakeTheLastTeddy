using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// Game manager
/// </summary>
public class DontTakeTheLastTeddy : MonoBehaviour
{
    Board board;
    Player player1;
    Player player2;
    bool playertoggle = true; //true equals player one. . .for easy first player toggle
    // events invoked by class
    TakeTurn takeTurnEvent = new TakeTurn();
    GameOver gameOverEvent = new GameOver();
    GameStartingEvent gameStartingEvent = new GameStartingEvent();
    Timer inBetweenGamesTimer;
    bool gameOver  =false; // for efficiency

    //set diffculty support
    int difficultyCase = 1;
    int totalGames = 0;
    /// <summary>
    /// Awake is called before Start
    /// </summary>
    void Awake()
    {
        // retrieve board and player references
        board = GameObject.FindGameObjectWithTag("Board").GetComponent<Board>();
        player1 = GameObject.FindGameObjectWithTag("Player1").GetComponent<Player>();
        player2 = GameObject.FindGameObjectWithTag("Player2").GetComponent<Player>();

        // register as invoker and listener
        EventManager.AddTakeTurnInvoker(this);
        EventManager.AddGameOverInvoker(this);
        EventManager.AddTurnOverListener(HandleTurnOverEvent);
        EventManager.AddGameStartInvoker(this);
        EventManager.AddGameStartListener(NewGame);
        EventManager.AddGameOverListener(UpdateScore);
    }

    /// <summary>
    /// Use this for initialization
    /// </summary>
    void Start()
    {

        StartGame(PlayerName.Player1, Difficulty.Hard, Difficulty.Hard);
        inBetweenGamesTimer = gameObject.AddComponent<Timer>();
        inBetweenGamesTimer.Duration = 0.01f;
        Debug.Log("timer added");
    }

    private void Update()
    {
       

        if (gameOver && inBetweenGamesTimer.Finished)
        {
            if (difficultyCase == 7)
            {
                SceneManager.LoadScene("statistics");
            }

            if (totalGames % 100 == 0)
            {
                difficultyCase++;
                Debug.Log("difficulty  case  =  " + difficultyCase);
            }

            Debug.Log("new game staring");
            gameOver = false;
            gameStartingEvent.Invoke();
            
        }
    }
    /// <summary>
    /// Adds the given listener for the TakeTurn event
    /// </summary>
    /// <param name="listener">listener</param>
    public void AddTakeTurnListener(UnityAction<PlayerName, Configuration> listener)
    {
        takeTurnEvent.AddListener(listener);
    }

    /// <summary>
    /// Adds the given listener for the GameOver event
    /// </summary>
    /// <param name="listener">listener</param>
    public void AddGameOverListener(UnityAction<PlayerName> listener)
    {
        gameOverEvent.AddListener(listener);
    }

    public void AddGameStartListener(UnityAction listener)
    {
        gameStartingEvent.AddListener(listener);
    }




    /// <summary>
    /// Starts a game with the given player taking the
    /// first turn
    /// </summary>
    /// <param name="firstPlayer">player taking first turn</param>
    /// <param name="player1Difficulty">difficulty for player 1</param>
    /// <param name="player2Difficulty">difficulty for player 2</param>
    /// 
    void StartGame(PlayerName firstPlayer, Difficulty player1Difficulty,
        Difficulty player2Difficulty)
    {
        // set player difficulties
        player1.Difficulty = player1Difficulty;
        player2.Difficulty = player2Difficulty;

        // create new board
        board.CreateNewBoard();
        takeTurnEvent.Invoke(firstPlayer,
            board.Configuration);
    }

    /// <summary>
    /// Handles the TurnOver event by having the 
    /// other player take their turn
    /// </summary>
    /// <param name="player">who finished their turn</param>
    /// <param name="newConfiguration">the new board configuration</param>
    void HandleTurnOverEvent(PlayerName player,
        Configuration newConfiguration)
    {
        board.Configuration = newConfiguration;

        // check for game over
        if (newConfiguration.Empty)
        {
            // fire event with winner
            if (player == PlayerName.Player1)
            {
                gameOverEvent.Invoke(PlayerName.Player2);
                totalGames++;
                
            }
            else
            {
                gameOverEvent.Invoke(PlayerName.Player1);
                totalGames++;
            }
              gameOver = true;
              inBetweenGamesTimer.Run();
           
            
        }
        else
        {
            // game not over, so give other player a turn
            if (player == PlayerName.Player1)
            {
                takeTurnEvent.Invoke(PlayerName.Player2,
                    newConfiguration);
            }
            else
            {
                takeTurnEvent.Invoke(PlayerName.Player1,
                    newConfiguration);
            }
        }
    }

    void NewGame()
    {
        if (playertoggle)
        
            StartGame(PlayerName.Player2, SetDifficulty(PlayerName.Player1), SetDifficulty(PlayerName.Player2));
        
        else
            StartGame(PlayerName.Player1, SetDifficulty(PlayerName.Player1), SetDifficulty(PlayerName.Player2));

        playertoggle = !playertoggle;
    }

    Difficulty SetDifficulty( PlayerName player)
    {
        switch (difficultyCase)
        {
            case 1: return Difficulty.Easy; break;
            case 2: return Difficulty.Medium; break;
            case 3: return Difficulty.Hard; break;
            case 4: if (player == PlayerName.Player1) return Difficulty.Easy; else return Difficulty.Medium; break;
            case 5: if (player == PlayerName.Player1) return Difficulty.Easy; else return Difficulty.Hard; break;
            case 6: if (player == PlayerName.Player1) return Difficulty.Medium; else return Difficulty.Hard; break;
            default: return Difficulty.Easy;
        }
    }

    void UpdateScore(PlayerName winner)
    {
        if (winner == PlayerName.Player1)
            Statistics.UpdateScore(difficultyCase - 1, 0);
        else
            Statistics.UpdateScore(difficultyCase-1, 1);

     
    }
}
