using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lesson2
{
    public class DropItem : MonoBehaviour
    {
        public enum DropItemState
        {
            eNone,
            eMove,
            eBomb,
            eBombed
        }

        public Image DropImage;
    
        public DropNode DropData;

        public AnimationCurve DefaultMoveCurve;
        
        public int LastVal = 0;

        public DropItemState DropState { get; private set; } = DropItemState.eNone; 

        private MoveCommand moveCmd;
        
        public void SetData(DropNode data)
        {
            DropData = data;
            
            UpdateNode();
        }
    
        public void UpdateNode()
        {
            transform.localPosition = GetPositionByIndex(DropData.Position);

            if (DropData.Value != LastVal)
            {
                LastVal = DropData.Value;
                
                //TODO 用Atlas
                DropImage.sprite = Resources.Load<Sprite>($"Common/Images/{LastVal.ToString().Replace("-", "_")}");
            }
        }

        public void ExcuteMove(MoveCommand cmd)
        {
            if (moveCmd != null)
            {
                Debug.LogError("MoveCommandNotFinish");
                StopAllCoroutines();
            }

            moveCmd = cmd;
            StartCoroutine(PlayMove());
        }

        public IEnumerator PlayMove()
        {
            if (moveCmd == null)
            {
                yield break;
            }
            
            yield return new WaitForSeconds(moveCmd.DelayTime);

            var timeCounter = 0f;
            var totalTime = moveCmd.MoveTime;
            var beginPos = moveCmd.BeginPos;
            var endPos = moveCmd.EndPos;
            
            if (totalTime > 0)
            {
                var moveCurve = moveCmd.MoveCurve ?? DefaultMoveCurve;

                while (timeCounter <= totalTime)
                {
                    var percent = timeCounter / totalTime;
                    var currentVal = moveCurve?.Evaluate(percent) ?? percent;

                    var targetPos = Vector3.LerpUnclamped(beginPos, endPos, currentVal);
                    transform.localPosition = targetPos;
                    
                    timeCounter += Time.deltaTime;
                    yield return null;
                }
            }

            transform.localPosition = endPos;
        }

        public static Vector3 GetPositionByIndex(Vector2Int index)
        {
            int y = (index.y - DropNodeManager.CENTER_Y) * DropNodeManager.CELL_SIZE;
            int x = (index.x - DropNodeManager.CENTER_X) * DropNodeManager.CELL_SIZE;
            return new Vector3(x, y, 0);
        }
    }
}
