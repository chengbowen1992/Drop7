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
            eCreate,
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

        public bool IfReady => DropState != DropItemState.eCreate;
        
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

        #region 执行操作

        public void ExecuteCreate(CreateCommand cmd)
        {
            cmd.OnComplete(true);
        }

        public void ExecuteMove(MoveCommand cmd)
        {
            if (moveCmd != null)
            {
                if (!moveCmd.CanBreak)
                {
                    Debug.LogError("MoveCommandNotFinish");
                }

                moveCmd.OnComplete(false);
                moveCmd = null;
                ChangeStateTo(DropItemState.eNone);
                StopAllCoroutines();
            }
            
            StartCoroutine(PlayMove(cmd));
        }

        public void ExecuteCreateDrop(CreateCommand cmd)
        {
            StartCoroutine(PlayDropCreate(cmd));
        }

        public void ExecuteBomb(BombCommand cmd)
        {
            StartCoroutine(PlayBomb(cmd));
        }
        #endregion

        #region 具体操作

        private IEnumerator PlayMove(MoveCommand cmd)
        {
            ChangeStateTo(DropItemState.eMove);
            moveCmd = cmd;

            if (cmd.DelayTime > 0)
            {
                yield return new WaitForSeconds(cmd.DelayTime);
            }

            var timeCounter = 0f;
            var totalTime = cmd.ExecuteTime;
            var beginPos = cmd.BeginPos;
            var endPos = cmd.EndPos;
            
            if (totalTime > 0)
            {
                var moveCurve = cmd.MoveCurve ?? DefaultMoveCurve;

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
            cmd.OnComplete(true);
            
            ChangeStateTo(DropItemState.eNone);
        }

        private IEnumerator PlayDropCreate(CreateCommand cmd)
        {
            ChangeStateTo(DropItemState.eCreate);
            transform.localScale = Vector3.zero;

            yield return new WaitForSeconds(cmd.DelayTime);
            
            var timeCounter = 0f;
            var totalTime = cmd.ExecuteTime;

            if (totalTime > 0)
            {
                while (timeCounter <= totalTime)
                {
                    var percent = timeCounter / totalTime;
                    var currentValX = DefaultScaleCurveX.Evaluate(percent);
                    var currentValY = DefaultScaleCurveY.Evaluate(percent);

                    transform.localScale = new Vector3(currentValX, currentValY, 1);
                    
                    timeCounter += Time.deltaTime;
                    yield return null;
                }
            }

            transform.localScale = Vector3.one;
            ChangeStateTo(DropItemState.eNone);
            
            cmd.OnComplete(true);
        }

        private IEnumerator PlayBomb(BombCommand cmd)
        {
            yield return new WaitForSeconds(cmd.DelayTime);
            cmd.OnComplete(true);
            Destroy(this.gameObject);
        }
        #endregion

        private void ChangeStateTo(DropItemState state)
        {
            //TODO Check and alert
            
            DropState = state;
        }

        public static Vector3 GetPositionByIndex(Vector2Int index)
        {
            int y = (index.y - DropNodeManager.CENTER_Y) * DropNodeManager.CELL_SIZE;
            int x = (index.x - DropNodeManager.CENTER_X) * DropNodeManager.CELL_SIZE;
            return new Vector3(x, y, 0);
        }
    }
}
