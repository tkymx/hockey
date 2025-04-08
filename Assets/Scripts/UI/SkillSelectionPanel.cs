using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Hockey.Data;
using System;

public class SkillSelectionPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject skillSelectionPanel;
    [SerializeField] private Transform skillOptionsContainer;
    [SerializeField] private GameObject skillOptionPrefab;
    [SerializeField] private TextMeshProUGUI titleText;
    // Close button removed as per requirement

    private List<GameObject> skillOptionInstances = new List<GameObject>();
    private GameManager gameManager;
    
    // スキル選択時のイベント
    public event Action<string> OnSkillSelected;

    private void Awake()
    {
        if (skillSelectionPanel == null)
        {
            skillSelectionPanel = this.gameObject;
        }
        
        // 初期状態では非表示
        HidePanel();
    }

    public void Initialize(GameManager manager)
    {
        gameManager = manager;
    }

    // スキル選択パネルの表示
    public void ShowPanel(List<SkillData> skillOptions)
    {
        // ゲームを一時停止
        if (gameManager != null)
        {
            gameManager.SetGameActive(false);
        }

        // スキル選択パネルの表示
        skillSelectionPanel.SetActive(true);

        // タイトルの設定
        if (titleText != null)
        {
            titleText.text = "スキルを選択";
        }

        // 以前のスキルオプションをクリア
        ClearSkillOptions();

        // スキルオプションの生成（最大3つ）
        for (int i = 0; i < Mathf.Min(skillOptions.Count, 3); i++)
        {
            CreateSkillOption(skillOptions[i]);
        }
    }
    
    // スキル選択パネルの非表示
    public void HidePanel()
    {
        // ゲームを再開
        if (gameManager != null)
        {
            gameManager.SetGameActive(true);
        }
        
        // スキル選択パネルの非表示
        skillSelectionPanel.SetActive(false);
        
        // スキルオプションをクリア
        ClearSkillOptions();
    }
    
    // スキルオプションの生成
    private void CreateSkillOption(SkillData skill)
    {
        if (skillOptionPrefab == null || skillOptionsContainer == null) return;
        
        // スキルオプションのインスタンス生成
        GameObject optionInstance = Instantiate(skillOptionPrefab, skillOptionsContainer);
        skillOptionInstances.Add(optionInstance);
        
        // スキル情報の表示
        SkillOptionUI optionUI = optionInstance.GetComponent<SkillOptionUI>();
        if (optionUI != null)
        {
            // スキル情報を設定
            optionUI.SetupSkill(skill);
            
            // スキル選択イベントの設定
            optionUI.OnSkillSelected += (selectedSkill) => OnSkillOptionClicked(selectedSkill.skillId);
        }
    }
    
    // スキルオプションがクリックされたときの処理
    private void OnSkillOptionClicked(string skillId)
    {
        // イベントを発行
        OnSkillSelected?.Invoke(skillId);
        
        // パネルを閉じる
        HidePanel();
    }
    
    // スキルオプションのクリア
    private void ClearSkillOptions()
    {
        foreach (GameObject option in skillOptionInstances)
        {
            Destroy(option);
        }
        skillOptionInstances.Clear();
    }
}