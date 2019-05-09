using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays the statistics
/// </summary>
public class StatisticsDisplay : MonoBehaviour
{
    [SerializeField]
    Text easyEasyPlayer1Wins;
    [SerializeField]
    Text easyEasyPlayer2Wins;

    [SerializeField]
    Text mediumMediumPlayer1Wins;
    [SerializeField]
    Text mediumMediumPlayer2Wins;

    [SerializeField]
    Text hardHardPlayer1Wins;
    [SerializeField]
    Text hardHardPlayer2Wins;

    [SerializeField]
    Text easyMediumPlayer1Wins;
    [SerializeField]
    Text easyMediumPlayer2Wins;

    [SerializeField]
    Text easyHardPlayer1Wins;
    [SerializeField]
    Text easyHardPlayer2Wins;

    [SerializeField]
    Text mediumHardPlayer1Wins;
    [SerializeField]
    Text mediumHardPlayer2Wins;

    /// <summary>
	/// Use this for initialization
	/// </summary>
	void Start()
	{
        for (int i = 0; i < 7; i++)
            Debug.Log("for case "+ (i+1)+ "::::::" + "player 1 = " + Statistics.GetScore(i, 0) + "   player 2 = " + Statistics.GetScore(i, 1));

        easyEasyPlayer1Wins.text = ""+Statistics.GetScore(0,0);
        easyEasyPlayer2Wins.text = "" + Statistics.GetScore(0, 1);
        mediumMediumPlayer1Wins.text = "" + Statistics.GetScore(1, 0);
        mediumMediumPlayer2Wins.text = "" + Statistics.GetScore(1,1);
        hardHardPlayer1Wins.text = "" + Statistics.GetScore(2,0);
        hardHardPlayer2Wins.text = "" + Statistics.GetScore(2,1);
        easyMediumPlayer1Wins.text = "" + Statistics.GetScore(3,0);
        easyMediumPlayer2Wins.text = "" + Statistics.GetScore(3,1);
        easyHardPlayer1Wins.text = "" + Statistics.GetScore(4,0);
        easyHardPlayer2Wins.text = "" + Statistics.GetScore(4,1);
        mediumHardPlayer1Wins.text = "" + Statistics.GetScore(5,0);
        mediumHardPlayer2Wins.text = "" + Statistics.GetScore(5,1);

    }

}
