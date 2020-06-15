using System.Collections;
using System.Collections.Generic;
using Lesson2;
using UnityEngine;

namespace Lesson2
{
    public class PlaygroundManager : MonoBehaviour
    {
        public DropItem CopyOne;
    
        public DropNodeManager dropManager;
        
        public void LoadData(int[,] dataArray)
        {
            dropManager = new DropNodeManager();
            dropManager.LoadData(dataArray);
            
            
        }
    }
}
