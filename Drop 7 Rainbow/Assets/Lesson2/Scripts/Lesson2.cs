using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Lesson2
{
    public class Lesson2 : MonoBehaviour
    {
        private int[,] testArray = new int[,]
        {
            {-2, -2, 6, -2, 6, 6, -2},
            {7, 0, 6, 0, 7, 0, -2},
            {-2, 0, 6, 0, 6, 0, -2},
            {0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0},
            // {0, 0, 0, 0, 0, 0, 0},
        };

        public PlaygroundManager PlaygroundMgr;

        public DropNodeManager DropManager;
        
        private bool IfFinish;

        private int clickTimes;
        
        void Start()
        {
            IfFinish = false;
            clickTimes = 0;

            PlaygroundMgr.InitDetectArea();
            PlaygroundMgr.LoadData(testArray);
            DropManager = PlaygroundMgr.dropManager;
            PlaygroundMgr.ExecuteCommands(ifSuccess =>
            {
                PlaygroundMgr.CreateNewDrop(0.3f, 0);
                PlaygroundMgr.ExecuteCommands(null);
            });
        }
        
    }
}
