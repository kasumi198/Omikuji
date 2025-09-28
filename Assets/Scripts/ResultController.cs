using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ResultController : MonoBehaviour
{
    [Header("Result UI Elements")]
    public TMP_Text finalScoreText;
    public TMP_Text evaluationText;
    public TMP_Text shrineListText;
    
    void Start()
    {
        DisplayResult();
    }
    
    void DisplayResult()
    {
        // OmikujiGameから保存されたデータを取得
        int finalScore = PlayerPrefs.GetInt("finalScore", 0);
        string evaluation = PlayerPrefs.GetString("scoreEvaluation", "普通運気");
        string visitedShrines = PlayerPrefs.GetString("visitedShrines", "");
        
        if(finalScoreText != null)
            finalScoreText.text = "最終運気: " + finalScore;
            
        if(evaluationText != null)
            evaluationText.text = evaluation;
            
        if(shrineListText != null && !string.IsNullOrEmpty(visitedShrines))
        {
            string[] shrines = visitedShrines.Split(',');
            string shrineText = "参拝した神社:\n";
            for(int i = 0; i < shrines.Length; i++)
            {
                shrineText += "• " + shrines[i] + "\n";
            }
            shrineListText.text = shrineText;
        }
    }

    public void OnClickStartButton()
    {
        ClearGameData();
        SceneManager.LoadScene("Title");
    }
    
    void ClearGameData()
    {
        // データクリア
        PlayerPrefs.DeleteKey("finalScore");
        PlayerPrefs.DeleteKey("scoreEvaluation");
        PlayerPrefs.DeleteKey("visitedShrines");
        PlayerPrefs.DeleteKey("totalShrines");
        PlayerPrefs.Save();
    }
}
