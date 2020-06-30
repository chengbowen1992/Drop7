using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lesson2
{
    //Base Command of the Game Behaviour
    public class BaseGameCommand : BaseCommand
    {
        public override string Description => "BaseCommand of Game";
        public static DropNodeManager DropMgr { get; set; }

        public static ScoreManager ScoreMgr { get; set; }

        public DropItem Target { get; set; }
        public float DelayTime { get; set; }
        public float ExecuteTime { get; set; }
    }    
}
