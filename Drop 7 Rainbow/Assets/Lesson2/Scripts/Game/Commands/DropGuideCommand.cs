using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Lesson2
{
    public class DropGuideCommand : BaseGameCommand
    {
        public Vector3 Position;
        public bool IfDropNode;
        
        public override void OnAppend()
        {
            
        }

        public override void OnExecute()
        {
            DropMgr.DoCreateGuideCommand(this);
        }
    }
}
