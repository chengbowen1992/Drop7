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
        public static readonly int HEIGHT = 7;
        public static readonly int MAX_NUM = 7; //Included
        public static readonly int CENTER_Y = 3;
        public static readonly int CENTER_X = 3;
        public static readonly int CELL_SIZE = 100;

        public int[,] OriginData = new int[HEIGHT, WIDTH]; //原始数据
        public int[,] HorizonMap = new int[HEIGHT, WIDTH]; //行 统计
        public int[,] VerticalMap = new int[HEIGHT, WIDTH]; //列 统计
        public int[,] BombMap = new int[HEIGHT, WIDTH]; //爆炸 统计
        public int[,] MoveMap = new int[HEIGHT, WIDTH]; //移动 统计

        private int[,] BottomArray = new int[HEIGHT, WIDTH]; //底部 填充
        private int BottomHeight = 0;

        public readonly Dictionary<Vector2Int, DropItem> DropDictionary = new Dictionary<Vector2Int, DropItem>(WIDTH * HEIGHT); //统计字典

        public DropItem NewItem; //掉落节点
        
        public List<DropNode> MoveList = new List<DropNode>(); //移动列表
        public List<DropNode> BombList = new List<DropNode>(); //爆炸列表
        public List<DropNode> BombedList = new List<DropNode>(); //爆炸波及列表
        public List<DropNode> OutList = new List<DropNode>(); //超出区域列表

        public Transform DropRoot;
        public DropItem DropItemOne;

        private Random randomMgr;
        private CommandManager cmdManager;

        #region 执行操作

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

        /// <summary>
        /// 加载初始信息
        /// </summary>
        public void LoadData(int[,] data)
        {
            Assert.IsNotNull(data);
            Assert.IsTrue(data.GetLength(0) == HEIGHT);
            Assert.IsTrue(data.GetLength(1) == WIDTH);

            randomMgr = new Random(DateTime.Now.Millisecond);
            cmdManager = CommandManager.Instance;
            
            BombList.Clear();
            BombedList.Clear();
            OutList.Clear();

            OriginData = data;
            BottomHeight = 0;

            for (int i = 0; i < HEIGHT; i++)
            {
                for (int j = 0; j < WIDTH; j++)
                {
                    //初始化 移动 Map
                    //初始化 爆炸 Map
                    HorizonMap[i, j] = VerticalMap[i, j] = MoveMap[i, j] = BombMap[i, j] = BottomArray[i, j] = 0;
                }
            }

            //初始化 列 计数信息
            UpdateVerticalAll();

            //初始化 行 计数信息
            UpdateHorizonAll();
            
            CreateLoadCommands();
        }

        /// <summary>
        /// 产生掉落元素 
        /// </summary>
        public void CreateDropItem(float executeTime ,float delayTime)
        {
            int randomNum = 0;
            while (randomNum == 0)
            {
                randomNum = randomMgr.Next(-2, DropNodeManager.WIDTH);
            }

            CreateNewItemCommands(randomNum, executeTime, delayTime);
        }

        public void DropDropItem(int index)
        {
            if (CanDropNode(index))
            {
                var targetIndex = TryDropNode(index, NewItem.DropData.Value);
                DropDropItemCommand(targetIndex);
            }
        }

        /// <summary>
        /// 判断可否在 第 x 列 掉落
        /// </summary>
        public bool CanDropNode(int x)
        {
            return OriginData[HEIGHT - 1, x] == 0;
        }

        /// <summary>
        /// 在 第x列 掉落值val
        /// val 不为 0
        /// </summary>
        /// <returns>掉落到达的位置</returns>
        public Vector2Int TryDropNode(int x, int val)
        {
            Vector2Int pos = Vector2Int.zero;
            for (int i = 0; i < HEIGHT; i++)
            {
                var item = OriginData[i, x];
                if (item == 0)
                {
                    OriginData[i, x] = val;
                    pos = new Vector2Int(x, i);
                    UpdateDropNode(x, i);
                    break;
                }
            }

            return pos;
        }

        /// <summary>
        /// 掉落后的更新
        /// </summary>
        private void UpdateDropNode(int x, int y)
        {
            //为了便于测试，置于前方
            ClearMap();

            UpdateVerticalByCol(x);

            UpdateHorizonByRow(y);

            int bombAll = 0;

            do
            {
                bombAll = UpdateBombAll(); //可以优化

                if (bombAll > 0)
                {
                    int bombCount, showCount;
                    DealWithBomb(out bombCount, out showCount);
                    DealWitMove();
                }

            } while (bombAll > 0);
        }

        /// <summary>
        /// 执行爆炸操作
        ///     解锁元素
        ///     更新移动信息
        /// TODO 优化
        /// </summary>
        /// <param name="bombCount">爆炸次数</param>
        /// <param name="showCount">显现个数</param>
        private void DealWithBomb(out int bombCount, out int showCount)
        {
            bombCount = showCount = 0;
            //处理 隐藏元素
            for (int i = 0; i < HEIGHT; i++)
            {
                for (int j = 0; j < WIDTH; j++)
                {
                    var val = OriginData[i, j];
                    var bombVal = BombMap[i, j];

                    //隐藏 且 爆炸波及
                    if (val < 0 && bombVal > 0)
                    {
                        var add = val + bombVal;

                        //爆炸未解锁
                        if (add < 0)
                        {
                            OriginData[i, j] = add;
                        }
                        //爆炸解锁，需要随机获得新数字
                        else if (add >= 0)
                        {
                            var newVal = GetRandomNodeNum();
                            OriginData[i, j] = newVal;
                            showCount++;
                        }

                        BombedList.Add(CreateNode(new Vector2Int(j, i), OriginData[i, j]));
                    }
                }
            }

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
                        OriginData[i, j] = 0;
                        moveCount--;
                    }

                    if (OriginData[i, j] != 0)
                    {
                        MoveMap[i, j] = moveCount;
                    }
                    else
                    {
                        MoveMap[i, j] = 0;
                    }
                }
            }
        }

        /// <summary>
        /// 根据爆炸 执行 下行
        /// TODO 优化
        /// </summary>
        private void DealWitMove()
        {
            for (int j = 0; j < WIDTH; j++)
            {
                for (int i = 0; i < HEIGHT; i++)
                {
                    var moveCount = MoveMap[i, j];
                    var originVal = OriginData[i, j];
                    if (moveCount < 0 && originVal != 0)
                    {
                        var newRow = moveCount + i;
                        if (OriginData[newRow, j] != 0)
                        {
                            Debug.LogError($"[{j},{newRow}] == {OriginData[newRow, j]}!!!");
                        }

                        var pos = new Vector2Int(j, i);
                        var newPos = new Vector2Int(j, newRow);
                        OriginData[newRow, j] = OriginData[i, j];
                        OriginData[i, j] = 0;
                    }
                }
            }
        }

        /// <summary>
        /// 从底部生成指定行数目
        /// 不生成空元素
        /// </summary>
        /// <returns>是否游戏可以继续</returns>
        public bool AddBottomLine(int lineHeight)
        {
            if (lineHeight <= 0)
                return true;

            //为了便于测试，置于前方
            ClearMap();

            for (int i = HEIGHT - 1; i >= 0; i--)
            {
                for (int j = 0; j < WIDTH; j++)
                {
                    var toHeight = i + lineHeight;
                    if (toHeight >= HEIGHT)
                    {
                        if (OriginData[i, j] != 0)
                        {
                            var outNode = CreateNode(new Vector2Int(j, toHeight), OriginData[i, j]);
                            OutList.Add(outNode);
                        }
                    }
                    else
                    {
                        OriginData[toHeight, j] = OriginData[i, j];
                    }
                }
            }

            BottomHeight = lineHeight;
            for (int i = 0; i < lineHeight; i++)
            {
                for (int j = 0; j < WIDTH; j++)
                {
                    var randVal = -2 /*randomMgr.Next(0, 10000) % 2 == 0 ? GetRandomNodeNum() : randomMgr.Next(-2, 0)*/;
                    BottomArray[i, j] = randVal;
                    OriginData[i, j] = BottomArray[i, j];
                }
            }

            UpdateAddBottom();

            return OutList.Count == 0;
        }

        private void UpdateAddBottom()
        {
            UpdateVerticalAll();
            UpdateHorizonAll();
            var bombAll = UpdateBombAll();
            if (bombAll > 0)
            {
                int bombCount, showCount;
                DealWithBomb(out bombCount, out showCount);
                DealWitMove();
            }
        }

        /// <summary>
        /// 清理临时计算的 爆炸 移动 数组 以及 爆炸列表
        /// </summary>
        private void ClearMap()
        {
            for (int i = 0; i < HEIGHT; i++)
            {
                for (int j = 0; j < WIDTH; j++)
                {
                    BombMap[i, j] = MoveMap[i, j] = BottomArray[i, j] = 0;
                }
            }

            OutList.Clear();
            BombList.Clear();
            BombedList.Clear();
            BottomHeight = 0;
        }

        private int GetRandomNodeNum()
        {
            return randomMgr.Next(1, MAX_NUM + 1);

        }

        public DropNode CreateNode(Vector2Int pos, int val)
        {
            var node = new DropNode(pos, val);
#if UNITY_EDITOR
            Debug.Log($"CreateNode == {node.ToString()}");
#endif
            return node;
        }

        public DropItem CreateItem(DropNode node)
        {
            var item = Object.Instantiate(DropItemOne, DropRoot, true) as DropItem;
            item.gameObject.SetActive(true);
            item.SetData(node);
            return item;
        }

        public void ExecuteCommands(Action<bool> onFinish)
        {
            CommandManager.Instance.Execute(CommandManager.ExecuteMode.eAtOnce, (_, ifSuccess) => { onFinish?.Invoke(ifSuccess); });
        }

        #endregion

        #region 生成命令
        
        /// <summary>
        /// 生成 所有 元素
        /// </summary>
        private void CreateLoadCommands()
        {
            for (int i = 0; i < HEIGHT; i++)
            {
                for (int j = 0; j < WIDTH; j++)
                {
                    var val = OriginData[i, j];
                    if (val != 0)
                    {
                        //创建
                        var createCmd = new CreateCommand(){DropMgr = this, Index = new Vector2Int(j, i),Position = new Vector3(0, 10000, 0) , CreateType = CreateItemType.eLoad, Val = val};
                        cmdManager.AppendCommand(createCmd);
                                
                        var beginPos = DropItem.GetPositionByIndex(new Vector2Int(j, HEIGHT));
                        var endPos = DropItem.GetPositionByIndex(new Vector2Int(j, i));

                        //移动到初始位置
                        var moveCmd = new MoveCommand()
                        {
                            DropMgr = this, TargetIndex = new Vector2Int(j, i), BeginPos = beginPos, EndPos = endPos,
                            ExecuteTime = 0.6f, DelayTime = i * 0.5f + j * 0.03f
                        };
                        cmdManager.AppendCommand(moveCmd);
                    }
                }
            }
        }

        /// <summary>
        /// 生成 掉落元素
        /// </summary>
        private void CreateNewItemCommands(int randomNum, float executeTime, float delayTime)
        {
            var newItemCmd = new CreateCommand() {CreateType = CreateItemType.eDrop, DropMgr = this, Position = new Vector3(0, (HEIGHT + 1) * CELL_SIZE * 0.5f, 0),Index = new Vector2Int(-1,-1),Val = randomNum,ExecuteTime = executeTime,DelayTime = delayTime};
            cmdManager.AppendCommand(newItemCmd);
        }

        private void DropDropItemCommand(Vector2Int targetIndex)
        {
            var beginPos = DropItem.GetPositionByIndex(new Vector2Int(targetIndex.x,HEIGHT));
            var endPos = DropItem.GetPositionByIndex(targetIndex);
            var moveCmd = new MoveCommand()
            {
                DropMgr = this, Target = NewItem, EndIndex = targetIndex, BeginPos = beginPos, EndPos = endPos, 
                DelayTime = 0f, ExecuteTime = 0.1f
            };
            cmdManager.AppendCommand(moveCmd);
        }

        #endregion
        
        #region 处理命令

        /// <summary>
        /// 创建 元素节点
        /// </summary>
        public void CreateItem(CreateCommand cmd)
        {
            var node = CreateNode(cmd.Index, cmd.Val);
            var item = CreateItem(node);
            
            //正常加载
            if (cmd.CreateType == CreateItemType.eLoad)
            {
                if (cmd.Position.HasValue)
                {
                    item.transform.localPosition = cmd.Position.Value;
                }

                DropDictionary.Add(cmd.Index, item);
                item.ExecuteCreate(cmd);
            }
            //掉落
            else if (cmd.CreateType == CreateItemType.eDrop)
            {
                if (NewItem != null)
                {
                    Debug.LogError("上一个掉落物未销毁！");
                    Object.Destroy(NewItem.gameObject);
                }

                NewItem = item;
                
                if (cmd.Position.HasValue)
                {
                    item.transform.localPosition = cmd.Position.Value;
                }
                
                item.ExecuteCreateDrop(cmd);
            }
        }

        /// <summary>
        /// 强制设置节点位置
        /// </summary>
        public void SetItemPos(SetPositionCommand cmd)
        {
            var target = cmd.Target ? cmd.Target : DropDictionary[cmd.TargetIndex];

            target.transform.localPosition = cmd.Position;
        }

        /// <summary>
        /// 移动节点到
        /// </summary>
        public void MoveItem(MoveCommand cmd)
        {
            var target = cmd.Target ? cmd.Target : DropDictionary[cmd.TargetIndex];

            if (cmd.EndIndex.HasValue)
            {
                var lastIndex = target.DropData.Position;
                var newIndex = cmd.EndIndex.Value;

                DropDictionary.Remove(lastIndex);
                target.DropData.UpdatePosition(newIndex);
                DropDictionary.Add(newIndex, target);
            }

            target.ExecuteMove(cmd);
        }

        
        #endregion

        #region 更新 数据信息

        /// <summary>
        /// 更新所有 行 统计信息
        /// </summary>
        private void UpdateHorizonAll()
        {
            for (int i = 0; i < HEIGHT; i++)
            {
                UpdateHorizonByRow(i);
            }
        }

        /// <summary>
        /// 更新所有 列 统计信息
        /// </summary>
        private void UpdateVerticalAll()
        {
            for (int i = 0; i < WIDTH; i++)
            {
                UpdateVerticalByCol(i);
            }
        }

        /// <summary>
        /// 更新 第 row 行 统计信息
        /// </summary>
        private void UpdateHorizonByRow(int row)
        {
            var data = OriginData;
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
        private void UpdateVerticalByCol(int col)
        {
            var data = OriginData;
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

        /// <summary>
        /// 更新所有 爆炸 信息
        /// 0 正常 -1 消失 1-n 爆炸值
        /// TODO 优化
        /// 可以通过 DropDictionary 优化
        /// </summary>
        private int UpdateBombAll()
        {
            int bombCount = 0;
            var data = OriginData;
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
                        BombMap[i, j] = 0;
                    }
                    else
                    {
                        var hor = HorizonMap[i, j];
                        var ver = VerticalMap[i, j];

                        var ifBomb = val == hor || val == ver;
                        BombMap[i, j] = ifBomb ? -1 : 0;
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
