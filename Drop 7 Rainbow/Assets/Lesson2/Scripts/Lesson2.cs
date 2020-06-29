using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Lesson2
{
    public class Lesson2 : MonoBehaviour
    {
        public PlaygroundManager PlaygroundMgr;
        
        void Start()
        {
            PlaygroundMgr.InitSoundManager();
            PlaygroundMgr.StartPlayMusic();
            PlaygroundMgr.InitDetectArea();
            PlaygroundMgr.InitLevel();
        }
    }
}
