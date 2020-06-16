using System.Collections;
using System.Collections.Generic;
using Lesson1;
using UnityEngine;
using UnityEngine.Experimental.XR;
using UnityEngine.UI;

namespace Lesson2
{
    public class DropItem : MonoBehaviour
    {
        public Image DropImage;
    
        public DropNode DropData;

        public int LastVal = 0;
        
        public void SetData(DropNode data)
        {
            DropData = data;
            
            UpdateNode();
        }
    
        public void UpdateNode()
        {
            var index = DropData.Position;
            int y = (index.y - DropNodeManager.CENTER_Y) * DropNodeManager.CELL_SIZE;
            int x = (index.x - DropNodeManager.CENTER_X) * DropNodeManager.CELL_SIZE;
            transform.localPosition = new Vector3(x, y, 0);

            if (DropData.Value != LastVal)
            {
                LastVal = DropData.Value;
                //TODO 用Atlas
                DropImage.sprite = Resources.Load<Sprite>($"Common/Images/{LastVal.ToString().Replace("-", "_")}");
            }
        }
    }
}
