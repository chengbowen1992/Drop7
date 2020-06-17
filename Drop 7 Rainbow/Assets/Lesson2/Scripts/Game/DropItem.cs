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

        public AnimationCurve DefaultScaleCurveX;
        public AnimationCurve DefaultScaleCurveY;
        
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
            
            StartCoroutine(PlayMove(cmd));
        }

        public void ExcuteCreateDrop(CreateCommand cmd)
        {
            StartCoroutine(PlayDropCreate(cmd));
        }

        private IEnumerator PlayMove(MoveCommand cmd)
        {
            moveCmd = cmd;
            
            yield return new WaitForSeconds(moveCmd.DelayTime);

            var timeCounter = 0f;
            var totalTime = moveCmd.ExcuteTime;
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
            moveCmd = null;
        }

        private IEnumerator PlayDropCreate(CreateCommand cmd)
        {
            transform.localScale = Vector3.zero;

            yield return new WaitForSeconds(cmd.DelayTime);
            
            var timeCounter = 0f;
            var totalTime = cmd.ExcuteTime;

            if (totalTime > 0)
            {
                while (timeCounter <= totalTime)
                {
                    var percent = timeCounter / totalTime;
                    var currentValX = DefaultScaleCurveX.Evaluate(percent);
                    var currentValY = DefaultScaleCurveX.Evaluate(percent);

                    transform.localScale = new Vector3(currentValX, currentValY, 1);
                    
                    timeCounter += Time.deltaTime;
                    yield return null;
                }
            }

            transform.localScale = Vector3.one;
        }

        public static Vector3 GetPositionByIndex(Vector2Int index)
        {
            int y = (index.y - DropNodeManager.CENTER_Y) * DropNodeManager.CELL_SIZE;
            int x = (index.x - DropNodeManager.CENTER_X) * DropNodeManager.CELL_SIZE;
            return new Vector3(x, y, 0);
        }
    }
}
