using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lesson2
{
    public enum CreateItemType
    {
        eLoad,
        eDrop,
        eBottom,
    }
    
    //Command to Create New Item when load , drop and create from bottom
    public sealed class CreateItemCommand : BaseGameCommand
    {
        public override string Description => $"Create Item Command with Type {CreateType}";
        
        public CreateItemType CreateType = CreateItemType.eLoad;
        
        public Vector2Int Index = Vector2Int.zero;
        
        public int Value = 0;    

        public Vector3? Position = null;

        public override void OnAppend()
        {
            DropMgr.DoAppendCreateItemCommand(this);
        }

        public override void OnExecute()
        {
            DropMgr.DoExecuteCreateItemCommand(this);
        }
    }
}
