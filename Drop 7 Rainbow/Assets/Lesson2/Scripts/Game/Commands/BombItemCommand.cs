using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lesson2
{
    //Command to Bomb Item or Bombed Item
    public sealed class BombItemCommand : BaseGameCommand
    {
        public int? NewValue;

        public override void OnAppend()
        {
            if (!NewValue.HasValue)
            {
                DropMgr.RemoveInDrop(Target.DropData.Position);
            }
        }

        public override void OnExecute()
        {
            DropMgr.DoBombItemCommand(this);
        }
    }    
}
