using System.Collections;
using System.Collections.Generic;
using Lesson2;
using UnityEngine;

namespace Lesson2
{
    public class PlaygroundManager : MonoBehaviour
    {
        public Transform DropRoot;
        public DropItem CopyOne;
    
        public DropNodeManager dropManager;
        private DropItem NewItem => dropManager.NewItem;
        
        /// <summary>
        /// 加载关卡
        /// </summary>
        public void LoadData(int[,] dataArray)
        {
            dropManager = new DropNodeManager {DropItemOne = CopyOne, DropRoot = DropRoot};
            dropManager.LoadData(dataArray);
        }

        /// <summary>
        /// 创建掉落物
        /// </summary>
        public void CreateNewDrop(float executeTime = 1f,float delayTime = 0)
        {
            dropManager.CreateDropItem(executeTime, delayTime);
        }
        
        public void ExecuteCommands()
        {
            dropManager.ExecuteCommands();
        }
    }
}
