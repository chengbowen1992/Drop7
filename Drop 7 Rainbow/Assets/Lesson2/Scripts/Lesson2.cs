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

        private bool IfFinish;

        private int clickTimes;
        
        void Start()
        {
            IfFinish = false;
            clickTimes = 0;

            PlaygroundMgr.InitDetectArea();
            PlaygroundMgr.LoadData(testArray);
            PlaygroundMgr.ExecuteCommands(ifSuccess =>
            {
                PlaygroundMgr.CreateNewDrop(0.3f, 0);
                PlaygroundMgr.ExecuteCommands(null);
            });
        }

        void Update()
        {
            /*
            if (IfFinish)
            {
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (clickTimes % 10 == 9)
                {
                    var lineHeight = 1; //clickTimes / 10 + 1;
                    Debug.Log($"Add Line Num:{lineHeight}");
                    IfFinish = !dropNodeManager.AddBottomLine(lineHeight);
                }
                else
                {
                    int randomX = Random.Range(0, DropNodeManager.WIDTH);
                    int randomNum = Random.Range(-2, DropNodeManager.WIDTH);
                    if (randomNum != 0)
                    {
                        IfFinish = !dropNodeManager.CanDropNode(randomX);
                        if (!IfFinish)
                        {
                            Debug.Log($"Drop Num:{randomNum} to Col{randomX}");
                            dropNodeManager.TryDropNode(randomX, randomNum);
                        }
                        else
                        {
                            Debug.Log("Finish");
                        }
                    }   
                }

                clickTimes++;
            }
            */
        }
    }
}
