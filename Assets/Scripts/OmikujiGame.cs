using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class OmikujiGame : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text shrineNameText;
    public TMP_Text scoreText;
    public Transform shopPanel;
    public Transform inventoryPanel;
    public GameObject itemButtonPrefab;
    public Image omikujiBox;
    public TMP_Text omikujiResultText;
    public Button drawButton;
    public Button nextShineButton;
    public Button resultButton; // 結果画面へ遷移するボタン

    [Header("Background")]
    public Sprite[] backgrounds; // 背景画像をInspectorでセット
    public Image backgroundImage; // 背景表示用ImageをInspectorでセット

    [Header("Data")]
    public List<string> shrineNames = new List<string>{"金沢神社","尾山神社","石浦神社","白山神社"};
    private int currentShineIndex = 0;
    public int score = 100;

    [Header("Items")]
    public List<ItemData> allItems;
    private List<ItemData> shopItems = new List<ItemData>();
    private List<ItemData> inventoryItems = new List<ItemData>();

    private int omikujiCount = 0;

    // 効果用
    private int nextOmikujiBonusBig = 0;    // 大吉アップ
    private int nextOmikujiBonusKichi = 0;  // 吉アップ
    private int nextOmikujiDownDaikyo = 0;  // 大凶ダウン
    private bool nextOmikujiBless = false;  // 凶・大凶をプラス
    private bool nextOmikujiDouble = false; // 勝負運2倍

    void Start()
    {
    nextShineButton.gameObject.SetActive(false);
    if(resultButton != null) resultButton.gameObject.SetActive(false);
    SetupShine();
    UpdateUI();
    drawButton.onClick.AddListener(DrawOmikuji);
    nextShineButton.onClick.AddListener(NextShine);
    if(resultButton != null) resultButton.onClick.AddListener(GoToResult);
    }

    void SetupShine()
    {
        shrineNameText.text = shrineNames[currentShineIndex];
        omikujiCount = 0;
        omikujiResultText.text = "";
        nextShineButton.gameObject.SetActive(false);

        // 背景切り替え（神社ごと）
        if (backgrounds != null && backgrounds.Length > 0 && backgroundImage != null)
        {
            int bgIndex = currentShineIndex;
            bgIndex = Mathf.Clamp(bgIndex, 0, backgrounds.Length - 1);
            backgroundImage.sprite = backgrounds[bgIndex];
        }

        shopItems.Clear();
        shopItems.Add(allItems[currentShineIndex]);
        shopItems.Add(GetRandomItem());
        shopItems.Add(GetRandomItem());
        SpawnShopItems();
    }

    void SpawnShopItems()
    {
        foreach (Transform child in shopPanel) Destroy(child.gameObject);

        if(shopItems.Count == 0) return;

        float[] fixedPositions = new float[] { -75f, 0f, 75f };

        for (int i = 0; i < shopItems.Count && i < fixedPositions.Length; i++)
        {
            ItemData item = shopItems[i];
            GameObject btnObj = Instantiate(itemButtonPrefab, shopPanel);

            // アイコン設定
            btnObj.GetComponent<Image>().sprite = item.icon;

            // 購入ボタン機能
            btnObj.GetComponent<Button>().onClick.AddListener(() => BuyItem(item));

            // 配置
            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(fixedPositions[i], 0);
        }
    }

    void BuyItem(ItemData item)
    {
        if(inventoryItems.Count >= 3) return;
        if(score < item.cost) return;

        score -= item.cost;
        inventoryItems.Add(item);

        foreach (Transform child in shopPanel)
        {
            Image img = child.GetComponent<Image>();
            if(img != null && img.sprite == item.icon)
            {
                Destroy(child.gameObject);
                break;
            }
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        scoreText.text = "運気:" + score;

        foreach (Transform child in inventoryPanel) Destroy(child.gameObject);

        foreach (ItemData item in inventoryItems)
        {
            GameObject btnObj = Instantiate(itemButtonPrefab, inventoryPanel);
            Image img = btnObj.GetComponent<Image>();
            if(img != null) img.sprite = item.icon;

            Button btn = btnObj.GetComponent<Button>();
            if(btn != null)
            {
                btn.interactable = true;
                btn.onClick.AddListener(() => UseItem(item));
            }
        }
    }

    void UseItem(ItemData item)
    {
        switch(item.itemName)
        {
            case "豪運":
                nextOmikujiBonusBig = 45; // 大吉+45%
                break;
            case "至福":
                nextOmikujiBless = true;
                break;
            case "だるま":
                nextOmikujiBonusKichi = 45; // 吉+45%
                break;
            case "鈴":
                nextOmikujiDownDaikyo = 15; // 大凶-15%
                break;
            case "四つ葉":
                score += 20; // 即座に運気+20
                break;
            case "勝負運":
                nextOmikujiDouble = true; // スコア2倍
                break;
        }

        inventoryItems.Remove(item);
        UpdateUI();
    }

    public void DrawOmikuji()
    {
        if(omikujiCount >= 4) return;
        omikujiCount++;

        Animator anim = omikujiBox.GetComponent<Animator>();
        if(anim != null) anim.SetTrigger("Shake");

    float rand = Random.Range(0f, 100f);
    int scoreChange = 0;
    string resultText = "";

    // 基本値
    float baseDaikichi = 5;
    float baseKichi = 15;
    float baseShoukichi = 30;
    float baseKyoo = 30;
    float baseDaikyo = 20;

    // 豪運・だるま・鈴の効果
    float daikichiAdd = nextOmikujiBonusBig; // 豪運
    float kichiAdd = nextOmikujiBonusKichi; // だるま
    float daikyoDown = nextOmikujiDownDaikyo; // 鈴

    // 豪運・だるま・鈴の効果を反映
    float probDaikichi = baseDaikichi + daikichiAdd;
    float probKichi = baseKichi + kichiAdd;
    float probDaikyo = baseDaikyo - daikyoDown;

    // 残りを小吉・凶に分配
    float remain = 100f - probDaikichi - probKichi - probDaikyo;
    float baseSum = baseShoukichi + baseKyoo;
    float probShoukichi = remain * (baseShoukichi / baseSum);
    float probKyoo = remain * (baseKyoo / baseSum);

    // 0未満にならないように
    probDaikichi = Mathf.Max(0, probDaikichi);
    probKichi = Mathf.Max(0, probKichi);
    probShoukichi = Mathf.Max(0, probShoukichi);
    probKyoo = Mathf.Max(0, probKyoo);
    probDaikyo = Mathf.Max(0, probDaikyo);

        // 累積判定
        if(rand < probDaikichi){ resultText="大吉！"; scoreChange=50; }
        else if(rand < probDaikichi + probKichi){ resultText="吉"; scoreChange=30; }
        else if(rand < probDaikichi + probKichi + probShoukichi){ resultText="小吉"; scoreChange=10; }
        else if(rand < probDaikichi + probKichi + probShoukichi + probKyoo){ resultText="凶"; scoreChange=-20; if(nextOmikujiBless) scoreChange=20; }
        else{ resultText="大凶"; scoreChange=-40; if(nextOmikujiBless) scoreChange=40; }

        if(nextOmikujiDouble) scoreChange *= 2;

        score += scoreChange;
        omikujiResultText.text = resultText + " (" + (scoreChange>=0?"+":"") + scoreChange + ")";

        // 効果リセット
        nextOmikujiBonusBig = 0;
        nextOmikujiBonusKichi = 0;
        nextOmikujiDownDaikyo = 0;
        nextOmikujiBless = false;
        nextOmikujiDouble = false;

        UpdateUI();

        if(omikujiCount >= 4) nextShineButton.gameObject.SetActive(true);
    }

    void NextShine()
    {
        currentShineIndex++;
        if(currentShineIndex >= shrineNames.Count)
        {
            omikujiResultText.text = "最終スコア:" + score;
            nextShineButton.gameObject.SetActive(false);
            if(resultButton != null) resultButton.gameObject.SetActive(true); // 結果ボタン表示
            PlayerPrefs.SetInt("finalScore", score); // スコア保存
            return;
        }
        SetupShine();
        UpdateUI();
    }

    // 結果画面へ遷移する処理
    public void GoToResult()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("ResultScene");
    }
    

    ItemData GetRandomItem()
    {
        int index = Random.Range(currentShineIndex + 1, allItems.Count);
        return allItems[index];
    }
}

[System.Serializable]
public class ItemData
{
    public string itemName;
    public Sprite icon;
    public string description;
    public int cost;
}