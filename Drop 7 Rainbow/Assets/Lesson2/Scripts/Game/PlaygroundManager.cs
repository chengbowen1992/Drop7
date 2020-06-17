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
        
        public void LoadData(int[,] dataArray)
        {
            dropManager = new DropNodeManager {DropItemOne = CopyOne, DropRoot = DropRoot};
            dropManager.LoadData(dataArray);
        }

        public void CreateNewDrop(float excuteTime = 1f,float delayTime = 0)
        {
            dropManager.CreateDropItem(excuteTime, delayTime);
        }

        public void ExcuteCommands()
        {
            dropManager.ExcuteCommands();
        }
    }
}
