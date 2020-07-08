using System;
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
        public static readonly Dictionary<int, Color> DropNodeColor = new Dictionary<int, Color>()
        {
            {-2, new Color(119f / 255f, 140f / 255f, 162f / 255f, 0f)},
            {-1, new Color(209f / 255f, 217f / 255f, 225f / 255f, 0f)},
            {0, Color.white},
            {1, new Color(253f / 255f, 92f / 255f, 101f / 255f)},
            {2, new Color(253f / 255f, 150f / 255f, 68f / 255f)},
            {3, new Color(254f / 255f, 210f / 255f, 48f / 255f)},
            {4, new Color(38f / 255f, 222f / 255f, 128f / 255f)},
            {5, new Color(68f / 255f, 170f / 255f, 241f / 255f)},
            {6, new Color(75f / 255f, 122f / 255f, 236f / 255f)},
            {7, new Color(167f / 255f, 94f / 255f, 234f / 255f)},
        };
        
        public static Dictionary<int, Color> BackgroundColorOld = new Dictionary<int, Color>()
        {
            {-2, new Color(119f / 255f, 140f / 255f, 162f / 255f, 0f)},
            {-1, new Color(209f / 255f, 217f / 255f, 225f / 255f, 0f)},
            {0, Color.white},
            {1, new Color(169f / 255f, 31f / 255f, 36f / 255f)},
            {2, new Color(204f / 255f, 102f / 255f, 0f / 255f)},
            {3, new Color(255f / 255f, 204f / 255f, 0f / 255f)},
            {4, new Color(176f / 255f, 208f / 255f, 46f / 255f)},
            {5, new Color(89f / 255f, 191f / 255f, 185f / 255f)},
            {6, new Color(194f / 255f, 33f / 255f, 136f / 255f)},
            {7, new Color(167f / 255f, 94f / 255f, 234f / 255f)},
        };
        
        public enum DropItemState
        {
            eNone,
            eCreate,
            eMove,
            eBomb,
            eBombed
        }

        public Image NumImage;
        public Image BgImage;
        public Text BombText;
        
        public DropNode DropData;

        public AnimationCurve DefaultScaleCurveX;
        public AnimationCurve DefaultScaleCurveY;
        
        public AnimationCurve DefaultMoveCurve;
        
        public AnimationCurve DefaultBombScaleCurve;
        public AnimationCurve DefaultBombAlphaCurve;
        public AnimationCurve DefaultBombedScaleCurve;
        public Animation BombTextAnimation;
        public FlatFX BombEffect;
        
        public int LastVal = 0;

        public DropItemState DropState { get; private set; } = DropItemState.eNone;

        public bool IfReady => DropState != DropItemState.eCreate;

        public Color BgColor
        {
            get
            {
                int val = DropData?.Value ?? 0;
                return DropNodeColor[val];
            }
        }

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
                NumImage.sprite = Resources.Load<Sprite>($"Common/Images/num_{LastVal.ToString().Replace("-", "_")}");
                BgImage.color = DropNodeColor[LastVal];
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
            
            SoundManager.Instance.PlaySound(SoundNames.Sound_Bomb);

            BgImage.gameObject.SetActive(false);

            var tempColor = BgColor;
            
            if (cmd.IfDie)
            {
                BombEffect.AddEffectExt(BombEffect.transform.position, (int) FlatFXType.Crosslight, tempColor, tempColor,
                    tempColor, tempColor);   
            }
            else
            {
                BombEffect.AddEffectExt(BombEffect.transform.position, (int) FlatFXType.Explosion, tempColor, tempColor,
                    tempColor, tempColor); 
            }
            
            yield return new WaitForSeconds(cmd.DelayTime);

            float totalTime = cmd.ExecuteTime;
            var timeCounter = 0f;

            if (!cmd.IfDie)
            {
                BombText.text = $"+{cmd.ScoreValue * cmd.BombCount}";
                BombTextAnimation.Play();   
            }

            if (totalTime > 0)
            {
                while (timeCounter <= totalTime)
                {
                    var percent = timeCounter / totalTime;
                    var currentScale = DefaultBombScaleCurve.Evaluate(percent);
                    var currentAlpha = DefaultBombAlphaCurve.Evaluate(percent);

                    transform.localScale = new Vector3(currentScale, currentScale, 1);
                    var lastColor = BgImage.color;
                    lastColor.a = currentAlpha;
                    BgImage.color = lastColor;
                    
                    timeCounter += Time.deltaTime;
                    yield return null;
                }
            }
            yield return new WaitForSeconds(0.15f);
            ChangeStateTo(DropItemState.eNone);
            cmd.OnComplete(true);
            Destroy(this.gameObject);
        }
        
        private IEnumerator PlayBombedItem(BombItemCommand cmd)
        {
            ChangeStateTo(DropItemState.eBombed);
            var newVal = cmd.NewValue.Value;
            bool ifBomb = LastVal != newVal && newVal > 0;
            yield return new WaitForSeconds(cmd.DelayTime);
            DropData.UpdateVal(newVal);
            LastVal = newVal;
            
            if (ifBomb)
            {
                var tempColor = BgColor;
                BombEffect.AddEffectExt(BombEffect.transform.position, (int) FlatFXType.Pop, tempColor, tempColor,
                    tempColor, tempColor);

                // BombEffect.AddEffectExt(BombEffect.transform.position, (int) FlatFXType.Ripple, tempColor, tempColor,
                //     tempColor, tempColor);
            }
            
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
            NumImage.sprite = Resources.Load<Sprite>($"Common/Images/num_{LastVal.ToString().Replace("-", "_")}");
            BgImage.color = DropNodeColor[LastVal];
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

        public void DestroySelf()
        {
            Destroy(gameObject);
        }
    }
}
