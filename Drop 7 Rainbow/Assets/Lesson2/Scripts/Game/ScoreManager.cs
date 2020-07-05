using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lesson2
{
    public sealed class ScoreManager
    {
        public static ScoreManager Instance = new ScoreManager();

        public Action<int> OnScoreChanged;
        public Action<int> OnLevelChanged;
        public Action<int> OnBestChangeed;
        
        public Action<int> OnBombTurnChanged;

        public int Score { get; private set; } = 0;

        public int Turn { get; private set; } = 0;

        public int Level { get; private set; } = 1;

        public int Best {
            get { return LocalSaveManager.BestScore; }
        }

        public void AppendScore(int scoreAppend,int turn)
        {
            Score += scoreAppend;
            
            OnScoreChanged?.Invoke(Score);
            
            if (turn > Turn)
            {
                Turn = turn;

                OnBombTurnChanged?.Invoke(Turn);
            }
            
            Debug.Log($"Score:{Score}");
        }

        public void AppendLevel()
        {
            Level++;
            OnLevelChanged?.Invoke(Level);
        }

        public void SetScore(int score)
        {
            if (Score != score)
            {
                Score = score;
                OnScoreChanged?.Invoke(score);
            }
        }

        public void SetLevel(int level)
        {
            if (level != Level)
            {
                Level = level;
                OnLevelChanged?.Invoke(Level);
            }
        }

        public void SetBest(int best)
        {
            if (Best < best)
            {
                LocalSaveManager.BestScore = best;
                OnBestChangeed?.Invoke(Best);
            }
        }

        public void ResetManager()
        {
            Score = 0;
            Turn = 0;
            Level = 1;
            //Best Auto Change
            
            OnScoreChanged?.Invoke(Score);
            OnLevelChanged?.Invoke(Level);
            OnBombTurnChanged?.Invoke(Turn);
            OnBestChangeed?.Invoke(Best);
        }
    }
}
