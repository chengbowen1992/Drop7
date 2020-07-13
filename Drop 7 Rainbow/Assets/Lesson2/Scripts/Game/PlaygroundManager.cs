using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

namespace Lesson2
{
    public class PlaygroundManager : MonoBehaviour
    {
        public enum GameState
        {
            eGaming,
            ePause,
            eFinish
        }
        
        public static readonly int DefaultIndexX = 3;
        public static readonly int MaxTurnCount = 30;
        public static readonly int MinTuneCount = 5;

        public Canvas Scaler;
        public Camera MainCamera;

        public GameOverPanel GameOverCtrl;
        
        public AudioSource MusicPlayer;
        public AudioSource SoundPlayer;
        
        public Transform DropRoot;
        public DropItem CopyOne;

        public Transform GuideRoot;
        public GuideItem GuideOne;
        
        public LevelTitleManager titleManager;

        public Random randomMgr;
        public DropNodeManager dropManager;
        public CommandUtil commandMgr;
        public LevelCreatorBase levelCreator;
        public ScoreManager scoreManager;

        public Text ScoreText;
        public Text LevelText;
        public Text BestText;

        public Button ReplayButton;

        private DropItem NewItem => dropManager.NewItem;
        private Rect[] detectRects;
        public int SelectIndex = DefaultIndexX;

        public int DropCount = 0;

