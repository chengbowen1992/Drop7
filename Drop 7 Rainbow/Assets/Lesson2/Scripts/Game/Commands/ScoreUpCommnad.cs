using System.Collections;
using System.Collections.Generic;
using Lesson2;
using UnityEngine;

namespace Lesson2
{
    public class ScoreUpCommnad : BaseGameCommand
    {
        public int ScoreAppend;
        public int TurnCount;
        public int BombNum;

        public override void OnExecute()
        {
            ScoreMgr.AppendScore(ScoreAppend * BombNum,TurnCount);
            this.OnComplete(true);
        }
    }
}
