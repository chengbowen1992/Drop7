using System.Collections;
using System.Collections.Generic;
using Lesson1;
using UnityEngine;
using UnityEngine.UI;

namespace Lesson2
{
    public class DropItem : MonoBehaviour
    {
        public Image DropImage;
    
        public DropNode DropData;
    
        public void SetData(DropNode data)
        {
            DropData = data;
        }
    
        public void UpdateNode()
        {
            
        }
    }
}
