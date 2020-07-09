using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace Lesson2
{
    public sealed class DropNodeManager
    {
        public static readonly int WIDTH = 7;
        public static readonly int HEIGHT = 8;
        public static readonly int MAX_NUM = 7; //Included
        public static readonly int CENTER_Y = 3;
        public static readonly int CENTER_X = 3;
        public static readonly int CELL_SIZE = 100;
        public static readonly float DROP_LOCAL_Y = HEIGHT * CELL_SIZE * 0.5f;

        public int[,] OriginData = new int[HEIGHT, WIDTH]; //原始数据
        public int[,] HorizonMap = new int[HEIGHT, WIDTH]; //行 统计
        public int[,] VerticalMap = new int[HEIGHT, WIDTH]; //列 统计
        public int[,] BombMap = new int[HEIGHT, WIDTH]; //爆炸 统计
        public int[,] MoveMap = new int[HEIGHT, WIDTH]; //移动 统计

        private int[,] BottomArray = new int[HEIGHT, WIDTH]; //底部 填充
        private int BottomHeight = 0;

        private readonly Dictionary<Vector2Int, DropItem> DropDictionary = new Dictionary<Vector2Int, DropItem>(WIDTH * HEIGHT); //统计字典

        public DropItem NewItem; //掉落节点
        
        public List<DropNode> BombList = new List<DropNode>(); //爆炸列表
        public List<DropNode> BombedList = new List<DropNode>(); //爆炸波及列表
        public List<DropNode> OutList = new List<DropNode>(); //超出区域列表
        public List<ValueTuple<Vector2Int,Vector2Int>> MoveList = new List<ValueTuple<Vector2Int, Vector2Int>>();
        
        public Transform DropRoot;
        public DropItem DropItemOne;

        public LevelCreatorBase levelCreator;

        public Action<bool> OnGameFinished;
        
        private CommandUtil commandMgr;

        private int dropHorizonIndex = WIDTH / 2;
        private int? dropValue = null;
        
        //流程
        //--1.初始化

        //--2.掉落 新节点
        //---->新节点

        //--3.计算 爆炸节点
        //---->爆炸点列表

        //--4.爆炸节点
        //---->波及节点列表
        //------>解锁节点
        //------>移动节点
        //------>//无变化节点

        //--5.爆炸附近节点 重新掉落
        //-- 循环 3-5 至没有新的爆炸节点为止

        //--6.添加新的一行
        //--7.循环 3-5 至没有新的爆炸点为止
        
        #region 执行操作
        // 加载初始信息
        public void LoadData(int[,] data, Action<bool> onFinish)
        {
            commandMgr = CommandUtil.Instance;
            
            LoadDataInternal(data);
            LoadDataCommand(onFinish);
        }

        private void LoadDataInternal(int[,] data)
        {
            Assert.IsNotNull(data);
            Assert.IsTrue(data.GetLength(0) == HEIGHT);
            Assert.IsTrue(data.GetLength(1) == WIDTH);
            
            dropValue = null;
            dropHorizonIndex = WIDTH / 2;
            
            ClearMap();
            
            OriginData = data;

            //初始化 列 计数信息
            UpdateVerticalAll(OriginData);

            //初始化 行 计数信息
            UpdateHorizonAll(OriginData);
        }

        private void LoadDataCommand(Action<bool> onFinish)
        {
            var loadGroup = GenerateLoadCommands();
            commandMgr.AppendGroup(loadGroup);
            commandMgr.Execute(onFinish);
        }

        // 产生掉落元素 
        public void CreateDropItem(float executeTime,int? dropVal ,float delayTime,Action<bool> onComplete)
        {
            CreateDropItemInternal(dropVal);
            CreateDropItemCommand(executeTime, delayTime, onComplete);
        }

        public void CreateGuideItem()
        {
            //TODO
        }

        private int CreateDropItemInternal(int? dropVal)
        {
            int randomNum = dropVal ?? GetLevelNodeNum();

            dropHorizonIndex = WIDTH / 2;
            dropValue = randomNum;
            return dropValue.Value;
        }

        private void CreateDropItemCommand(float executeTime, float delayTime, Action<bool> onComplete)
        {
            if (dropValue.HasValue)
            {
                var createDropGroup = GenerateCreateDropCommand(dropValue.Value, delayTime, executeTime);
                commandMgr.AppendGroup(createDropGroup);
                commandMgr.Execute(onComplete);
            }
        }

        // 横向移动掉落物
        public void MoveDropItem(int fromIndex, int toIndex)
        {
            var ifMove = MoveDropItemInternal(fromIndex, toIndex);

            if (ifMove)
            {
                MoveDropItemCommand();
            }
        }

        private bool MoveDropItemInternal(int fromIndex, int toIndex)
        {
            if (fromIndex == toIndex || toIndex == dropHorizonIndex)
            {
                return false;
            }

            dropHorizonIndex = toIndex;
            return true;
        }

        private void MoveDropItemCommand()
        {
            var moveHorizonCmd = GenerateMoveHorizontalCommand(dropHorizonIndex, 0f, 0.2f);
            commandMgr.ExecuteNotWait(moveHorizonCmd);
        }

        // 选择掉落掉落物
        public void DropDropItem(int index,Action<bool> onComplete)
        {
            var targetIndex = DropDropItemInternal(index, dropValue.Value, OriginData);
            DropDropItemCommand(targetIndex, onComplete);
        }

        // 在 第index列 掉落值val
        private Vector2Int DropDropItemInternal(int index,int val,int[,] data)
        {
            Vector2Int pos = Vector2Int.zero;
            for (int i = 0; i < HEIGHT; i++)
            {
                var item = data[i, index];
                if (item == 0)
                {
                    data[i, index] = val;
                    pos = new Vector2Int(index, i);
                    break;
                }
            }

            dropValue = null;
            
            return pos;
        }

        private void DropDropItemCommand(Vector2Int targetIndex, Action<bool> onComplete)
        {
            var dropGroup = GenerateDropItemCommands(targetIndex);
            commandMgr.AppendGroup(dropGroup);
            UpdateAllNode();
            NewItem = null;

            if (CanGameFinish())
            {
                var dieBombGroup = GenerateDieBombCommands();
                commandMgr.AppendGroup(dieBombGroup);
                var gameFinishGroup = GenerateGameFinishCommand();
                commandMgr.AppendGroup(gameFinishGroup);
            }

            commandMgr.Execute(onComplete);
        }

        // 判断可否在 第 x 列 掉落
        private bool CanDropNode(int x,int[,] data)
        {
            return data[HEIGHT - 2, x] == 0;
        }
        
        // 全部节点更新
        private int UpdateAllNode()
        {
            int bombCount = 0;
            int totalBomb = 0;
            int bombTurn = 0;
            do
            {
                ClearMap();
                UpdateHorizonAll(OriginData);
                UpdateVerticalAll(OriginData);
                
                bombCount = UpdateBombInternal(OriginData); //可以优化
                
                if (bombCount > 0)
                {
                    bombTurn++;
                    totalBomb += bombCount;
                    int bombedCount, showCount;
                    DealWithBombInternal(out bombedCount, out showCount,OriginData);

                    var score = bombTurn == 1 ? 7 : 49 * (int)Mathf.Pow(2, bombTurn - 2);

                    DealWithBombCommand(score, bombTurn);

                    DealWitMoveInternal(OriginData);
                    DealWithMoveCommand();
                }

            } while (bombCount > 0);

            return totalBomb;
        }

        public void NormalizeData(int[,] data)
        {
            Assert.IsNotNull(data);
            Assert.IsTrue(data.GetLength(0) == HEIGHT);
            Assert.IsTrue(data.GetLength(1) == WIDTH);
            
            int totalBomb = 0;
            int bombCount = 0;
            do
            {
                ClearMap();
                UpdateHorizonAll(data);
                UpdateVerticalAll(data);
                bombCount = UpdateBombInternal(data); //可以优化
                if (bombCount > 0)
                {
                    totalBomb += bombCount;
                    int bombedCount, showCount;
                    DealWithBombInternal(out bombedCount, out showCount,data);
                    DealWitMoveInternal(data);
                }

            } while (bombCount > 0);

            ClearMap();
        }

        /// <summary>
        /// 执行爆炸操作
        ///     解锁元素
        /// TODO 优化
        /// </summary>
        /// <param name="bombCount">爆炸次数</param>
        /// <param name="showCount">显现个数</param>
        private void DealWithBombInternal(out int bombCount, out int showCount,int[,] data)
        {
            bombCount = showCount = 0;
            //处理 隐藏元素
            for (int i = 0; i < HEIGHT; i++)
            {
                for (int j = 0; j < WIDTH; j++)
                {
                    var val = data[i, j];
                    var bombVal = BombMap[i, j];

                    //隐藏 且 爆炸波及
                    if (val < 0 && bombVal > 0)
                    {
                        var add = val + bombVal;

                        var newVal = 0;
                        
                        //爆炸未解锁
                        if (add < 0)
                        {
                            newVal = add;
                        }
                        //爆炸解锁，需要随机获得新数字
                        else if (add >= 0)
                        {
                            newVal = GetBombNodeNum();
                            showCount++;
                        }

                        data[i, j] = newVal;
                        bombCount++;
                        BombedList.Add(CreateNode(new Vector2Int(j, i), newVal));
                    }
                }
            }
        }

        private void DealWithBombCommand(int score, int bombTurn)
        {
            var bombGroup = GenerateBombCommands(score, bombTurn, 0.3f,0.3f,0.2f,0.3f);
            commandMgr.AppendGroup(bombGroup);
        }

        // 根据爆炸 执行 下行
        private void DealWitMoveInternal(int[,] data)
        {
            //处理移动列表
            for (int j = 0; j < WIDTH; j++)
            {
                int moveCount = 0;
                for (int i = 0; i < HEIGHT; i++)
                {
                    //炸了
                    if (BombMap[i, j] < 0)
                    {
                        //删除 爆炸 点
                        data[i, j] = 0;
                        moveCount--;
                    }

                    if (data[i, j] != 0)
                    {
                        MoveMap[i, j] = moveCount;
                    }
                    else
                    {
                        MoveMap[i, j] = 0;
                    }
                }
            }
            
            for (int j = 0; j < WIDTH; j++)
            {
                for (int i = 0; i < HEIGHT; i++)
                {
                    var moveCount = MoveMap[i, j];
                    var originVal = data[i, j];
                    if (moveCount < 0 && originVal != 0)
                    {
                        var newRow = moveCount + i;
                        if (data[newRow, j] != 0)
                        {
                            Debug.LogError($"[{j},{newRow}] == {data[newRow, j]}!!!");
                        }

                        var pos = new Vector2Int(j, i);
                        var newPos = new Vector2Int(j, newRow);
                        data[newRow, j] = data[i, j];
                        data[i, j] = 0;

                        MoveList.Add(ValueTuple.Create(pos, newPos));
                    }
                }
            }
        }

        private void DealWithMoveCommand()
        {
            var bombMoveGroup = GenerateBombMoveCommands(0.1f,0.2f);
            commandMgr.AppendGroup(bombMoveGroup);
        }

        public bool CanAddBottomLine(int lingHeight)
        {
            for (int i = 0; i < WIDTH; i++)
            {
                if (OriginData[HEIGHT - 2, i] != 0)
                {
                    return false;
                }
            }

            return true;
        }

        // 从底部生成指定行数目
        // 不生成空元素
        public void AddBottomLine(int lineHeight,Action<bool> onComplete)
        {
            Assert.IsTrue(lineHeight > 0);

            AddBottomLineInternal(lineHeight, OriginData);
            AddBottomLineCommand(lineHeight, onComplete);
        }

        private void AddBottomLineInternal(int lineHeight,int[,] data)
        {
            ClearMap();

            for (int i = HEIGHT - 1; i >= 0; i--)
            {
                for (int j = 0; j < WIDTH; j++)
                {
                    var toHeight = i + lineHeight;
                    var value = data[i, j];

                    if (toHeight >= HEIGHT)
                    {
                        if (value != 0)
                        {
                            var outNode = CreateNode(new Vector2Int(j, toHeight), data[i, j]);
                            OutList.Add(outNode);
                        }
                    }
                    else
                    {
                        data[toHeight, j] = value;
                    }
                }
            }

            BottomHeight = lineHeight;
            for (int i = 0; i < lineHeight; i++)
            {
                for (int j = 0; j < WIDTH; j++)
                {
                    var randVal = -2;
                    data[i, j] = BottomArray[i, j] = randVal;
                }
            }
        }

        private void AddBottomLineCommand(int lineHeight, Action<bool> onComplete)
        {
            var bottomGroup = GenerateBottomCommands(lineHeight, 0f, 0f, 0f, 0f);
            commandMgr.AppendGroup(bottomGroup);
            UpdateAllNode();
            
            if (CanGameFinish())
            {
                var dieBombGroup = GenerateDieBombCommands();
                commandMgr.AppendGroup(dieBombGroup);
                var gameFinishGroup = GenerateGameFinishCommand();
                commandMgr.AppendGroup(gameFinishGroup);
            }
            
            commandMgr.Execute(onComplete);
        }

        // 清理临时计算的 爆炸 移动 数组 以及 爆炸列表
        private void ClearMap()
        {
            for (int i = 0; i < HEIGHT; i++)
            {
                for (int j = 0; j < WIDTH; j++)
                {
                    HorizonMap[i, j] = VerticalMap[i, j] = BombMap[i, j] = MoveMap[i, j] = BottomArray[i, j] = 0;
                }
            }

            OutList.Clear();
            BombList.Clear();
            BombedList.Clear();
            MoveList.Clear();
            BottomHeight = 0;
        }

        private int GetLevelNodeNum()
        {
            return levelCreator.GetLevelItem();
        }

        private int GetBombNodeNum()
        {
            return levelCreator.GetBombItem();
        }

        private DropNode CreateNode(Vector2Int pos, int val)
        {
            var node = new DropNode(pos, val);
#if UNITY_EDITOR
            Debug.Log($"CreateNode == {node.ToString()}");
#endif
            return node;
        }

        private DropItem CreateItem(DropNode node,bool ifShow = true)
        {
            var item = Object.Instantiate(DropItemOne, DropRoot, true) as DropItem;
            item.gameObject.SetActive(true);
            item.SetData(node);
            if(!ifShow) item.gameObject.SetActive(false);
            return item;
        }

        #endregion

        #region 生成命令

        private CommandGroup GenerateLoadCommands()
        {
            var loadGroup = commandMgr.CreateGroup(GroupExecuteMode.eAllAtOnce, "loadGroup");
            for (int i = 0; i < HEIGHT; i++)
            {
                for (int j = 0; j < WIDTH; j++)
                {
                    var val = OriginData[i, j];
                    if (val != 0)
                    {
                        //create
                        var createItemCmd = new CreateItemCommand()
                        {
                            CreateType = CreateItemType.eLoad,
                            Index = new Vector2Int(j, i),
                            Position = new Vector3(0, 10000, 0) , 
                            Value = val
                        };
                        loadGroup.AppendCommand(createItemCmd);
                                
                        var beginPos = DropItem.GetPositionByIndex(new Vector2Int(j, HEIGHT-1));
                        var endPos = DropItem.GetPositionByIndex(new Vector2Int(j, i));

                        //move
                        var moveItemCmd = new MoveItemCommand()
                        {
                            Target = createItemCmd.Target,
                            Direction = MoveItemCommand.MoveDirection.eVertical,
                            BeginPos = beginPos, 
                            EndPos = endPos,
                            FromIndex = null,
                            ToIndex = null,
                            ExecuteTime = 0.6f,
                            DelayTime = i * 0.5f + j * 0.03f
                        };
                        loadGroup.AppendCommand(moveItemCmd);
                    }
                }
            }

            return loadGroup;
        }

        private CommandGroup GenerateCreateDropCommand(int randomNum, float delayTime, float executeTime)
        {
            CommandGroup createDropGroup = commandMgr.CreateGroup(GroupExecuteMode.eAfterFinish, "createDropGroup");
            var newItemCmd = new CreateItemCommand()
            {
                CreateType = CreateItemType.eDrop,
                Position = new Vector3(0, DROP_LOCAL_Y, 0),
                Index = new Vector2Int(-1,-1),
                Value = randomNum,
                ExecuteTime = executeTime,
                DelayTime = delayTime
            };
            createDropGroup.AppendCommand(newItemCmd);
            return createDropGroup;
        }

        private CommandGroup GenerateBottomCommands(int lineHeight,float createDelayTime,float createExecuteTime,float moveDelayTime,float moveExecuteTime)
        {
            var bottomGroup = commandMgr.CreateGroup(GroupExecuteMode.eAllAtOnce, "bottomGroup");
            
            for (int i = HEIGHT-1; i >= 0; i--)
            {
                for (int j = 0; j < WIDTH; j++)
                {
                    var value = OriginData[i, j];
                    if (value == 0)
                    {
                        continue;
                    }
                    
                    var index = new Vector2Int(j, i);
                    
                    //create
                    if (i < lineHeight)
                    {
                        Vector3 beginPos = DropItem.GetPositionByIndex(index - new Vector2Int(0, 1));

                        var newItemCmd = new CreateItemCommand()
                        {
                            CreateType = CreateItemType.eBottom,
                            Position = beginPos,
                            Index = index,
                            Value = value,
                            DelayTime = createDelayTime,
                            ExecuteTime = createExecuteTime,
                        };
                        bottomGroup.AppendCommand(newItemCmd);

                        var endPos = DropItem.GetPositionByIndex(index);
                        var moveCmd = new MoveItemCommand()
                        {
                            Target = newItemCmd.Target,
                            BeginPos = beginPos,
                            EndPos = endPos,
                            DelayTime = moveDelayTime,
                            ExecuteTime = moveExecuteTime,
                        };
                        bottomGroup.AppendCommand(moveCmd);
                    }
                    //move
                    else
                    {
                        var lastIndex = new Vector2Int(j,i - lineHeight);
                        var target = GetFromDrop(lastIndex);
                        var moveCmd = new MoveItemCommand()
                        {
                            Target = target,
                            FromIndex = lastIndex,
                            ToIndex = index,
                            DelayTime = moveDelayTime,
                            ExecuteTime =  moveExecuteTime,
                        };
                        
                        bottomGroup.AppendCommand(moveCmd);
                    }
                }
            }
            
            return bottomGroup;
        }

        private CommandGroup GenerateBombCommands(int score, int bombTurn, float bombDelayTime, float bombExecuteTime,
            float bombedDelayTime, float bombedExecuteTime)
        {
            var bombGroup = commandMgr.CreateGroup(GroupExecuteMode.eAllAtOnce, "bombGroup");
            var bombCount = BombList.Count;
            var bombedCount = BombedList.Count;
            for (int i = 0; i < bombCount; i++)
            {
                var item = BombList[i];
                var bombItemCmd = new BombItemCommand()
                {
                    Target = GetFromDrop(item.Position),
                    DelayTime = bombDelayTime,
                    ExecuteTime = bombExecuteTime,
                    BombCount = bombCount,
                    NewValue = null,
                    ScoreValue = score,
                };
                bombGroup.AppendCommand(bombItemCmd);

                var scoreCmd = new ScoreUpCommnad()
                {
                    ScoreAppend = score,
                    BombNum = bombCount,
                    TurnCount = bombTurn,
                };
                bombGroup.AppendCommand(scoreCmd);
            }

            for (int i = 0; i < bombedCount; i++)
            {
                var item = BombedList[i];
                var bombedItemCmd = new BombItemCommand()
                {
                    Target = GetFromDrop(item.Position),
                    NewValue = item.Value,
                    BombCount = bombedCount,
                    ExecuteTime = bombedExecuteTime,
                    DelayTime = bombedDelayTime,
                };
                bombGroup.AppendCommand(bombedItemCmd);
            }

            return bombGroup;
        }

        private CommandGroup GenerateBombMoveCommands(float delayTime, float executeTime)
        {
            var bombMoveGroup = commandMgr.CreateGroup(GroupExecuteMode.eAllAtOnce, "bombMoveGroup");

            for (int i = 0; i < MoveList.Count; i++)
            {
                var moveData = MoveList[i];

                var target = GetFromDrop(moveData.Item1);
                var moveCmd = new MoveItemCommand()
                {
                    Target = target,
                    FromIndex = moveData.Item1,
                    ToIndex = moveData.Item2,
                    DelayTime = delayTime,
                    ExecuteTime = executeTime,
                };
                
                bombMoveGroup.AppendCommand(moveCmd);
            }

            return bombMoveGroup;
        }

        private CommandGroup GenerateDropItemCommands(Vector2Int targetIndex)
        {
            var curPos = NewItem.transform.localPosition;
            var beginPos = DropItem.GetPositionByIndex(new Vector2Int(targetIndex.x, HEIGHT - 1));
            var endPos = DropItem.GetPositionByIndex(targetIndex);

            var dropGroup = commandMgr.CreateGroup(GroupExecuteMode.eAfterFinish, "dropGroup");
            
            if (Vector3.Distance(curPos, beginPos) > 10f)
            {
                var moveHorizonCmd = GenerateMoveHorizontalCommand(targetIndex.x,0f,0.2f);
                
                dropGroup.AppendCommand(moveHorizonCmd);
            }

            var dropCmd = new MoveItemCommand()
            {
                Target = NewItem,
                BeginPos = beginPos,
                EndPos = endPos,
                ToIndex = targetIndex,
                DelayTime = 0f,
                ExecuteTime = 0.3f,
            };
            dropGroup.AppendCommand(dropCmd);
            return dropGroup;
        }

        private MoveItemCommand GenerateMoveHorizontalCommand(int targetIndex,float delayTime,float executeTime)
        {
            var curPos = NewItem.transform.localPosition;
            var endPos = DropItem.GetPositionByIndex(new Vector2Int(targetIndex, 0));
            endPos.y = DROP_LOCAL_Y;
            
            var moveCmd = new MoveItemCommand()
            {
                Target = NewItem,
                CanBreak = true,
                Direction = MoveItemCommand.MoveDirection.eHorizontal,
                BeginPos = curPos,
                EndPos = endPos,
                DelayTime = delayTime,
                ExecuteTime = executeTime,
            };
            return moveCmd;
        }

        public CommandGroup GenerateDieBombCommands()
        {
            var dieBombGroup = commandMgr.CreateGroup(GroupExecuteMode.eAllAtOnce, "dieBombGroup");

            for (int i = 0; i < WIDTH-1; i++)
            {
                var itemVal = OriginData[HEIGHT - 1, i];
                if (itemVal != 0)
                {
                    var dieBombCmd = new BombItemCommand()
                    {
                        IfDie = true,
                        DelayTime = i * 0.1f,
                        ExecuteTime = 0.5f,
                        Target = GetFromDrop(new Vector2Int(i, HEIGHT - 1))
                    };
                    
                    dieBombGroup.AppendCommand(dieBombCmd);
                }
            }
            
            return dieBombGroup;
        }

        public CommandGroup GenerateGameFinishCommand()
        {
            Assert.IsTrue(CanGameFinish());            
            var gameFinishGroup = commandMgr.CreateGroup(GroupExecuteMode.eAfterFinish, "gameFinishGroup");
            var gameFinishCmd = new GameFinishCommand()
            {
                Score = ScoreManager.Instance.Score,
                Level = ScoreManager.Instance.Level,
                BestScore = ScoreManager.Instance.Best,
            };
            gameFinishGroup.AppendCommand(gameFinishCmd);
            return gameFinishGroup;
        }

        public bool CanGameFinish()
        {
            for (int i = 0; i < WIDTH; i++)
            {
                if (OriginData[HEIGHT - 1, i] != 0)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
        
        #region 处理命令

        public void DoAppendCreateItemCommand(CreateItemCommand cmd)
        {
            var node = CreateNode(cmd.Index, cmd.Value);
            var item = CreateItem(node);
            cmd.Target = item;

            var createType = cmd.CreateType;
            if (createType == CreateItemType.eLoad || createType == CreateItemType.eBottom)
            {
                AddToDrop(cmd.Index, item);
            }
            else if (cmd.CreateType == CreateItemType.eDrop)
            {
                if (NewItem != null)
                {
                    Debug.LogError("上一个掉落物未销毁！");
                    Object.Destroy(NewItem.gameObject);
                }
                NewItem = item;
            }
        }

        public void DoExecuteCreateItemCommand(CreateItemCommand cmd)
        {
            var target = cmd.Target;
            var createType = cmd.CreateType;
            target.DoCreateItem(cmd);
        }

        public void DoMoveItemCommand(MoveItemCommand cmd)
        {
            cmd.Target.DoMoveItem(cmd);
        }

        public void DoBombItemCommand(BombItemCommand cmd)
        {
            cmd.Target.DoBombItem(cmd);
        }

        #endregion

        #region 更新 数据信息
        
        // 更新所有 行 统计信息
        private void UpdateHorizonAll(int[,] data)
        {
            for (int i = 0; i < HEIGHT; i++)
            {
                UpdateHorizonByRow(i,data);
            }
        }
        
        // 更新所有 列 统计信息
        private void UpdateVerticalAll(int[,] data)
        {
            for (int i = 0; i < WIDTH; i++)
            {
                UpdateVerticalByCol(i, data);
            }
        }

        /// <summary>
        /// 更新 第 row 行 统计信息
        /// </summary>
        private void UpdateHorizonByRow(int row, int[,] data)
        {
            int begin, len;
            begin = len = -1;

            for (int j = 0; j < WIDTH; j++)
            {
                HorizonMap[row, j] = 0;
                var item = data[row, j];

                if (item == 0)
                {
                    if (len > 0)
                    {
                        for (int k = begin; k < begin + len; k++)
                        {
                            HorizonMap[row, k] = len;
                        }
                    }

                    begin = len = -1;
                }
                else
                {
                    if (begin < 0)
                    {
                        begin = j;
                        len = 1;
                    }
                    else
                    {
                        len++;
                    }

                    if (j == WIDTH - 1)
                    {
                        for (int k = begin; k < begin + len; k++)
                        {
                            HorizonMap[row, k] = len;
                        }

                        begin = len = -1; //重置，虽然多余
                    }
                }
            }
        }

        /// <summary>
        /// 更新 第 col 列 统计信息
        /// </summary>
        private void UpdateVerticalByCol(int col, int[,] data)
        {
            int countY = -1;
            for (int i = HEIGHT - 1; i >= 0; i--)
            {
                if (countY < 0 && data[i, col] != 0)
                {
                    countY = i + 1;
                }

                VerticalMap[i, col] = countY < 0 ? 0 : countY;
            }
        }
        
        // 更新所有 爆炸 信息
        //== 0 正常 -1 消失 1-n 爆炸值
        //TODO: 可以通过 DropDictionary 优化
        private int UpdateBombInternal(int[,] data)
        {
            int bombCount = 0;
            Action<int, int> bombItem = (row, col) =>
            {
                if (BombMap[row, col] != -1)
                {
                    Debug.LogError($"Bomb [{col},{row}] is not -1 but {BombMap[row, col]}");
                    return;
                }

                for (int i = row - 1; i <= row + 1; i++)
                {
                    if (i >= 0 && i < HEIGHT)
                    {
                        var oldVal = BombMap[i, col];
                        if (oldVal >= 0)
                        {
                            BombMap[i, col]++;
                        }
                    }
                }

                for (int j = col - 1; j <= col + 1; j++)
                {
                    if (j >= 0 && j < WIDTH)
                    {
                        var oldVal = BombMap[row, j];
                        if (oldVal >= 0)
                        {
                            BombMap[row, j]++;
                        }
                    }
                }

                BombMap[row, col] = -1;
            };

            for (int i = 0; i < HEIGHT; i++)
            {
                for (int j = 0; j < WIDTH; j++)
                {
                    var val = data[i, j];

                    if (val == 0)
                    {
                        // BombMap[i, j] = 0;
                    }
                    else
                    {
                        var hor = HorizonMap[i, j];
                        var ver = VerticalMap[i, j];

                        var ifBomb = val == hor || val == ver;
                        BombMap[i, j] = ifBomb ? -1 : BombMap[i, j];
                        if (ifBomb)
                        {
                            bombItem(i, j);
                            BombList.Add(CreateNode(new Vector2Int(j, i), val));
                            bombCount++;
                        }
                    }
                }
            }

            return bombCount;
        }

        #endregion

        #region 处理字典

        public DropItem GetFromDrop(Vector2Int index)
        {
#if UNITY_EDITOR
            Debug.Log($"Drop == Get from {index} : {DropDictionary[index]}");
#endif

            return DropDictionary[index];
        }

        public void AddToDrop(Vector2Int index, DropItem item)
        {
#if UNITY_EDITOR
            Debug.Log($"Drop == Add to {index} : {item.DropData.Value}");

            if (DropDictionary.ContainsKey(index))
            {
                Debug.LogError($"Drop == Add to {index} : old:{DropDictionary[index]}");
            }
#endif
            DropDictionary.Add(index, item);
        }

        public void RemoveInDrop(Vector2Int index)
        {
#if UNITY_EDITOR
            Debug.Log($"Drop == Remove from {index}");

            if (DropDictionary.ContainsKey(index))
            {
                Debug.Log($"Drop == Remove from {index} have value :{DropDictionary[index].DropData.Value}");
            }
#endif
            DropDictionary.Remove(index);
        }

        public void ReplaceInDrop(Vector2Int from, Vector2Int to)
        {
#if UNITY_EDITOR
            Debug.Log($"Drop == Replace from {from} : {to} : with {DropDictionary[from]}");
            
            if (DropDictionary.ContainsKey(to))
            {
                Debug.LogError($"Drop == Replace to {to} : old:{DropDictionary[to]}");
            }
#endif
            var item = DropDictionary[from];
            DropDictionary.Add(to,item);
            DropDictionary.Remove(from);
        }

        #endregion

        public void ResetManager()
        {
            for (int i = 0; i < HEIGHT; i++)
            {
                for (int j = 0; j < WIDTH; j++)
                {
                    OriginData[i, j] = 0;
                }
            }

            foreach (var bombItem in DropDictionary.Values)
            {
                bombItem.DestroySelf();
            }
            
            DropDictionary.Clear();
            
            OutList.Clear();
            BombList.Clear();
            BombedList.Clear();
            MoveList.Clear();
            BottomHeight = 0;

            if (NewItem)
            {
                NewItem.DestroySelf();
                NewItem = null;
            }
        }

        #region 测试

        public enum DebugInfoType
        {
            eOriginMap = 0,
            eHorizonMap,
            eVerticalMap,
            eMoveMap,
            eBombMap,
            eBombList,
            eBombedList,
            eBottomMap,
            eOutList,

            //Do not change
            eAll
        }

        public string GetDebugInfo(DebugInfoType infoType)
        {
            Action<int[,], StringBuilder> _AppendArrayInfo = (arr, strBuilder) =>
            {
                for (int i = 0; i < HEIGHT; i++)
                {
                    for (int j = 0; j < WIDTH; j++)
                    {
                        var val = arr[i, j];
                        strBuilder.Append(string.Format("{0,4}", val));
                    }

                    strBuilder.Append("\n");
                }

            };

            Action<List<DropNode>, StringBuilder> _AppendListInfo = (list, strBuilder) =>
            {
                foreach (var item in list)
                {
                    strBuilder.Append(
                        $"[{item.Position.x} , {item.Position.y}] = {string.Format("{0,4}", item.Value)} ,");
                }

                strBuilder.Append("\n");
            };

            var stringBuilder = new StringBuilder();

            switch (infoType)
            {
                case DebugInfoType.eOriginMap:
                    stringBuilder.Append("\nOriginData:\n");
                    _AppendArrayInfo(OriginData, stringBuilder);
                    break;
                case DebugInfoType.eHorizonMap:
                    stringBuilder.Append("\nHorizonMap:\n");
                    _AppendArrayInfo(HorizonMap, stringBuilder);
                    break;
                case DebugInfoType.eVerticalMap:
                    stringBuilder.Append("\nVerticalMap:\n");
                    _AppendArrayInfo(VerticalMap, stringBuilder);
                    break;
                case DebugInfoType.eMoveMap:
                    stringBuilder.Append("\nMoveMap:\n");
                    _AppendArrayInfo(MoveMap, stringBuilder);
                    break;
                case DebugInfoType.eBombMap:
                    stringBuilder.Append("\nBombMap:\n");
                    _AppendArrayInfo(BombMap, stringBuilder);
                    break;
                case DebugInfoType.eBombList:
                    stringBuilder.Append("\nBombList\n");
                    _AppendListInfo(BombList, stringBuilder);
                    break;
                case DebugInfoType.eBombedList:
                    stringBuilder.Append("\nBombedList\n");
                    _AppendListInfo(BombedList, stringBuilder);
                    break;
                case DebugInfoType.eBottomMap:
                    stringBuilder.Append("\nBottomArray\n");
                    _AppendArrayInfo(BottomArray, stringBuilder);
                    break;
                case DebugInfoType.eOutList:
                    stringBuilder.Append("\nOutList\n");
                    _AppendListInfo(OutList, stringBuilder);
                    break;
                case DebugInfoType.eAll:
                    break;
                default:
                    break;
            }

            return stringBuilder.ToString();
        }

        #endregion
    }
}
