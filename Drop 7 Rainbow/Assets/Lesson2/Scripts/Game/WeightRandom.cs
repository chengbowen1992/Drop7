using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Random = System.Random;

public sealed class WeightRandom
{
    public class WeightNode
    {
        public int Min;    // Include
        public int Max;    // Not Include
        public int Val;    

        public bool IfShoot(int index)
        {
            return index >= Min && index < Max;
        }
    }

    private Random randomMgr;

    private int totalCount = 0;
    
    public List<WeightNode> Nodes;

    public WeightRandom(Random random, string weightStr)
    {
        Assert.IsFalse(string.IsNullOrEmpty(weightStr));
        randomMgr = random;
        
        var items = weightStr.Split('|');
        Assert.IsTrue(items.Length > 0);
        
        Nodes = new List<WeightNode>(items.Length);
        totalCount = 0;
        
        for (int i = 0; i < items.Length; i++)
        {
            var item = items[i].Split(':');
            Assert.IsTrue(item.Length == 2);

            int weight = int.Parse(item[0]);
            int value = int.Parse(item[1]);

            if (weight > 0)
            {
                var weightNode = new WeightNode()
                {
                    Min =  totalCount,
                    Max =  totalCount + weight,
                    Val = value
                };

                totalCount += weight;
                
                Nodes.Add(weightNode);
            }
            else
            {
                Debug.LogWarning($"Weight should not be {weight}");
            }
        }
    }

    public int GetRandom()
    {
        int randVal = randomMgr.Next(0, totalCount);

        for (int i = 0; i < Nodes.Count; i++)
        {
            var node = Nodes[i];
            if (node.IfShoot(randVal))
            {
                return node.Val;
            }
        }

        Debug.LogError($"The random val is outOfRange {randVal}!!!");
        
        return Nodes[0].Val;
    }
}
