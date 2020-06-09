using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using Random = System.Random;

namespace Lesson1 
{
    public sealed class DropNodeManager
    {
        public static readonly int WIDTH = 7;
        public static readonly int HEIGHT = 8;
        public static readonly int MAX_NUM = 7;        //Included

        public int[,] OriginData = new int[HEIGHT, WIDTH];   //原始数据
        public int[,] HorizonMap = new int[HEIGHT, WIDTH];  //行 统计
        public int[,] VerticalMap = new int[HEIGHT, WIDTH]; //列 统计
        public int[,] BombMap = new int[HEIGHT, WIDTH];     //爆炸 统计
        public int[,] MoveMap = new int[HEIGHT, WIDTH];     //移动 统计

        public List<DropNode> BombList = new List<DropNode>();  //爆炸列表
        public List<DropNode> BombedList = new List<DropNode>(); //爆炸波及列表

        public Random randomMgr;

        #region  执行操作
        /// <summary>
        /// 加载初始信息
        /// </summary>
        public void LoadData(int[,] data)
        {
            Assert.IsNotNull(data);
            Assert.IsTrue(data.GetLength(0) == HEIGHT);
            Assert.IsTrue(data.GetLength(1) == WIDTH);

            randomMgr = new Random(DateTime.Now.Millisecond);
            BombList.Clear();
            BombedList.Clear();
            OriginData = data;

            for (int i = 0; i < HEIGHT; i++)
            {
                for (int j = 0; j < WIDTH; j++)
                {
                    //初始化 移动 Map
                    //初始化 爆炸 Map
                    HorizonMap[i, j] = VerticalMap[i, j] = MoveMap[i, j] = BombMap[i, j] = 0;
                }
            }

            //初始化 列 计数信息
            UpdateVerticalAll();

            //初始化 行 计数信息
            UpdateHorizonAll();
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

            var bombAll = UpdateBombAll(); //可以优化

            if (bombAll > 0)
            {
                int bombCount, showCount;
                DealWithBomb(out bombCount, out showCount);
                DealWitMove();
            }
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
        /// 清理临时计算的 爆炸 移动 数组 以及 爆炸列表
        /// </summary>
        private void ClearMap()
        {
            for (int i = 0; i < HEIGHT; i++)
            {
                for (int j = 0; j < WIDTH; j++)
                {
                    BombMap[i, j] = MoveMap[i, j] = 0;
                }
            }

            BombList.Clear();
            BombedList.Clear();
        }

        private int GetRandomNodeNum()
        {
            return randomMgr.Next(1, MAX_NUM + 1);

        }


        private DropNode CreateNode(Vector2Int pos, int val)
        {
            var node = new DropNode(pos, val);
#if UNITY_EDITOR
            Debug.Log($"CreateNode == {node.ToString()}");
#endif
            return node;
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

        //TODO Create Bottom Row

        #region  测试
        public enum DebugInfoType 
        {
            eOriginMap = 0,
            eHorizonMap,
            eVerticalMap,
            eMoveMap,
            eBombMap,
            eBombList,
            eBombedList,

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
                    strBuilder.Append($"[{item.Position.x} , {item.Position.y}] = {string.Format("{0,4}", item.Value)} ,");
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
