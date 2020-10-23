using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Events;

// Jacob Pratley - 100653937    
// October 20th 2020

// Delegate functions are used as a callback to alert the game when the tree is done generating
// This is actually an artifact from when I attempted to GenerateTree() in a Coroutine
// The purpose of the coroutine was to facilitate the creation of a progress bar so you wouldn't be left wondering if the game froze or if it's actually loading
// Coroutines mimic the behaviour of multi-threading in Unity, but they are simply running after the main loop's execution each frame
// By forcing the tree to generate one child a frame, we greatly limit the performance of the generation and it becomes impractical to do so
// In an ideal world, this would be put onto its own thread
// Multithreading in C# is something I'll look into for future Unity projects so I don't run into the same issue
public delegate void OnGenerateDelegate();

public class DecisionTree : MonoBehaviour
{
    // References to the root of the tree and the current state of the board
    public Node root;
    public Node currentNode;
    
    // BoardSpaceValue enum is used for convenience sake  
    // This variable stores if the AI or Player will be going first
    public BoardSpaceValue firstMove;

    // UnityEvent system is super helpful, it allows you to link the execution of functions in the inspector to a Invoke() event in the script
    // In this case, we use the OnRefreshDisplay() event to display the current board state on the screen
    public UnityEvent OnRefreshDisplay;

    // GenerateTree creates a decision tree from the ground up each time it's called
    public void GenerateTree(OnGenerateDelegate onGenerateDelegate)
    {
        GenerateRawTree();
        Debug.Log("Starting pruning");
        PruneNode(currentNode);

        OnRefreshDisplay.Invoke();
        onGenerateDelegate.Invoke();
    }

    // Generates all 9 possible children of an empty board, then recursively generates all their children, and grand children, etc.
    // This tree has no optimization (ie. Alpha/Beta pruning)
    private void GenerateRawTree()
    {
        root = new Node();
        root.ownerTree = this;

        // Nodes store their move position in a Vector2Int that represents the position on the board 
        // (0, 0) is the top left, (2, 2) is the bottom right
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                GenerateChild(root, new Vector2Int(x, y), firstMove);
            }
        }

        root.CalculateScore();
        // Set the current scope of the board to the root node
        // This effectively starts a new game
        currentNode = root;
    }

    // Recursive function that generates a new child and all of its children
    private void GenerateChild(Node parent, Vector2Int movePos, BoardSpaceValue moveType)
    {
        Node child = new Node(moveType, movePos, parent, this);

        // Do not generate more moves if there is already a winner
        WinState winner = child.winState;
        if(winner == WinState.None)
        {
            for(int x = 0; x < 3; x++)
            {
                for(int y = 0; y < 3; y++)
                {
                    // If the child does not have an available move at the looped position then there is already an X or O there, and we should not put a child there
                    if(!child.GetAvailableMoves().Contains(new Vector2Int(x, y)))
                    {
                        continue;
                    }

                    GenerateChild(child, new Vector2Int(x, y), Node.FlipBoardSpaceValue(moveType));
                }
            }

        }
    }

    // Alpha-beta pruning
    // Recursively searches every node, and deletes any child nodes that do not put the AI at an advantage on their turn, while assuming that the player will be selecting the best moves against them
    private void PruneNode(Node node)
    {
        // Don't care to prune choices if the node doesn't have any
        if(node.children.Count <= 1)
        {
            return;
        }

        for (int i = 0; i < node.children.Count; i++)
        {
            // Only remove AI choices, we cannot predict what the player will do and therefore must leave all options available
            // If the current node was a player's move, then that means it's our AI's turn and time to trim!
            if(node.move.value == BoardSpaceValue.Player)
            {
                // For the node where it is the AI's turn, the score represents the biggest 
                if(node.children[i].score < node.score)
                {
                    node.children.RemoveAt(i);
                    i--;
                    continue;
                }
            }

            PruneNode(node.children[i]);
        }
    }

    // Sets the current node to the current node's parent and refreshes the display
    // This was used to debug early in the process
    public void TraverseToCurrentParent()
    {
        if (currentNode.parent != null)
        {
            currentNode = currentNode.parent;
        }
        OnRefreshDisplay.Invoke();
    }

    // Randomly sets the current node to one of its children and refreshes the display
    // This is used to select the AI's turn
    public void TraverseToRandomChild()
    {
        int childIndex = Random.Range(0, currentNode.children.Count);
        currentNode = currentNode.children[childIndex];
        OnRefreshDisplay.Invoke();
    }

    // Sets the current node to a child at the provided index
    // If the index is not within the bounds of the child list, the function does nothing
    public void TraverseToCurrentChild(int childID)
    {
        if (childID < currentNode.children.Count && childID > 0)
        {
            currentNode = currentNode.children[childID];
        }
        OnRefreshDisplay.Invoke();
    }
    
    // Sets the current node to a child whom's move corrisponds to the provided one
    // If no child has a matching move then the function does nothing
    // True is returned if the function succeeds
    // False returns otherwise, thee were used to debug
    public bool TraverseToCurrentChild(Vector2 movePos)
    {
        var foundChildren = currentNode.children.Where(c => c.move.pos == movePos).ToArray();

        if (foundChildren.Length > 0)
        {
            currentNode = foundChildren[0];
            OnRefreshDisplay.Invoke();
            return true;
        }

        return false;
    }

    // Sets the current node to the direct neighbor sibling to the left or right
    // If right is true, the function will attempt to switch to the sibling with an index that is exactly one greater
    // Else the function will attempt to switch to the sibling with an index that is exactly one less
    // Function will not run if the node has no parent
    public void TraverseToCurrentSibling(bool right)
    {
        if(currentNode.parent == null)
        {
            return;
        }

        // Get the index of this node and increment/decrement it 
        int index = currentNode.parent.children.FindIndex(n => n == currentNode);
        // Ternary operator, this is the equivalent to 
        // if(right)
        // {
        //     index += 1;
        // }
        // else
        // {
        //     index += -1;
        // }
        index += right ? 1 : -1;

        // Move the current node to the parent, then move back down to the child at our new index
        TraverseToCurrentParent();
        index = Mathf.Clamp(index, 0, currentNode.children.Count - 1);
        TraverseToCurrentChild(index);
    }

}
