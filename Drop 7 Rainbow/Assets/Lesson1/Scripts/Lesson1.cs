using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Lesson1
{
    public class Lesson1 : MonoBehaviour
    {
        private int[,] testArray = new int[,]
        {
        {-2,-2,6,-2,6,6,-2},
        {7,0,6,0,7,0,-2},
        {-2,0,6,0,6,0,-2},
        {0,0,0,0,0,0,0},
        {0,0,0,0,0,0,0},
        {0,0,0,0,0,0,0},
        {0,0,0,0,0,0,0},
        {0,0,0,0,0,0,0},
        };

        private DropNodeManager dropNodeManager;

        private string mapBefore, mapAfter, dropInfo;

        private GUIStyle fontStyle;

        void Start()
        {
            fontStyle = new GUIStyle();
            fontStyle.fontSize = 30;

            dropNodeManager = new DropNodeManager();
            dropNodeManager.LoadData(testArray);
            mapBefore = dropInfo = "";
            mapAfter = dropNodeManager.GetDebugInfo(DropNodeManager.DebugInfoType.eOriginMap);

            ShowDebugInfo();
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                int randomX = Random.Range(0, DropNodeManager.WIDTH);
                int randomNum = Random.Range(-2, DropNodeManager.WIDTH);
                if (randomNum != 0)
                {
                    mapBefore = dropNodeManager.GetDebugInfo(DropNodeManager.DebugInfoType.eOriginMap);

                    dropInfo = $"x=> {randomX}  val=>{randomNum}";

                    bool ifFinish = !dropNodeManager.CanDropNode(randomX);
                    if (!ifFinish)
                    {
                        Debug.Log($"Drop Num:{randomNum} to Col{randomX}");
                        dropNodeManager.TryDropNode(randomX, randomNum);

                        mapAfter = dropNodeManager.GetDebugInfo(DropNodeManager.DebugInfoType.eOriginMap);

                        ShowDebugInfo();
                    }
                    else
                    {
                        Debug.Log("Finish");
                    }
                }
            }
        }

        void ShowDebugInfo() 
        {
            var debugCount = (int)DropNodeManager.DebugInfoType.eAll;
            for (int i = 0; i < debugCount; i++)
            {
                Debug.Log(dropNodeManager.GetDebugInfo((DropNodeManager.DebugInfoType)i));
            }
        }

        private void OnGUI()
        {
            int fontSize = fontStyle.fontSize;

            GUI.TextArea(new Rect(0, 0, fontSize * 8, fontSize * 10), mapBefore, fontStyle);
            GUI.TextArea(new Rect(0, fontSize * 12, fontSize * 8, fontSize * 10), dropInfo, fontStyle);
            GUI.TextArea(new Rect(0, fontSize * 14, fontSize * 8, fontSize * 10), mapAfter, fontStyle);
        }
    }
}
