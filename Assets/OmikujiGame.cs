using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class OmikujiGame : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text shrineNameText;
    public TMP_Text scoreText;
    public Transform shopPanel;       // 右上ショップ
    public Transform inventoryPanel;  // 下側手持ちアイテム
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
    public List<ItemData> allItems; // Inspectorで仮アイテムを登録
    private List<ItemData> shopItems = new List<ItemData>();
    private List<ItemData> inventoryItems = new List<ItemData>();

    private int omikujiCount = 0; // 1神社4回

    // 効果用
    private int nextOmikujiBonus = 0;
    private bool nextOmikujiBless = false;

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
        shopItems.Add(allItems[currentShineIndex]); // 固定お守り
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

        // ショップのボタンを削除
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
        if(item.itemName == "豪運")
        {
            nextOmikujiBonus = 90; // 次のくじで大吉+45%
        }
        else if(item.itemName == "至福")
        {
            nextOmikujiBless = true; // 次の凶・大凶をプラスに
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
        if(rand < 5 + nextOmikujiBonus){ resultText="大吉！"; scoreChange=50; }
        else if(rand < 20){ resultText="吉"; scoreChange=30; }
        else if(rand < 50){ resultText="小吉"; scoreChange=10; }
        else if(rand < 80){ resultText="凶"; scoreChange=-20; if(nextOmikujiBless) scoreChange=20; }
        else{ resultText="大凶"; scoreChange=-40; if(nextOmikujiBless) scoreChange=40; }

        score += scoreChange;
        omikujiResultText.text = resultText + " (" + (scoreChange>=0?"+":"") + scoreChange + ")";

        // 効果リセット
        nextOmikujiBonus = 0;
        nextOmikujiBless = false;

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