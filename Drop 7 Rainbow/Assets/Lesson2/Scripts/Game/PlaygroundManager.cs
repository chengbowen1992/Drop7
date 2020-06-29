using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace Lesson2
{
    public class PlaygroundManager : MonoBehaviour
    {
        public static readonly int DefaultIndexX = 3;
        public static readonly int MaxTurnCount = 30;
        public static readonly int MinTuneCount = 5;

        public Canvas Scaler;

        public AudioSource MusicPlayer;
        public AudioSource SoundPlayer;
        
        public Transform DropRoot;
        public DropItem CopyOne;

        public LevelTitleManager titleManager;

        public Random randomMgr;
        public DropNodeManager dropManager;
        public CommandUtil commandMgr;
        public LevelCreatorBase levelCreator;
        
        private DropItem NewItem => dropManager.NewItem;
        private Rect[] detectRects;
        public int SelectIndex = DefaultIndexX;

        public int DropCount = 0;

        //初始化音乐管理器
        public void InitSoundManager()
        {
            SoundManager.Instance.Init(MusicPlayer, SoundPlayer);
        }

        //播放背景音乐
        public void StartPlayMusic()
        {
            SoundManager.Instance.PlayMusic(SoundNames.Music_GameBg);
        }

        // 初始化 标题 UI
        public void InitTitle()
        {
            titleManager.CreateTitle(MaxTurnCount, MinTuneCount);
        }

        // 创建 输入检测区域
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

        // 加载关卡
        public void InitLevel()
        {
            randomMgr = new Random(DateTime.Now.Millisecond);
            commandMgr = CommandUtil.Instance;
            dropManager = new DropNodeManager {DropItemOne = CopyOne, DropRoot = DropRoot};
            BaseGameCommand.DropMgr = dropManager;
            levelCreator = LevelCreatorBase.Instance;
            levelCreator.DropMgr = dropManager;

            WeightRandom levelRandom = new WeightRandom(randomMgr, "10:1|20:2|20:3|20:4|20:5|20:6|15:7|20:-1|20:-2");
            levelCreator.LevelRandom = levelRandom;
            WeightRandom bombRandom = new WeightRandom(randomMgr, "10:1|20:2|20:3|20:4|20:5|20:6|15:7");
            levelCreator.BombRandom = bombRandom;
            
            dropManager.levelCreator = levelCreator;
            
            WeightRandom createRandom = new WeightRandom(randomMgr, "20:2|30:3|20:4|15:5|5:6|5:7|2:1|10:-1|15:-2");
            var dataArray = levelCreator.CreateLevel(DropNodeManager.WIDTH, DropNodeManager.HEIGHT, 21, createRandom);
            dropManager.LoadData(dataArray, _ =>
            {
                titleManager.AutoUpdateShow();
                CreateNewDrop(null,0.3f, 0);
            });
            DropCount = 0;

        }

        // 创建掉落物
        public void CreateNewDrop(Action<bool> onComplete, float executeTime = 1f, float delayTime = 0)
        {
            SelectIndex = DefaultIndexX;
            dropManager.CreateDropItem(executeTime, delayTime, onComplete);
        }
        
        //游戏循环
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
                                if (titleManager.AutoUpdateShow())
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

        #region 调试
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
