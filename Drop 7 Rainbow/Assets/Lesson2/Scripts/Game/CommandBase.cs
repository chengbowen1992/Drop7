using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lesson2
{
    public abstract class CommandBase
    {
        public DropNodeManager DropMgr;
        public DropNode Target;
        
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
        public Vector2Int Pos;
        public int Val;
        
        public override void Excute()
        {
            if (CreateType == CreateItemType.eLoad)
            {
                var node = DropMgr.CreateNode(Pos, Val);
                var item = DropMgr.CreateItem(node);

                DropMgr.DropDictionary.Add(Pos, item);
            }
        }

        public override void Undo()
        {
            //TODO
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