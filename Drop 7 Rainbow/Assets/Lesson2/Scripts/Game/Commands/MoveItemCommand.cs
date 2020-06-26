using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Lesson2
{
    //Command to Move Item both in Horizontal or Vertical direction
    public sealed class MoveItemCommand : BaseGameCommand
    {
        public enum MoveDirection
        {
            eHorizontal,
            eVertical,
        }

        public bool CanBreak = false;
        public Vector3? BeginPos = null;
        public Vector3? EndPos = null;
        public Vector2Int? FromIndex = null;
        public Vector2Int? ToIndex = null;
        public MoveDirection Direction = MoveDirection.eVertical;

        public override void OnAppend()
        {
            if (ToIndex.HasValue)
            {
                Target.DropData.UpdatePosition(ToIndex.Value);
                if (FromIndex.HasValue)
                {
                    DropMgr.RemoveInDrop(FromIndex.Value);
                }
                DropMgr.AddToDrop(ToIndex.Value, Target);
            }

            if (!BeginPos.HasValue)
            {
                if (FromIndex.HasValue)
                {
                    BeginPos = DropItem.GetPositionByIndex(FromIndex.Value);
                }
                else
                {
                    Debug.LogError("MoveItemCmd == Should Set BeginPos!!!");
                }
            }

            if (!EndPos.HasValue)
            {
                if (ToIndex.HasValue)
                {
                    EndPos = DropItem.GetPositionByIndex(ToIndex.Value);
                }
                else
                {
                    Debug.LogError("MoveItemCmd == Should Set EndPos!!!");
                }
            }
        }

        public override void OnExecute()
        {
            DropMgr.DoMoveItemCommand(this);
        }
    }    
}
