using System.Collections;
using System.Collections.Generic;
using Lesson2;
using UnityEngine;

public class LevelCreatorBase
{
    public static LevelCreatorBase Instance = new LevelCreatorBase();
    
    private LevelCreatorBase(){}
    
    public DropNodeManager DropMgr;

    public WeightRandom CreateRandom;
    public WeightRandom LevelRandom;
    public WeightRandom BombRandom;
    
    public int[,] CreateLevel(int width, int height, int totalCount)
    {
        var levelData = new int[height, width];
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if (totalCount >= 0)
                {
                    levelData[i, j] = CreateRandom.GetRandom();
                    totalCount--;
                }
            }
        }

        DropMgr.NormalizeData(levelData);
        
        return levelData;
    }

    public int GetLevelItem()
    {
        return LevelRandom.GetRandom();
    }

    public int GetBombItem()
    {
        return BombRandom.GetRandom();
    }
}
