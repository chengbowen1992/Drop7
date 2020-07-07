using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lesson2
{
    //command to call finish
    public class GameFinishCommand : BaseGameCommand
    {
        public int Level;
        public int Score;
        public int BestScore;

        public override void OnAppend()
        {
            
        }

        public override void OnExecute()
        {
            DropMgr.OnGameFinished?.Invoke(false);
            OnComplete(true);
        }
    }
}