        public GameState CurrentGameState { get; private set; }

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
                var centerPos = new Vector2(deltaIndex * cell /** Scaler.scaleFactor*/ + cenPos.x, cenPos.y);
                var rectSize = new Vector2(cell,cell * (DropNodeManager.HEIGHT + 2)) /** Scaler.scaleFactor*/;
                var rectPos = centerPos - rectSize * 0.5f;
                detectRects[i] = new Rect(rectPos, rectSize);
            }
        }

        // 加载关卡
        public void InitLevel()
        {
            randomMgr = new Random(DateTime.Now.Millisecond);
            commandMgr = CommandUtil.Instance;
            scoreManager = ScoreManager.Instance;
            scoreManager.OnScoreChanged = UpdateScore;
            scoreManager.OnLevelChanged = UpdateLevel;
            scoreManager.OnBestChangeed = UpdateBest;
            
            UpdateScore(0);
            UpdateLevel(1);
            UpdateBest(scoreManager.Best);

            dropManager = new DropNodeManager
            {
                DropItemOne = CopyOne, DropRoot = DropRoot, GuideItemOne = GuideOne, GuideRoot = GuideRoot,
                OnGameFinished = OnGameFinished
            };
            BaseGameCommand.DropMgr = dropManager;
            BaseGameCommand.ScoreMgr = scoreManager;
            
            levelCreator = LevelCreatorBase.Instance;
            levelCreator.DropMgr = dropManager;
            WeightRandom createRandom = new WeightRandom(randomMgr, "20:2|30:3|20:4|15:5|5:6|5:7|2:1|10:-1|15:-2");
            levelCreator.CreateRandom = createRandom;            
            WeightRandom levelRandom = new WeightRandom(randomMgr, "20:1|20:2|20:3|20:4|20:5|20:6|20:7|20:-1|20:-2");
            levelCreator.LevelRandom = levelRandom;
            WeightRandom bombRandom = new WeightRandom(randomMgr, "10:1|20:2|20:3|20:4|20:5|20:6|15:7");
            levelCreator.BombRandom = bombRandom;
            
            dropManager.levelCreator = levelCreator;

            var localData = LocalSaveManager.GameData;
            
            ReStartLevel(localData);
            DropCount = 0;
        }

        public void ResetLevel()
        {
            commandMgr.ResetManager();
            titleManager.ResetManager();
            scoreManager.ResetManager();
            dropManager.ResetManager();
            DropCount = 0;
        }

        public void ReStartLevel(string levelInfo)
        {
            CurrentGameState = GameState.eGaming;
            if (!string.IsNullOrEmpty(levelInfo))
            {
                var gameData = GameSaveData.FromJson(levelInfo);

                if (gameData != null)
                {
                    int[,] dataInfo = GameSaveData.StringToArray(gameData.OriginData);
                    int dropVal = gameData.DropVal;
                    dropManager.LoadData(dataInfo, _ => { CreateNewDrop(null, dropVal, 0.3f, 0); });
                    return;
                }
            }

            var dataArray = levelCreator.CreateLevel(DropNodeManager.WIDTH, DropNodeManager.HEIGHT, 21);
            dropManager.LoadData(dataArray, _ =>
            {
                titleManager.AutoUpdateShow();
                CreateNewDrop(null, null, 0.3f, 0);
            });
        }

        public void SaveDataToLocal()
        {
            var gameData = new GameSaveData();
            gameData.OriginData = GameSaveData.ArrayToString(dropManager.OriginData);
            gameData.DropVal = dropManager.NewItem?.DropData.Value ?? -2;
            
            LocalSaveManager.GameData = gameData.ToJson();
        }

        private void UpdateScore(int score)
        {
            ScoreText.text = score.ToString();
        }

        private void UpdateLevel(int level)
        {
            LevelText.text = $"Level: {level}";
        }

        public void UpdateBest(int best)
        {
            BestText.text = $"Best: {best}";
        }

        // 创建掉落物
        public void CreateNewDrop(Action<bool> onComplete,int? dropVal = null, float executeTime = 1f, float delayTime = 0)
        {
            SelectIndex = DefaultIndexX;
            dropManager.CreateDropItem(executeTime, dropVal, delayTime, onComplete);
            if (false)
            {
                dropManager.CreateAllGuideItem(true);
            }
        }

        public void OnGameFinished(bool ifWin = false)
        {
            CurrentGameState = GameState.eFinish;
            LocalSaveManager.GameData = "";
            LocalSaveManager.BestScore = scoreManager.Score;

            GameOverCtrl.Open(() =>
                {
                    //TODO
                    ResetLevel();
                    ReStartLevel(LocalSaveManager.GameData);
                },
                () =>
                {
                    ResetLevel();
                });
        }

        //游戏循环
        private void Update()
        {
            if (CurrentGameState == GameState.eGaming)
            {
                UpdateInGaming();
            }
            else if (CurrentGameState == GameState.ePause)
            {
                UpdatePause();
            }
            else if (CurrentGameState == GameState.eFinish)
            {
                UpdateFinished();
            }

#if UNITY_EDITOR
            //测试重置游戏
            if (Input.GetKeyDown(KeyCode.R))
            {
                SaveDataToLocal();
                ResetLevel();
                ReStartLevel(LocalSaveManager.GameData);
            }

            //测试清除数据
            if (Input.GetKeyDown(KeyCode.K))
            {
                LocalSaveManager.ClearData();
            }
#endif
        }

        private void OnApplicationQuit()
        {
            SaveDataToLocal();
        }

        private void UpdateInGaming()
        {
            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(Scaler.transform as RectTransform,
                Input.mousePosition, MainCamera, out mousePos);

            int count = detectRects?.Length ?? 0;

            for (int i = 0; i < count; i++)
            {
                if (detectRects[i].Contains(mousePos))
                {
                    var newItem = dropManager.NewItem;
                    if (newItem != null && newItem.IfReady)
                    {
                        if (Input.GetMouseButtonUp(0))
                        {
                            dropManager.DropDropItem(i, onDropComplete =>
                            {
                                if (CurrentGameState == GameState.eFinish)
                                {
                                    return;
                                }
                                
                                if (titleManager.AutoUpdateShow())
                                {
                                    dropManager.CanAddBottomLine(1);
                                    scoreManager.AppendLevel();

                                    dropManager.AddBottomLine(1,
                                        onAddLineComplete =>
                                        {
                                            if (CurrentGameState == GameState.eFinish)
                                            {
                                                return;
                                            }

                                            CreateNewDrop(null, null, 0.3f, 0);
                                        });
                                }
                                else
                                {
                                    CreateNewDrop(null, null, 0.3f, 0);
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

        private void UpdateFinished()
        {
            
        }

        private void UpdatePause()
        {
            
        }

        #region 调试
        private void OnDrawGizmos()
        {
            bool haveInput = Input.GetMouseButton(0);
            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(Scaler.transform as RectTransform,
                Input.mousePosition, MainCamera, out mousePos);
            var inputPos = mousePos;
            
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
