using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Lesson2
{
    public abstract class CommandBase
    {
        public DropNodeManager DropMgr;
        public DropItem Target;
        public Vector2Int TargetIndex;
        public float ExecuteTime;
        public float DelayTime;

        private Action<CommandBase,bool> onComplete;

        public virtual void OnAppend(){}

        public void Execute(Action<CommandBase, bool> onFinish = null)
        {
            onComplete = onFinish;
            OnExecute();
        }

        protected virtual void OnExecute(){ }

        public abstract void Undo();

        public virtual void OnComplete(bool ifSuccess = true)
        {
            onComplete?.Invoke(this, ifSuccess);
        }
    }

    public enum CreateItemType
    {
        eLoad,
        eDrop,
        eBottom,
    }
    
    /// <summary>
    /// 创建命令
    /// </summary>
    public class CreateCommand : CommandBase
    {
        public CreateItemType CreateType;
        public Vector2Int Index;
        public Vector3? Position;

        public int Val;
        
        protected override void OnExecute()
        {
            DropMgr.CreateItem(this);
        }

        public override void Undo()
        {
            //TODO
        }
    }

    /// <summary>
    /// 移动命令
    /// 直接移动到位置
    /// </summary>
    public class SetPositionCommand : CommandBase
    {
        public Vector3 Position;
        
        protected override void OnExecute()
        {
            DropMgr.SetItemPos(this);
        }

        public override void Undo()
        {
            throw new System.NotImplementedException();
        }
    }

    /// <summary>
    /// 掉落命令
    /// </summary>
    public class DropCommand : CommandBase
    {
        protected override void OnExecute()
        {
            throw new System.NotImplementedException();
        }

        public override void Undo()
        {
            throw new System.NotImplementedException();
        }
    }

    /// <summary>
    /// 移动命令
    /// </summary>
    public class MoveCommand : CommandBase
    {
        public Vector3 BeginPos;
        public Vector3 EndPos;
        public bool CanBreak;
        public Vector2Int? EndIndex;
        
        public AnimationCurve MoveCurve;

        public override void OnAppend()
        {
            if (Target != null)
            {
                if (Target.DropData.Position.x < 0)
                {
                    //TODO
                    if (EndIndex.HasValue)
                    {
                        var newIndex = EndIndex.Value;
                        
                        Target.DropData.UpdatePosition(newIndex);
                        DropMgr.DropDictionary.Add(newIndex, Target);
                
#if UNITY_EDITOR
                        Debug.Log($"MoveItem == Add Node {newIndex}"); 
#endif
                    }
                }
            }
        }

        protected override void OnExecute()
        {
            DropMgr.MoveItem(this);
        }

        public override void Undo()
        {
            throw new System.NotImplementedException();
        }
    }

    /// <summary>
    /// 爆炸命令
    /// </summary>
    public class BombCommand : CommandBase
    {
        public override void OnAppend()
        {
            Target = DropMgr.DropDictionary[TargetIndex];
            DropMgr.DropDictionary.Remove(TargetIndex);
        }

        protected override void OnExecute()
        {
            DropMgr.BombItem(this);
        }

        public override void Undo()
        {
            throw new System.NotImplementedException();
        }
    }

    public class BombedMoveCommand : MoveCommand
    {
        public Vector2Int FromIndex;
        public Vector2Int ToIndex;

        public override void OnAppend()
        {
            var dropDic = DropMgr.DropDictionary;
            dropDic.Remove(FromIndex);
            Target.DropData.UpdatePosition(ToIndex);
            //Replace Bomb
            dropDic[ToIndex] = Target;

            BeginPos = DropItem.GetPositionByIndex(FromIndex);
            EndPos = DropItem.GetPositionByIndex(ToIndex);

#if UNITY_EDITOR
            Debug.Log($"MoveItem == Move Node to {ToIndex} =< from {FromIndex}"); 
#endif
        }

        protected override void OnExecute()
        {
            DropMgr.MoveBombedItem(this);
        }

        public override void Undo()
        {
            throw new System.NotImplementedException();
        }
    }

    /// <summary>
    /// 被爆炸波及命令
    /// </summary>
    public class BombedCommand : CommandBase
    {
        protected override void OnExecute()
        {
            OnComplete(true);
        }

        public override void Undo()
        {
            throw new System.NotImplementedException();
        }
    }
}