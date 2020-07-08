using System;
using System.Collections;
using System.Collections.Generic;
using Lesson2;
using UnityEngine;
using UnityEngine.UI;

public class GameOverPanel : MonoBehaviour
{
    public Animator AnimController;
    private static readonly int Show = Animator.StringToHash("Show");
    private static readonly int Hide = Animator.StringToHash("Hide");

    public CanvasGroup CanvasCtrl;
    public Text ScoreText;
    public Text LevelText;
    public Text BestText;
    public Text ComboText;

    public Button ReplayBtn;
    public Button MenuBtn;
    
    public Action OnReplay;
    public Action OnBack;
    
    
    private void Awake()
    {
        OnInit();
    }

    private void OnInit()
    {
        CanvasCtrl.alpha = 0;
        CanvasCtrl.interactable = false;
        CanvasCtrl.blocksRaycasts = false;
        ReplayBtn.onClick.AddListener(OnReplayClick);
        MenuBtn.onClick.AddListener(OnBackClick);
    }

    public void Open(Action onReplay,Action onBack)
    {
        OnReplay = onReplay;
        OnBack = onBack;
        OnOpen();
    }

    public void Close()
    {
        OnReplay = OnBack = null;
        OnClose();        
    }

    private void OnOpen()
    {
        AnimController.SetTrigger(Show);
        CanvasCtrl.interactable = true;
        CanvasCtrl.blocksRaycasts = true;
        var scoreMgr = ScoreManager.Instance;
        ScoreText.text = $"Score : {scoreMgr.Score}";
        LevelText.text = $"Level : {scoreMgr.Level}";
        BestText.text  = $" Best : {scoreMgr.Best}";
        ComboText.text = $"Combo : {scoreMgr.Turn}";
    }

    private void OnClose()
    {
        CanvasCtrl.interactable = false;
        CanvasCtrl.blocksRaycasts = false;
        AnimController.SetTrigger(Hide);
    }

    private void OnReplayClick()
    {
        OnReplay?.Invoke();
        Close();
    }

    private void OnBackClick()
    {
        OnBack?.Invoke();
        Close();
    }

    private void OnDestroy()
    {
        ReplayBtn.onClick.RemoveAllListeners();
        MenuBtn.onClick.RemoveAllListeners();
    }
}
