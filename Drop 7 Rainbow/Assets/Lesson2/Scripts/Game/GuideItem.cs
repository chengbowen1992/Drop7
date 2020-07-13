using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lesson2
{
    public class GuideItem : MonoBehaviour
    {
        public Image BackBg;

        public void Show(DropGuideCommand cmd)
        {
            this.gameObject.SetActive(true);
            cmd.OnComplete(true);
        }

        public void Hide()
        {
            Destroy(this.gameObject);
        }
    }
}