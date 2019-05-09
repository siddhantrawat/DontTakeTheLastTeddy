using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Statistics
{

    static int[,] score = new int[7,2];

    public static int GetScore(int a, int b)
    {
         return score[a,b]; 
    }

    public static void UpdateScore(int a, int b)
    {
        score[a,b]++;
    }
}
