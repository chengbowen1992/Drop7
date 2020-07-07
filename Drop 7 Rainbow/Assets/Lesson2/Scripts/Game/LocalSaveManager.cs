using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

public class LocalSaveManager
{
    public static readonly string Key_BestScore = "Key_BestScore";
    public static readonly string Key_GameData = "Key_GameData";
    
    public static int BestScore
    {
        get => PlayerPrefs.GetInt(Key_BestScore, 0);
        set
        {
            if (value > BestScore)
            {
                PlayerPrefs.SetInt(Key_BestScore,value);
            }
        }
    }

    public static string GameData
    {
        get => PlayerPrefs.GetString(Key_GameData,"");
        set => PlayerPrefs.SetString(Key_GameData, value);
    }

    public static void ClearData()
    {
        PlayerPrefs.SetInt(Key_BestScore,0);
        PlayerPrefs.SetString(Key_GameData, "");
    }
}

[Serializable]
public class GameSaveData
{
    public string OriginData;
    public int DropVal;

    public int Level;
    public int Score;

    public int TitleTotal;
    public int TitleShow;
    public int TitleCurrent;

    public string ToJson()
    {
        var resultStr = JsonUtility.ToJson(this);

#if UNITY_EDITOR
        Debug.Log($"GameSaveData == SaveString:{resultStr}");
#endif
        
        return resultStr;
    }

    public static GameSaveData FromJson(string info)
    {
        if (string.IsNullOrEmpty(info))
        {
            return null;
        }

        var data = JsonUtility.FromJson<GameSaveData>(info);
        return data;
    }

    public static string ArrayToString(int[,] mapData)
    {
        Assert.IsNotNull(mapData);
        
        StringBuilder infoBuilder = new StringBuilder();

        int height = mapData.GetLength(0);
        int width = mapData.GetLength(1);

        infoBuilder.Append(height).Append('|').Append(width).Append('|');
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                infoBuilder.Append(mapData[i, j]);
                if (!(i == height - 1 && j == width - 1))
                {
                    infoBuilder.Append(',');
                }
            }
        }
        
        return infoBuilder.ToString();
    }

    public static int[,] StringToArray(string mapStr)
    {
        Assert.IsFalse(string.IsNullOrEmpty(mapStr));

        string[] rootStrs = mapStr.Split('|');
        int height = int.Parse(rootStrs[0]);
        int width = int.Parse(rootStrs[1]);
        
        Assert.IsTrue(rootStrs.Length == 3);
        string[] mapItems = rootStrs[2].Split(',');
        Assert.IsTrue(mapItems.Length == height * width);
        int[,] mapData = new int[height,width];

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                mapData[i, j] = int.Parse(mapItems[i * width + j]);
            }
        }
        
        return mapData;
    }
}
