using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lesson2
{
    public sealed class ScoreManager
    {
        public static ScoreManager Instance = new ScoreManager();

        public Action<int> OnScoreAppend;

        public Action<int> OnBombTurnChanged;
        
        public int Score { get; private set; }

        public int Turn { get; private set; }

        public void AppendScore(int scoreAppend,int turn)
        {
            Score += scoreAppend;
            
            OnScoreAppend?.Invoke(Score);
            
            if (turn != Turn)
            {
                Turn = turn;

                OnBombTurnChanged?.Invoke(Turn);
            }
            
            Debug.Log($"Score:{Score}");
        }
    }
}
