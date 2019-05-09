using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A player
/// </summary>
public class Player : MonoBehaviour
{
    PlayerName myName;
    Timer thinkingTimer;

    // minimax search support
    Difficulty difficulty;
    int searchDepth = 0;
    MinimaxTree<Configuration> tree;

    // events invoked by class
    TurnOver turnOverEvent = new TurnOver();

    // saved for efficiency
    LinkedList<MinimaxTreeNode<Configuration>> nodeList =
        new LinkedList<MinimaxTreeNode<Configuration>>();
    List<int> binContents = new List<int>();
    List<Configuration> newConfigurations =
        new List<Configuration>();

    /// <summary>
    /// Awake is called before Start
    /// </summary>
    void Awake()
	{
        // set name
		if (CompareTag("Player1"))
        {
            myName = PlayerName.Player1;
        }
        else
        {
            myName = PlayerName.Player2;
        }

        // add timer component
        thinkingTimer = gameObject.AddComponent<Timer>();
        thinkingTimer.Duration = GameConstants.AiThinkSeconds;
        thinkingTimer.AddTimerFinishedListener(HandleThinkingTimerFinished);

        // register as invoker and listener
        EventManager.AddTurnOverInvoker(this);
        EventManager.AddTakeTurnListener(HandleTakeTurnEvent);
	}
	
	/// <summary>
	/// Update is called once per frame
	/// </summary>
	void Update()
	{
		
	}

    /// <summary>
    /// Gets and sets the difficulty for the player
    /// </summary>
    public Difficulty Difficulty
    {
        get { return difficulty; }
        set
        {
            difficulty = value;
            switch (difficulty)
            {
                case Difficulty.Easy:
                    searchDepth = GameConstants.EasyMinimaxDepth;
                    break;
                case Difficulty.Medium:
                    searchDepth = GameConstants.MediumMinimaxDepth;
                    break;
                case Difficulty.Hard:
                    searchDepth = GameConstants.HardMinimaxDepth;
                    break;
                default:
                    searchDepth = GameConstants.EasyMinimaxDepth;
                    break;
            }
        }
    }

    /// <summary>
    /// Adds the given listener for the TurnOver event
    /// </summary>
    /// <param name="listener">listener</param>
    public void AddTurnOverListener(
        UnityAction<PlayerName, Configuration> listener)
    {
        turnOverEvent.AddListener(listener);
    }

    /// <summary>
    /// Handles the TakeTurn event
    /// </summary>
    /// <param name="player">whose turn it is</param>
    /// <param name="boardConfiguration">current board configuration</param>
    void HandleTakeTurnEvent(PlayerName player,
        Configuration boardConfiguration)
    {
        // only take turn if it's our turn
        if (player == myName)
        {
            tree = BuildTree(boardConfiguration);
            thinkingTimer.Run();
        }
    }

    /// <summary>
    /// Builds the tree
    /// </summary>
    /// <param name="boardConfiguration">current board configuration</param>
    /// <returns>tree</returns>
    MinimaxTree<Configuration> BuildTree(
        Configuration boardConfiguration)
    {
        // build tree to appropriate depth
        MinimaxTree<Configuration> tree =
            new MinimaxTree<Configuration>(boardConfiguration);
        nodeList.Clear();
        nodeList.AddLast(tree.Root);
        while (nodeList.Count > 0)
        {
            MinimaxTreeNode<Configuration> currentNode =
                nodeList.First.Value;
            nodeList.RemoveFirst();
            List<Configuration> children =
                GetNextConfigurations(currentNode.Value);
            foreach (Configuration child in children)
            {
                // STUDENTS: only add to tree if within search depth

                MinimaxTreeNode<Configuration> childNode =
                    new MinimaxTreeNode<Configuration>(
                        child, currentNode);
                if (Level(childNode) <= searchDepth)
                {
                    tree.AddNode(childNode);
                    nodeList.AddLast(childNode);

                }
            }
        }
        return tree;
    }

    /// <summary>
    /// Handles the thinking timer finishing
    /// </summary>
    void HandleThinkingTimerFinished()
    {
        // do the search and pick the move
        Minimax(tree.Root, true);

        // find child node with maximum score
        IList<MinimaxTreeNode<Configuration>> children =
            tree.Root.Children;
        MinimaxTreeNode<Configuration> maxChildNode = children[0];
        for (int i = 1; i < children.Count; i++)
        {
            if (children[i].MinimaxScore > maxChildNode.MinimaxScore)
            {
                maxChildNode = children[i];
            }
        }

        // provide new configuration as second argument
        turnOverEvent.Invoke(myName, maxChildNode.Value);
    }

    /// <summary>
    /// Gets a list of the possible next configurations
    /// given the current configuration
    /// </summary>
    /// <param name="currentConfiguration">current configuration</param>
    /// <returns>list of next configurations</returns>
    List<Configuration> GetNextConfigurations(
        Configuration currentConfiguration)
    {
        newConfigurations.Clear();
        IList<int> currentBins = currentConfiguration.Bins;
        for (int i = 0; i < currentBins.Count; i++)
        {
            int currentBinCount = currentBins[i];
            while (currentBinCount > 0)
            {
                // take one teddy from current bin
                currentBinCount--;

                // add new next configuration to list
                binContents.Clear();
                binContents.AddRange(currentBins);
                binContents[i] = currentBinCount;
                newConfigurations.Add(
                    new Configuration(binContents));
            }
        }
        return newConfigurations;
    }

