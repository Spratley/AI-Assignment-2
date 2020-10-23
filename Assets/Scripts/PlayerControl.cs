using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Jacob Pratley - 100653937    
// October 21st 2020

public class PlayerControl : MonoBehaviour
{
    // Limiting the magnitude of the rotation will lock it to a specific angle in an easy way
    public float rotationMagnitudeMax;
    // Stores the rotation of the camera in degrees away from a rotation of (0, 0, 0)
    Vector2 rotation;

    // A reference to the object that is currently being looked at by the player
    GameObject selectedSpace;

    // Rotate the player camera based on delta mouse movement
    // Select the space that the player is looking at
    // If they press the mouse button, make them thake their turn
    void Update()
    {
        rotation.x -= Input.GetAxis("Mouse Y");
        rotation.y += Input.GetAxis("Mouse X");

        rotation = Vector2.ClampMagnitude(rotation, rotationMagnitudeMax);

        transform.rotation = Quaternion.Euler(rotation);

        SelectSpace();

        if(Input.GetMouseButtonDown(0))
        {
            CommitSpace();
        }
    }

    // Casts a ray out into the scene that will only interact with the GameGrid layer (where the game pieces are)
    // If it hits a collider, that collider's GameObject is selected
    // If not, the selected space is null
    public void SelectSpace()
    {
        RaycastHit hit;
        LayerMask mask = 1 << LayerMask.NameToLayer("GameGrid");

        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
        {
            selectedSpace = hit.collider.gameObject;
        }
        else
        {
            selectedSpace = null;
        }
    }

    // If we have a selected space, then attempt to make a move at the selected space
    // Since the PlayerTurn function will only allow valid moves, it will remain the player's turn until they CommitSpace() with a valid move
    // The position is determined by converting the selected object's name to a position
    // This implementation assumes that the grid is properly named, TL, TM, TR, ML, MM, MR, BL, BM, BR
    public void CommitSpace()
    {
        if(selectedSpace != null)
        {
            GameManager.instance.PlayerTurn(NameToPosition(selectedSpace.name));
        }
    }

    // Converts a GameObject name to a position
    // This implementation assumes that the grid is properly named, TL, TM, TR, ML, MM, MR, BL, BM, BR
    public Vector2Int NameToPosition(string name)
    {
        Debug.Assert(name.Length == 2, "Incorrect name length: " + name);

        Vector2Int position = new Vector2Int(-1, -1);

        // The Y value => vertical position, Top, Middle, Bottom
        switch(name[0])
        {
            case 'T':
                position.y = 0;
                break;
            case 'M':
                position.y = 1;
                break;
            case 'B':
                position.y = 2;
                break;
        }

        // The X value => horizontal position, Left, Middle, Right
        switch (name[1])
        {
            case 'L':
                position.x = 0;
                break;
            case 'M':
                position.x = 1;
                break;
            case 'R':
                position.x = 2;
                break;
        }

        // -1 is our default value
        // If any component is -1, then the name was not valid for translation
        if(position.x == -1 || position.y == -1)
        {
            Debug.LogError("Incorrect name given: " + name);
        }

        return position;
    }
}
