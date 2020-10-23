using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Jacob Pratley - 100653937    
// October 21st 2020

// This is a debug class that displays the current Tic-Tac-Toe grid on a basic GUI
// Unused in the final game
// This was called using the UnityEvent system so that it doesn't have to run every frame
public class DisplayNodeOnGUI : MonoBehaviour
{
    public DecisionTree tree;
    public GameObject nodeDisplay;

    public void DisplayCurrentNode()
    {
        // These are the text components in the scene, the first 9 are the Tic-Tac-Toe grid spaces
        // The 10th is just a text display to print any debug messages relative to the current node
        Text[] gridTexts = nodeDisplay.GetComponentsInChildren<Text>();
        Debug.Assert(gridTexts.Length == 10, "Not enough or too many grid texts!");

        // Clear the grid
        foreach (var t in gridTexts)
        {
            t.text = "";
        }

        // We must traverse the tree vertically, since each node only holds a single move
        Node cNode = tree.currentNode;
        while (cNode != null)
        {
            // Retrieve the value of the current node's move
            string spaceVal = cNode.move.value.ToString();

            if (spaceVal != "NULL")
            {
                // This makes the assumption that AI is O and Player is X,
                // This will not be changed as this is a debug class and not used in the final product
                spaceVal = spaceVal.Replace("AI", "O");
                spaceVal = spaceVal.Replace("Player", "X");

                // Flatten Vector2Int converts the 2D coordinates of the move position into the linear coordinates of the 10 element text array
                // eg. (0, 0) => 0, (1, 0) => 1, (2, 0) => 2, (0, 1) => 3 ... (2, 2) => 8
                // As I wrote that out I realized that it's technically trinary (ternary?) and that's cool :D
                gridTexts[Utils.FlattenVector2Int(cNode.move.pos, 3)].text = spaceVal;
            }

            // Traverse one more node upwards in the tree
            cNode = cNode.parent;
        }

        // Display debug information, including the current node's score and the board's WinState
        gridTexts[gridTexts.Length - 1].text = "Score: " + tree.currentNode.score + " " + tree.currentNode.winState.ToString();
    }
}
