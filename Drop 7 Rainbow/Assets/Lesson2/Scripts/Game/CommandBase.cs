using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lesson2
{
    public abstract class CommandBase
    {
        public DropNodeManager DropMgr;
        public DropItem Target;
        public Vector2Int TargetIndex;
        public float ExcuteTime;
        public float DelayTime;
        
        public abstract void Excute();
        public abstract void Undo();
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
        
        public override void Excute()
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
        
        public override void Excute()
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
        public override void Excute()
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

        public override void Excute()
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
        public override void Excute()
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
        public override void Excute()
        {
            throw new System.NotImplementedException();
        }

        public override void Undo()
        {
            throw new System.NotImplementedException();
        }
    }
}