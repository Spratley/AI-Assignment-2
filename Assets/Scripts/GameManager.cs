using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Jacob Pratley - 100653937    
// October 21st 2020

public class GameManager : MonoBehaviour
{
    // This is a pseudo singleton 
    public static GameManager instance;

    // A reference to our AI's decision making tree
    public DecisionTree tree;

    // The animator component for the AI, used to trigger the AI's turn animation
    public Animator aiAnimator;

    // This is a super sketchy solution to the introduction
    // We need this referenceto trigger the light animation when the game begins
    public LightingAnimator la;

    // Confetti particles that will be shot when the AI wins (Cause let's face it, the player can't win)
    // A party horn sound will also play
    public ParticleSystem[] particleSystems;
    public AudioSource hornSource;

    // References to all the things we need to turn off an on during the course of the game
    public GameObject[] xOButtons;
    public Text winnerText;
    public GameObject[] gameEndButtons;
    public GameObject loadingScreen;
    public GameObject reticle;
    public LineRenderer lr;

    // Boolean to control the flow of the game
    // In a more complex system, a state machine would be preferrable
    private bool isGameRunning = false;

    // Pseudo singleton, instead of creating a new instance when you get the variable,
    // Delete all duplicate game managers in the scene
    // This must be done in order to use the public variables in the inspector 
    private void Awake()
    {
        if(instance != null)
        {
            Destroy(this.gameObject);
        }

        instance = this;

        ResetGame();
    }

    // Unity's built in OnClick event is used with the UI buttons, and it does not support enums
    // For reference, these are the possible values, although 2 and 3 should be the only ones used.
    // 0 = NULL,
    // 1 = Empty,
    // 2 = AI,
    // 3 = Player
    // This function is called by the X and O buttons in the inspector
    // Set who is the first player to make their move in the game then disable the selection buttons
    public void SetFirstMove(int firstMove)
    {
        switch(firstMove)
        {
            case 0:
                Debug.LogError("NULL was entered as first move!");
                break;
            case 1:
                Debug.LogError("Empty was entered as first move!");
                break;
            case 2:
                tree.firstMove = BoardSpaceValue.AI;
                break;
            case 3:
                tree.firstMove = BoardSpaceValue.Player;
                break;
        }

        foreach (var button in xOButtons)
        {
            button.SetActive(false);
        }
    }

    // LoadTree is called as a coroutine in order to display "NOW LOADING..." on screen
    public void StartGame()
    {
        if(isGameRunning)
        {
            return;
        }
        StartCoroutine(LoadTree());
    }

    // Called by the Quit button in the game
    public void QuitGame()
    {
        Application.Quit();
    }

    // Display the "NOW LOADING..." screen then wait one frame
    // Then generate the tree
    // If we don't wait for the frame, the loading screen will not appear until after the tree has generated
    // (Since this is all one thread)
    public IEnumerator LoadTree()
    {
        loadingScreen.SetActive(true);
        yield return null;
        tree.GenerateTree(OnTreeGenerated);
    }

    // The callback function that is activated when the tree is finished generating
    // This turns off the loading screen, locks the cursor, and triggers any needed animation
    public void OnTreeGenerated()
    {
        loadingScreen.SetActive(false);

        isGameRunning = true;

        la.StartLightAnimation();

        Cursor.lockState = CursorLockMode.Locked;
        reticle.SetActive(true);

        if (tree.firstMove == BoardSpaceValue.AI)
        {
            TriggerAIAnimation();
        }
    }

    // Frees up the cursor and enables the quit and restart buttons
    public void EndGame()
    {
        if(!isGameRunning)
        {
            return;
        }

        isGameRunning = false;

        Cursor.lockState = CursorLockMode.None;
        reticle.SetActive(false);

        foreach (var button in gameEndButtons)
        {
            button.SetActive(true);
        }
    }

