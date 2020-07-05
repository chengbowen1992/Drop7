using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Lesson2
{
    public class LevelTitleManager : MonoBehaviour
    {
        public LevelTitleItem LevelItemOne;
    
        public List<LevelTitleItem> LevelItems;

        public int TotalCount { get; private set; }
        public int ShowCount { get; private set; }
        public int CurrentShow { get; private set; }

        public int MaxTurnCount { get; private set; }

        public int MinTurnCount { get; private set; }

        public void CreateTitle(int totalCount,int minCount)
        {
            TotalCount = totalCount;
            MaxTurnCount = totalCount;
            MinTurnCount = minCount;

            LevelItemOne.gameObject.SetActive(true);
            LevelItems = new List<LevelTitleItem>(totalCount);
            
            for (int i = 0; i < TotalCount; i++)
            {
                var item = Object.Instantiate(LevelItemOne, transform);
                LevelItems.Add(item);
            }
            
            LevelItemOne.gameObject.SetActive(false);
            UpdateShow(totalCount, totalCount);
        }

        public bool AutoUpdateShow()
        {
            var ifNewTurn = false;
            var currentShow = CurrentShow - 1;
            var showCount = ShowCount;
            if (currentShow <= -1)
            {
                showCount = Mathf.Max(MinTurnCount, showCount - 1);
                currentShow = showCount;
                ifNewTurn = true;
            }
        
            UpdateShow(showCount,currentShow);
            return ifNewTurn;
        }

        public void UpdateShow(int showCount,int currentShow)
        {
            Assert.IsTrue(showCount <= TotalCount);

            if (ShowCount == showCount && CurrentShow == currentShow)
            {
                return;
            }

            ShowCount = showCount;
            CurrentShow = currentShow;

            for (int i = 0; i < TotalCount; i++)
            {
                var item = LevelItems[i];
            
                var state = LevelTitleItem.TitleState.eHide;

                if (i < currentShow)
                {
                    state = LevelTitleItem.TitleState.eShowOn;
                }
                else if (i < showCount)
                {
                    state = LevelTitleItem.TitleState.eShowOff;
                }

                item.SwitchState(state);
            }
        }

        public void ResetManager()
        {
            UpdateShow(TotalCount, TotalCount);
        }
    }
}