﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

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
        public Text BombText;
        
        public DropNode DropData;

        public AnimationCurve DefaultScaleCurveX;
        public AnimationCurve DefaultScaleCurveY;
        
        public AnimationCurve DefaultMoveCurve;
        
        public AnimationCurve DefaultBombScaleCurve;
        public AnimationCurve DefaultBombAlphaCurve;
        public AnimationCurve DefaultBombedScaleCurve;
        public Animation BombTextAnimation;
        
        public int LastVal = 0;

        public DropItemState DropState { get; private set; } = DropItemState.eNone;

        public bool IfReady => DropState != DropItemState.eCreate;
        
        private MoveItemCommand currentMoveCmd;

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

        public void DoCreateItem(CreateItemCommand cmd)
        {
            var position = cmd.Position;
            var createType = cmd.CreateType;
            
            if (position.HasValue)
            {
                transform.localPosition = position.Value;
            }
            
            if (createType == CreateItemType.eLoad || cmd.CreateType == CreateItemType.eBottom)
            {
                StartCoroutine(PlayCreateItem(cmd));
            }
            
            else if (createType == CreateItemType.eDrop)
            {
                StartCoroutine(PlayCreateDropItem(cmd));
            }
        }

        public void DoMoveItem(MoveItemCommand cmd)
        {
            if (currentMoveCmd != null)
            {
                if (!currentMoveCmd.CanBreak)
                {
                    Debug.LogError("MoveCommandNotFinish");
                }

                currentMoveCmd.OnComplete(false);
                currentMoveCmd = null;
                ChangeStateTo(DropItemState.eNone);
                StopCoroutine(nameof(PlayMoveItem));
            }
            
            StartCoroutine(nameof(PlayMoveItem),cmd);
        }

        public void DoBombItem(BombItemCommand cmd)
        {
            if (cmd.NewValue.HasValue)
            {
                StartCoroutine(PlayBombedItem(cmd));
            }
            else
            {
                StartCoroutine(PlayBombItem(cmd));
            }
        }

        #endregion

        #region 具体操作

        private IEnumerator PlayCreateItem(CreateItemCommand cmd)
        {
            ChangeStateTo(DropItemState.eCreate);
            float delayTime = cmd.DelayTime;
            
            if (delayTime > 0)
            {
                yield return new WaitForSeconds(cmd.DelayTime);
            }
            
            ShowItem(true);
            ChangeStateTo(DropItemState.eNone);
            cmd.OnComplete(true);
        }

        private IEnumerator PlayCreateDropItem(CreateItemCommand cmd)
        {
            ChangeStateTo(DropItemState.eCreate);
            float delayTime = cmd.DelayTime;
            
            if (delayTime > 0)
            {
                yield return new WaitForSeconds(cmd.DelayTime);
            }
            ShowItem(true);
            transform.localScale = Vector3.zero;

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

        public IEnumerator PlayMoveItem(MoveItemCommand cmd)
        {
            ChangeStateTo(DropItemState.eMove);
            currentMoveCmd = cmd;
            
            var direction = cmd.Direction;

            if (cmd.DelayTime > 0)
            {
                yield return new WaitForSeconds(cmd.DelayTime);
            }

            var timeCounter = 0f;
            var totalTime = cmd.ExecuteTime;
            var beginPos = cmd.BeginPos.Value;
            var endPos = cmd.EndPos.Value;
            
            if (totalTime > 0)
            {
                var moveCurve = DefaultMoveCurve;

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

            if (direction == MoveItemCommand.MoveDirection.eVertical)
            {
                SoundManager.Instance.PlaySound(SoundNames.Sound_Drop);
            }

            transform.localPosition = endPos;
            currentMoveCmd = null;
            ChangeStateTo(DropItemState.eNone);
            cmd.OnComplete(true);
        }
        
        private IEnumerator PlayBombItem(BombItemCommand cmd)
        {
            ChangeStateTo(DropItemState.eBomb);

            BombText.text = $"+{cmd.ScoreValue}";
            BombTextAnimation.Play();
            
            yield return new WaitForSeconds(cmd.DelayTime);

            float totalTime = cmd.ExecuteTime;
            var timeCounter = 0f;

            SoundManager.Instance.PlaySound(SoundNames.Sound_Bomb);
            
            if (totalTime > 0)
            {
                while (timeCounter <= totalTime)
                {
                    var percent = timeCounter / totalTime;
                    var currentScale = DefaultBombScaleCurve.Evaluate(percent);
                    var currentAlpha = DefaultBombAlphaCurve.Evaluate(percent);

                    transform.localScale = new Vector3(currentScale, currentScale, 1);
                    DropImage.color = new Color(1,1,1,currentAlpha);
                    
                    timeCounter += Time.deltaTime;
                    yield return null;
                }
            }

            ChangeStateTo(DropItemState.eNone);
            cmd.OnComplete(true);
            Destroy(this.gameObject);
        }
        
        private IEnumerator PlayBombedItem(BombItemCommand cmd)
        {
            ChangeStateTo(DropItemState.eBombed);
            
            yield return new WaitForSeconds(cmd.DelayTime);
            DropData.UpdateVal(cmd.NewValue.Value);
            LastVal = DropData.Value;
            
            float totalTime = cmd.ExecuteTime;
            var timeCounter = 0f;

            if (totalTime > 0)
            {
                while (timeCounter <= totalTime)
                {
                    var percent = timeCounter / totalTime;
                    var currentScale = DefaultBombedScaleCurve.Evaluate(percent);
                    transform.localScale = new Vector3(currentScale, currentScale, 1);
                    timeCounter += Time.deltaTime;
                    yield return null;
                }
            }
            
            //TODO 用Atlas
            DropImage.sprite = Resources.Load<Sprite>($"Common/Images/{LastVal.ToString().Replace("-", "_")}");
            ChangeStateTo(DropItemState.eNone);
            cmd.OnComplete(true);
        }
        
        #endregion

        private void ChangeStateTo(DropItemState state)
        {
            if (DropState == state)
            {
                return;
            }

            if (state != DropItemState.eNone && DropState != DropItemState.eNone)
            {
                Debug.LogError($"ChangeState is Not Allowed {state} => {DropState}");
            }

            DropState = state;
        }

        public static Vector3 GetPositionByIndex(Vector2Int index)
        {
            int y = (index.y - DropNodeManager.CENTER_Y) * DropNodeManager.CELL_SIZE;
            int x = (index.x - DropNodeManager.CENTER_X) * DropNodeManager.CELL_SIZE;
            return new Vector3(x, y, 0);
        }

        public void ShowItem(bool ifShow)
        {
            gameObject.SetActive(ifShow);
        }
    }
}
