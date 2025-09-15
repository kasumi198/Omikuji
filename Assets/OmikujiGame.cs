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

    [Header("Data")]
    public List<string> shrineNames = new List<string>{"金沢","尾山","石浦","白山"};
    private int currentShineIndex = 0;
    public int score = 100;

    [Header("Items")]
    public List<ItemData> allItems; 
    private List<ItemData> shopItems = new List<ItemData>();
    private List<ItemData> inventoryItems = new List<ItemData>();

    private int omikujiCount = 0;

    // 効果フラグ
    private int nextOmikujiBonus = 0; 
    private bool nextOmikujiBless = false;
    private bool nextOmikujiSuzu = false;
    private bool nextOmikujiDaruma = false;
    private bool nextOmikujiDouble = false;
    private bool nextOmikujiTriple = false;

    void Start()
    {
        nextShineButton.gameObject.SetActive(false);
        SetupShine();
        UpdateUI();
        drawButton.onClick.AddListener(DrawOmikuji);
        nextShineButton.onClick.AddListener(NextShine);
    }

    void SetupShine()
    {
        shrineNameText.text = shrineNames[currentShineIndex];
        omikujiCount = 0;
        omikujiResultText.text = "";
        nextShineButton.gameObject.SetActive(false);

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
            btnObj.GetComponent<Image>().sprite = item.icon;
            btnObj.GetComponent<Button>().onClick.AddListener(() => BuyItem(item));

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
            case "豪運": nextOmikujiBonus = 45; break; // 次の大吉+45%
            case "至福": nextOmikujiBless = true; break; // 次の凶・大凶をプラス
            case "四つ葉": score += 20; break; // 即座に運気+20
            case "勝負運": nextOmikujiDouble = true; break; // 次のくじスコア2倍
            case "福返し": nextOmikujiTriple = true; break; // 大吉のときスコア3倍
            case "だるま": nextOmikujiDaruma = true; break; // 吉確率アップ
            case "鈴": nextOmikujiSuzu = true; break; // 吉＋小吉確率アップ
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

        int rand = Random.Range(0, 100);
        int scoreChange = 0;
        string resultText = "";

        // 鈴・だるまなど確率アップ
        if(nextOmikujiSuzu) 
        {
            if(rand < 75) { resultText = "小吉"; scoreChange = 10; }
            else { resultText = "吉"; scoreChange = 30; }
        }
        else if(nextOmikujiDaruma) 
        {
            resultText = "吉"; scoreChange = 30;
        }
        else // 通常
        {
            if(rand < 5 + nextOmikujiBonus){ resultText="大吉"; scoreChange=50; }
            else if(rand < 20){ resultText="吉"; scoreChange=30; }
            else if(rand < 50){ resultText="小吉"; scoreChange=10; }
            else if(rand < 80){ resultText="凶"; scoreChange=-20; if(nextOmikujiBless) scoreChange=20; }
            else{ resultText="大凶"; scoreChange=-40; if(nextOmikujiBless) scoreChange=40; }
        }

        // スコア倍率
        if(nextOmikujiDouble) scoreChange *= 2;
        if(nextOmikujiTriple && resultText=="大吉") scoreChange *= 3;

        score += scoreChange;
        omikujiResultText.text = resultText + " (" + (scoreChange>=0?"+":"") + scoreChange + ")";

        // 効果リセット（1回のみ）
        nextOmikujiBonus = 0;
        nextOmikujiBless = false;
        nextOmikujiSuzu = false;
        nextOmikujiDaruma = false;
        nextOmikujiDouble = false;
        nextOmikujiTriple = false;

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
            return;
        }
        SetupShine();
        UpdateUI();
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