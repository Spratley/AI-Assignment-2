using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Jacob Pratley - 100653937    
// October 20th 2020

// Enums are used to make the code less confusing than using something like 0 for X and 1 for O
// On top of that, AI and Player are used behind the scenes instead of X and O so that the player can choose which they want to play as
public enum BoardSpaceValue
{
    NULL,
    Empty,
    AI,
    Player
}

public enum WinState
{
    NULL,
    None,
    Win,
    Tie
}

// This struct holds the data for a single move (placing an X or O on the board)
public struct Move
{
    public Move(BoardSpaceValue moveValue, Vector2Int movePos)
    {
        value = moveValue;
        pos = movePos;
    }

    public BoardSpaceValue value;
    public Vector2Int pos;
}

public class Node
{
    // Constructor that takes in all the data required to make a Node
    public Node(BoardSpaceValue moveValue, Vector2Int movePos, Node parent, DecisionTree ownerTree)
    {
        this.ownerTree = ownerTree;

        move = new Move(moveValue, movePos);

        this.parent = parent;
        Debug.Assert(parent != null);

        parent.children.Add(this);

        //availableMoves = new List<Vector2Int>(parent.availableMoves);
        Debug.Assert(parent.GetAvailableMoves().Contains(movePos), "Attempting to make a move that is invalid");
        //availableMoves.Remove(movePos);
    }

    // Empty default constructor, just to tell the compiler that we want to allow the creation of Nodes without the above constructor
    public Node() { }

    public static int totalNodes;

    public DecisionTree ownerTree;
    public Node parent = null;
    public List<Node> children = new List<Node>();

    public Move move;
    public int score;

    // Calculate available moves on access
    // This is part of an efficiency upgrade
    // The efficiency is that nodes do not contain the entire board, but instead just the single move that is the difference between the parent and this
    // Storing all available moves would make this efficiency redundant as that's almost as if you save the entire board
    // More information will (hopefully) be in my efficiency report
    public List<Vector2Int> GetAvailableMoves()
    {
        // Generate a list of all moves possible in a game of TicTacToe
        List<Vector2Int> availableMoves = new List<Vector2Int>();
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                availableMoves.Add(new Vector2Int(x, y));
            }
        }

        // Recursively search parent and eliminate available moves that are taken
        Node cNode = this;
        do
        {
            if(cNode.move.value == BoardSpaceValue.NULL)
            {
                break;
            }

            availableMoves.Remove(cNode.move.pos);
            cNode = cNode.parent;
        } while (cNode != null);

        // Return this new list
        return availableMoves;
    }

    public int CalculateScore()
    {
        // Set the score to 1 if the AI wins in this situation or -1 if the player wins
        if (winState == WinState.Win)
        {
            score = move.value == BoardSpaceValue.AI ? 1 : -1;
        }
        else if (winState == WinState.Tie)
        {
            score = 0;
        }
        else
        {
            BoardSpaceValue nextMoveType = FlipBoardSpaceValue(move.value);
            // nextMoveType will be NULL at the root node
            if(nextMoveType == BoardSpaceValue.NULL)
            {
                nextMoveType = ownerTree.firstMove;
            }

            // Initialize the score to either the highest possible or lowest possible value
            // If the AI is next, we'll be picking the highest score, so int.MinValue is guaranteed to be lower than any child's score
            // If the Player is next , we'll be picking the lowest score, so int.MaxValue is guaranteed to be higher than any child's score
            switch (nextMoveType)
            {
                case BoardSpaceValue.AI:
                    score = int.MinValue;
                    break;
                case BoardSpaceValue.Player:
                    score = int.MaxValue;
                    break;
            }

            foreach (var child in children)
            {
                // The AI will place their next move
                // Use Max score and pick the best moves possible
                if(nextMoveType == BoardSpaceValue.AI)
                {
                    score = Mathf.Max(score, child.CalculateScore());
                }
                // The Player will place their next move
                // Use Min score and assume that the player will pick the option that will limit the AI most
                else if (nextMoveType == BoardSpaceValue.Player)
                {
                    score = Mathf.Min(score, child.CalculateScore());
                }
                
            }
        }
        return score;
    }

    // C# Acessors allow us to make this a bit better
    // Instead of calculating the winner every time we want to check if there is one,
    // calculate once on the first call, then return that same value each subsequent call
    private WinState _winState = WinState.NULL;
    public WinState winState
    {
        get
        {
            if (_winState == WinState.NULL)
            {
                _winState = GetWinner();
            }

            return _winState;
        }
    }

    // Compare the position of this new move to all previous moves of this type
    // If two previous moves as well as this new one lie on any straight line, a winner has been found
    // If not, then check if the board is full. If so, then return a tie
    // Otherwise return none to signify the game is not over
    //
    // We can assume that this move will be the deciding factor in if the game has been won
    // This assumption can be made as the algorithm will not generate any children on a winning node
    private WinState GetWinner()
    {
        // Hypothetically this could be bit-shifted into a single byte (each two bits representing the two possible moves in question)
        // That is a memory usage optimization and wouldn't affect runtime significantly
        // Also there's definitely a much better way to do this idk

        int horizontalTally = 0;
        int verticalTally = 0;
        int posDiagonalTally = 0;
        int negDiagonalTally = 0;
        
        Node currentNode = this;
        int totalMoves = 1;
        while(currentNode.parent != null)
        {
            currentNode = currentNode.parent;
            totalMoves++;
            // Skip nodes of moves by the opponent, they cannot win if it's not their turn
            if(currentNode.move.value != this.move.value)
            {
                continue;
            }

            if(currentNode.move.pos.x == this.move.pos.x)
            {
                // The move in question's position is on the same horizontal line as this one! Mark up the tally
                horizontalTally++;
            }
            else if(currentNode.move.pos.y == this.move.pos.y)
            {
                // The move in question's position is on the same vertical line as this one! Mark up the tally
                verticalTally++;
            }

            // Check diagonals
            Vector2Int direction = currentNode.move.pos - this.move.pos;

            if (direction.x == direction.y)
            {
                // The move in question is on a direct diagonal from this one! Mark up the tally
                posDiagonalTally++;
            }

            if(direction.x == -direction.y)
            {
                // The move in question is on a direct diagonal from this one! Mark up the tally
                negDiagonalTally++;
            }
        }

        // If any of these tallies are greater than 2, we've found a winner!
        if(horizontalTally == 2 || verticalTally == 2 || posDiagonalTally == 2 || negDiagonalTally == 2)
        {
            return WinState.Win;
        }
        
        // If there are nine moves in the sequence, the game is over
        if(GetAvailableMoves().Count == 0)
        {
            return WinState.Tie;
        }

        return WinState.None;
    }

    // Simple static function to return the opposite of the BoardSpaceValue
    // Only AI and Player have opposites, eachother
    public static BoardSpaceValue FlipBoardSpaceValue(BoardSpaceValue val)
    {
        switch (val)
        {
            case BoardSpaceValue.AI:
                return BoardSpaceValue.Player;
            case BoardSpaceValue.Player:
                return BoardSpaceValue.AI;
            default:
                return val;
        }
    }
}
