using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lesson2
{
    public class LevelTitleItem : MonoBehaviour
    {
        public enum TitleState
        {
            eShowOn,
            eShowOff,
            eHide,
        }

        public GameObject ImageOn;
        public GameObject ImageOff;
        public int Index { get; private set; }
        public TitleState CurrentState { get; private set; }

        public void SetData(int index, TitleState state)
        {
            Index = index;
            CurrentState = state;
        
            UpdateUI();
        }

        public void SwitchState(TitleState state)
        {
            if (CurrentState == state)
            {
                return;
            }

            CurrentState = state;
            UpdateUI();
        }

        private void UpdateUI()
        {
            ImageOn.SetActive(CurrentState == TitleState.eShowOn);
            ImageOff.SetActive(CurrentState == TitleState.eShowOff);
        }
    }    
}
