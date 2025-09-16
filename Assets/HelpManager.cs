using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HelpManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject helpPanel;
    public TMP_Text helpText;
    public Button closeButton;

    [TextArea]
    public string helpMessage;      // 説明文

    void Start()
    {
        // 最初は非表示
        helpPanel.SetActive(false);

        // 閉じるボタン
        closeButton.onClick.AddListener(() =>
        {
            helpPanel.SetActive(false);
        });
    }

    // 「？」ボタン
    public void ShowHelp()
    {
        helpText.text = helpMessage;
        helpPanel.SetActive(true);
    }
}