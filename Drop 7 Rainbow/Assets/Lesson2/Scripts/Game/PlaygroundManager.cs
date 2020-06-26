using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Lesson2
{
    public class PlaygroundManager : MonoBehaviour
    {
        public static readonly int DefaultIndexX = 3;
        public static readonly int OneTurnCount = 3;
        public Canvas Scaler;
        public Transform DropRoot;
        public DropItem CopyOne;
    
        public DropNodeManager dropManager;
        public CommandUtil commandMgr;
        
        private DropItem NewItem => dropManager.NewItem;
        private Rect[] detectRects;
        public int SelectIndex = DefaultIndexX;

        public int DropCount = 0;
        
        /// <summary>
        /// 创建 输入检测区域
        /// </summary>
        public void InitDetectArea()
        {
            int total = DropNodeManager.WIDTH;
            int cell = DropNodeManager.CELL_SIZE;
            int center = total / 2;
            Vector3 cenPos = DropRoot.transform.position;
            
            detectRects = new Rect[DropNodeManager.WIDTH];
            
            for (int i = 0; i < total; i++)
            {
                int deltaIndex = i - center;
                var centerPos = new Vector2(deltaIndex * cell * Scaler.scaleFactor + cenPos.x, cenPos.y);
                var rectSize = new Vector2(cell,cell * DropNodeManager.HEIGHT) * Scaler.scaleFactor;
                var rectPos = centerPos - rectSize * 0.5f;
                detectRects[i] = new Rect(rectPos, rectSize);
            }
        }

        /// <summary>
        /// 加载关卡
        /// </summary>
        public void LoadData(int[,] dataArray)
        {
            commandMgr = CommandUtil.Instance;
            dropManager = new DropNodeManager {DropItemOne = CopyOne, DropRoot = DropRoot};
            BaseGameCommand.DropMgr = dropManager;
            dropManager.LoadData(dataArray, _ =>
            {
                CreateNewDrop(null,0.3f, 0);
            });
            DropCount = 0;
        }

        /// <summary>
        /// 创建掉落物
        /// </summary>
        public void CreateNewDrop(Action<bool> onComplete, float executeTime = 1f, float delayTime = 0)
        {
            SelectIndex = DefaultIndexX;
            dropManager.CreateDropItem(executeTime, delayTime, onComplete);
        }

        #region 调试

        private void Update()
        {
            int count = detectRects?.Length ?? 0;
            
            for (int i = 0; i < count; i++)
            {
                if (detectRects[i].Contains(Input.mousePosition))
                {
                    var newItem = dropManager.NewItem;
                    if (newItem != null && newItem.IfReady)
                    {
                        if (Input.GetMouseButtonUp(0))
                        {
                            dropManager.DropDropItem(i, onDropComplete =>
                            {
                                if (DropCount % OneTurnCount == 0)
                                {
                                    dropManager.AddBottomLine(1 ,onAddLineComplete=>
                                    {
                                        CreateNewDrop(null,0.3f, 0);
                                    });
                                }
                                else
                                {
                                    CreateNewDrop(null,0.3f, 0);
                                }
                            });

                            DropCount++;
                            SelectIndex = i;
                            return;
                        }
                        else if (Input.GetMouseButton(0))
                        {
                            if (SelectIndex != i)
                            {
                                dropManager.MoveDropItem(SelectIndex, i);
                                SelectIndex = i;
                            }

                        }
                    }
                }
            }
        }

        private void OnDrawGizmos()
        {
            bool haveInput = Input.GetMouseButton(0);
            var inputPos = Input.mousePosition;
            int count = detectRects?.Length ?? 0;
            
            for (int i = 0; i < count; i++)
            {
                if (haveInput && detectRects[i].Contains(inputPos))
                {
                    Gizmos.color = Color.red;
                }
                else
                {
                    Gizmos.color = Color.blue;
                }

                DrawRect(detectRects[i]);
            }
        }

        private void DrawRect(Rect rect)
        {
            Vector3[] rectPoints = new[]
            {
                new Vector3(rect.xMin, rect.yMin),
                new Vector3(rect.xMin, rect.yMax),
                new Vector3(rect.xMax, rect.yMax),
                new Vector3(rect.xMax, rect.yMin),
            };

            int count = rectPoints.Length;

            for (int i = 0; i < count; i++)
            {
                Gizmos.DrawLine(rectPoints[i],rectPoints[(i+1) % count]);
            }
        }

        #endregion

    }
}