    /// <summary>
    /// Assigns minimax scores to the tree nodes
    /// </summary>
    /// <param name="tree">tree to mark with scores</param>
    /// <param name="maximizing">whether or not we're maximizing</param>
    void Minimax(MinimaxTreeNode<Configuration> tree,
        bool maximizing)
    {
        // recurse on children
        IList<MinimaxTreeNode<Configuration>> children = tree.Children;
        if (children.Count > 0)
        {
            foreach (MinimaxTreeNode<Configuration> child in children)
            {
                // toggle maximizing as we move down
                Minimax(child, !maximizing);
            }

            // set default node minimax score
            if (maximizing)
            {
                tree.MinimaxScore = int.MinValue;
            }
            else
            {
                tree.MinimaxScore = int.MaxValue;
            }

            // find maximum or minimum value in children
            foreach (MinimaxTreeNode<Configuration> child in children)
            {
                if (maximizing)
                {
                    // check for higher minimax score
                    if (child.MinimaxScore > tree.MinimaxScore)
                    {
                        tree.MinimaxScore = child.MinimaxScore;
                    }
                }
                else
                {
                    // minimizing, check for lower minimax score
                    if (child.MinimaxScore < tree.MinimaxScore)
                    {
                        tree.MinimaxScore = child.MinimaxScore;
                    }
                }
            }
        }
        else
        {
            // leaf nodes are the base case
            AssignHeuristicMinimaxScore(tree, maximizing);
        }
    }

    /// <summary>
    /// Assigns the end of game minimax score
    /// </summary>
    /// <param name="node">node to mark with score</param> 
    /// <param name="maximizing">whether or not we're maximizing</param>
    void AssignEndOfGameMinimaxScore(MinimaxTreeNode<Configuration> node,
        bool maximizing)
    {
        if (maximizing)
        {
            // other player took the last teddy
            node.MinimaxScore = 1;
        }
        else
        {
            // we took the last teddy
            node.MinimaxScore = 0;
        }
    }
        
    /// <summary>
    /// Assigns a heuristic minimax score to the given node
    /// </summary>
    /// <param name="node">node to mark with score</param>
    /// <param name="maximizing">whether or not we're maximizing</param>
    void AssignHeuristicMinimaxScore(
        MinimaxTreeNode<Configuration> node,
        bool maximizing)
    {
        // might have reached an end-of-game configuration
        if (node.Value.Empty)
        {
            AssignEndOfGameMinimaxScore(node, maximizing);
        }
        else
        {

            // use a heuristic evaluation function to score the node
            if (node.Value.TotalNonEmpty == 1)
            {
                if (node.Value.TotalBears != 1)
                {
                    if (maximizing)
                        node.MinimaxScore = 1;  //because he leave only on bear and takes the rest, thus he wins cuz other will have to take that bear
                    else
                        node.MinimaxScore = 0;
                }
                else
                {
                    if (maximizing)
                        node.MinimaxScore = 0; // beacuse we lose when we take this 
                    else
                        node.MinimaxScore = 1;
                }
            }
            else if (node.Value.TotalNonEmpty == 2)
            {
                if (node.Value.TotalBears == 2 || node.Value.TotalBears == 3) // case 2 : we can take 1 teddy and leave 1 for other player to pick up, case 3: we can take 2 teddies from one box and that will leave only 1 teddy in other box
                {
                    if (maximizing)
                        node.MinimaxScore = 1;
                    else
                        node.MinimaxScore = 0;
                }
                else if (node.Value.TotalBears == 4) // it could be 2 in each or 3 in one an 1 in other
                {
                    if (node.Value.Bins[0] == 2) // one bin contains 2 bear. . ie we loose. . if we take 1 teddy. other player will take 2 from other box and win. if we take 2 , other player will take 1 from other box and win
                    {
                        if (maximizing)
                            node.MinimaxScore = 0;
                        else
                            node.MinimaxScore = 1;
                    }
                    else // one bin contain 1 and other 3. . . we take 3 and thus win cuz only one left
                    {
                        if (maximizing)
                            node.MinimaxScore = 1;
                        else
                            node.MinimaxScore = 0;
                    }
                }

            }
            else if (node.Value.TotalNonEmpty == 3)
            {
                if (node.Value.TotalBears == 3) // we take one , other player take one , then we take last
                {
                    if (maximizing)
                        node.MinimaxScore = 0;
                    else
                        node.MinimaxScore = 1;
                }
                else if (node.Value.TotalBears == 4) // we take from one from box that contains 2 and thus create previous case
                {
                    if (maximizing)
                        node.MinimaxScore = 1;
                    else
                        node.MinimaxScore = 0;
                }
                else if (node.Value.TotalBears == 5) // 2 case. . 2 in 2 and 1 in 3rd or 1 in 2 and 3 in 3rd
                {
                    if (maximizing)                 // we win in both cases. ..think about it. .
                        node.MinimaxScore = 1;
                    else
                        node.MinimaxScore = 0;
                }
                
            }
            else        /// cant create all the scenario. .. . XD Assignment told me to create just 2
                node.MinimaxScore = 0.5f;
		}
    }

    int Level (MinimaxTreeNode<Configuration> node)
    {
        int lev = 0;
        while (node.Parent != null)
        {
            lev++;
            node = node.Parent;
        }
        return lev;
    }
}
