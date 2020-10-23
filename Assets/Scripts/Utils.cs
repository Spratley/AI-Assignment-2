using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Jacob Pratley - 100653937    
// October 22nd 2020

public class Utils
{
    // Converts a Vector2Int coordinate to an int linear coordinate
    // (0, 0) => 0, (0, 1) => 1, (0, 2) => 2, (1, 0) => 3 ... (1, 1) => 8
    // This is just here cause I couldn't find a better place for it
    public static int FlattenVector2Int(Vector2Int val, int width)
    {
        return val.x + val.y * width;
    }
}