    // Enables the X and O buttons, and disables all UI that would not be visible at the start
    public void ResetGame()
    {
        foreach (var button in xOButtons)
        {
            button.SetActive(true);
        }

        winnerText.gameObject.SetActive(false);
        foreach (var button in gameEndButtons)
        {
            button.SetActive(false);
        }
        reticle.SetActive(false);
    }

    // Attempts to run a player turn when called
    // Will place an X or O (depending on who went first) at the playerMove position
    // Only works if the game is running *and* it is the player's turn
    // Will immediately call the AI's turn if the game is not over
    public void PlayerTurn(Vector2Int playerMove)
    {
        if(!isGameRunning)
        {
            return;
        }

        BoardSpaceValue val = tree.currentNode.move.value;

        // If the current board space value is the AI, that means that the last placed move was by the AI
        // It's the player's turn now
        if(val == BoardSpaceValue.AI || (val == BoardSpaceValue.NULL && tree.firstMove == BoardSpaceValue.Player))
        {
            tree.TraverseToCurrentChild(playerMove);

            // If there is still a turn able to be made, trigger the AI to make their turn
            if(tree.currentNode.winState == WinState.None)
            {
                TriggerAIAnimation();
            }

        }
    }

    // When called, the AI will play its turn animation
    // Since the turn actions are triggered by the animation, we only need to start the animation to perform the turn
    public void TriggerAIAnimation()
    {
        if(!isGameRunning)
        {
            return;
        }

        aiAnimator.SetTrigger("Trigger Turn");
    }

    // AI Turn function called by the animation
    // Similar to the player, will place an X or O at a random space
    // (since the tree has been pruned with Alpha/Beta pruning, there is no need to evaluate score as the only present nodes are the ones with the highest score)
    public void AITurn()
    {
        if(!isGameRunning)
        {
            return;
        }

        BoardSpaceValue val = tree.currentNode.move.value;
        
        // If the current board space value is the player, that means that the last placed move was by the player
        // It's the AI's turn now
        if (val == BoardSpaceValue.Player || (val == BoardSpaceValue.NULL && tree.firstMove == BoardSpaceValue.AI))
        {
            tree.TraverseToRandomChild();
        }

        SetLaserPosition();
    }

    // The AI has a laser that is momentarily enabled in the animation
    // This function converts a tree position to a string name, then finds the GameObject with the same name
    // This makes the assumption that all the grid spaces are named TL, TM, TR, ML, MM, MR, BL, BM, and BR and that no other GameObjects share their name
    // The end of the line renderer is set to the position of the found object
    public void SetLaserPosition()
    {
        Vector2Int pos = tree.currentNode.move.pos;

        string name = "";

        // Position on the Y => vertical position, Top, Middle or Bottom
        switch (pos.y)
        {
            case 0:
                name += "T";
                break;
            case 1:
                name += "M";
                break;
            case 2:
                name += "B";
                break;
        }

        // Position on the X => horizontal position, Left, Middle, or Right
        switch (pos.x)
        {
            case 0:
                name += "L";
                break;
            case 1:
                name += "M";
                break;
            case 2:
                name += "R";
                break;
        }

        GameObject found = GameObject.Find(name);

        if (found)
        {
            lr.SetPosition(0, found.transform.position);
        }
        // If we don't find an object, set the end of the line to the beginning of the line so nothing is drawn
        else
        {
            lr.SetPosition(0, lr.GetPosition(1));
        }
    }

    // Displays the proper message for winning and shoots confetti
    public void OnWin(Node winningMove)
    {
        ShootConfetti();

        if(winningMove.winState == WinState.Tie)
        {
            winnerText.text = "It's a Tie!";
        }
        else
        {
            winnerText.text = winningMove.move.value.ToString() + " Wins!";
        }

        winnerText.gameObject.SetActive(true);

        EndGame();
    }

    // Just activates the confetti particles and plays the celebration sound
    private void ShootConfetti()
    {
        foreach (var particleSystem in particleSystems)
        {
            particleSystem.Play();
        }

        hornSource.Play();
    }
}
