using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Jacob Pratley - 100653937    
// October 22nd 2020

public class DisplayNodeOnLEDs : MonoBehaviour
{
    // A reference to our AI's decision making tree
    public DecisionTree tree;

    // These are the meshes that are supposed to be LED arrays
    // This implementation makes the assumption that the mesh renderers are in this order:
    // Top Left, Top Middle, Top Right, Middle Left, Centre Middle, Middle Right, Bottom Left, Botttom Middle, Bottom Right
    // The UVs of the meshes have been setup so one only needs to swap the texture to X, O, or Empty to display the desired character
    public MeshRenderer[] LEDs;

    // The LED materials
    // There's a better way to do this but I haven't done much work with dynamic materials in Unity
    // So the best solution for me atm is to just swap out the materials 
    // Victory material goes unused in the final build, I just didn't have time to add it in :\
    public Material emptyMaterial;
    public Material xMaterial;
    public Material oMaterial;
    public Material xVictoryMaterial;
    public Material oVictoryMaterial;

    public void DisplayCurrentNode()
    {
        // Ensure that the LEDs provided in the inspector has the exact right amount
        Debug.Assert(LEDs.Length == 9, "Not enough or too many LED meshes!");

        // Set all the LEDs to display nothing
        foreach (var LED in LEDs)
        {
            LED.material = emptyMaterial;
        }

        // We must traverse the tree vertically, since each node only holds a single move
        Node cNode = tree.currentNode;
        while (cNode != null)
        {
            // Retrieve the value of the current node's move
            string spaceVal = cNode.move.value.ToString().ToLower();

            if (spaceVal != "null")
            {
                // This version of the display class no longer makes the assumption that AI is O and Player is X
                // Depending on who went first, replace ai or player with X and O to reflect the person who goes first
                if (tree.firstMove == BoardSpaceValue.Player)
                {
                    spaceVal = spaceVal.Replace("ai", "O");
                    spaceVal = spaceVal.Replace("player", "X");
                }
                else
                {
                    spaceVal = spaceVal.Replace("ai", "X");
                    spaceVal = spaceVal.Replace("player", "O");
                }

                // Flatten Vector2Int converts the 2D coordinates of the move position into the linear coordinates of the 10 element text array
                // eg. (0, 0) => 0, (1, 0) => 1, (2, 0) => 2, (0, 1) => 3 ... (2, 2) => 8
                LEDs[Utils.FlattenVector2Int(cNode.move.pos, 3)].material = (spaceVal == "O" ? oMaterial : xMaterial);
            }

            // Traverse to the parent and repeat
            cNode = cNode.parent;
        }

        // If we just displayed a win, trigger the OnWin() function to end the game!
        if(tree.currentNode.winState != WinState.None)
        {
            GameManager.instance.OnWin(tree.currentNode);
        }
    }
}
