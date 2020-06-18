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
        
        public Vector2Int? EndIndex;
        
        public AnimationCurve MoveCurve;

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
    /// 被爆炸波及命令
    /// </summary>
    public class BombedCommand : CommandBase
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
}