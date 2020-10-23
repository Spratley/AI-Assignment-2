using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Jacob Pratley - 100653937
// October 22nd 2020

// This class is a middle man between the Unity animation system and the GameManager component
// The AI's turn is triggered in the animation, and it can't call functions on components that are not directly attached to the animated object or its children
// Since the GameManager is a pseudo singleton, we can call the AITurn() function from here easily
public class AIAnimatedMoveComponent : MonoBehaviour
{
    public void TriggerTurn()
    {
        GameManager.instance.AITurn();
    }
}
