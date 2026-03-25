using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    public static bool IsAnyShopOpen { get; private set; }

    [Header("References")]
    public GameObject boardRoot;
    public TextMeshProUGUI currencyText;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI[] titleTexts;
    public TextMeshProUGUI[] levelTexts;
    public TextMeshProUGUI[] descriptionTexts;
    public TextMeshProUGUI[] costTexts;
    public Button[] buyButtons;
    public Image[] rowBackgrounds;

    static readonly Color ColMuted = new Color(1f, 1f, 1f, 0.38f);
    static readonly Color ColSelected = new Color(1f, 1f, 1f, 0.14f);
    static readonly Color ColAffordable = new Color(1f, 1f, 1f, 0.07f);
    static readonly Color ColLocked = new Color(1f, 1f, 1f, 0.03f);
    static readonly Color ColMaxed = new Color(1f, 1f, 1f, 0.1f);

    static readonly Color[] UpgradeColors =
    {
        new Color(0.48f, 0.82f, 0.95f),
        new Color(0.53f, 0.90f, 0.60f),
        new Color(0.98f, 0.66f, 0.35f),
        new Color(0.95f, 0.42f, 0.42f),
        new Color(0.76f, 0.60f, 0.95f),
    };

    bool _isOpen;
    int _selectedIndex;

    void Start()
    {
        HookButtonEvents();
        if (boardRoot != null)
            boardRoot.SetActive(false);

        IsAnyShopOpen = false;
        _isOpen = false;
    }

    void OnDisable()
    {
        IsAnyShopOpen = false;
        _isOpen = false;
    }

    void Update()
    {
        if (!_isOpen)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Hide();
            return;
        }

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            MoveSelection(-1);

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            MoveSelection(1);

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            TryBuyByIndex(_selectedIndex);

        RefreshUI();
    }

    public void Show()
    {
        if (boardRoot == null)
            return;

        boardRoot.SetActive(true);
        _isOpen = true;
        IsAnyShopOpen = true;
        _selectedIndex = Mathf.Clamp(_selectedIndex, 0, Mathf.Max(0, ShopUpgrades.Count - 1));
        RefreshUI();
    }

    public void Hide()
    {
        if (boardRoot != null)
            boardRoot.SetActive(false);

        _isOpen = false;
        IsAnyShopOpen = false;

        if (statusText != null)
            statusText.text = "";
    }

    void HookButtonEvents()
    {
        if (buyButtons == null)
            return;

        for (int i = 0; i < buyButtons.Length; i++)
        {
            int idx = i;
            if (buyButtons[idx] == null)
                continue;

            buyButtons[idx].onClick.RemoveAllListeners();
            buyButtons[idx].onClick.AddListener(() => TryBuyByIndex(idx));
        }
    }

    void TryBuyByIndex(int index)
    {
        if (index < 0 || index >= ShopUpgrades.Count)
            return;

        var def = ShopUpgrades.GetDefinition(index);
        bool purchased = ShopUpgrades.TryPurchase(def.type, out string status);

        if (statusText != null)
            statusText.text = purchased ? $"Purchased: {status}" : status;

        RefreshUI();
    }

    void MoveSelection(int direction)
    {
        if (ShopUpgrades.Count <= 0)
            return;

        _selectedIndex += direction;
        if (_selectedIndex < 0)
            _selectedIndex = ShopUpgrades.Count - 1;
        if (_selectedIndex >= ShopUpgrades.Count)
            _selectedIndex = 0;
    }

    void RefreshUI()
    {
        if (currencyText != null)
            currencyText.text = "$" + CurrencyWallet.Current;

        for (int i = 0; i < ShopUpgrades.Count; i++)
        {
            var def = ShopUpgrades.GetDefinition(i);
            int level = ShopUpgrades.GetLevel(def.type);
            bool maxed = level >= def.maxLevel;
            int cost = ShopUpgrades.GetCost(def.type);
            bool affordable = CurrencyWallet.Current >= cost;
            bool selected = i == _selectedIndex;
            Color upgradeColor = UpgradeColors[i % UpgradeColors.Length];

            if (titleTexts != null && i < titleTexts.Length && titleTexts[i] != null)
            {
                titleTexts[i].text = def.title;
                titleTexts[i].color = selected ? upgradeColor : new Color(upgradeColor.r, upgradeColor.g, upgradeColor.b, 0.8f);
            }

            if (descriptionTexts != null && i < descriptionTexts.Length && descriptionTexts[i] != null)
                descriptionTexts[i].text = def.description;

            if (levelTexts != null && i < levelTexts.Length && levelTexts[i] != null)
                levelTexts[i].text = $"Lv {level}/{def.maxLevel}";

            if (costTexts != null && i < costTexts.Length && costTexts[i] != null)
                costTexts[i].text = maxed ? "MAX" : "$" + cost;

            if (buyButtons != null && i < buyButtons.Length && buyButtons[i] != null)
                buyButtons[i].interactable = !maxed && affordable;

            if (rowBackgrounds != null && i < rowBackgrounds.Length && rowBackgrounds[i] != null)
            {
                if (selected)
                    rowBackgrounds[i].color = ColSelected;
                else if (maxed)
                    rowBackgrounds[i].color = ColMaxed;
                else if (affordable)
                    rowBackgrounds[i].color = ColAffordable;
                else
                    rowBackgrounds[i].color = ColLocked;
            }

            if (costTexts != null && i < costTexts.Length && costTexts[i] != null)
                costTexts[i].color = maxed ? new Color(upgradeColor.r, upgradeColor.g, upgradeColor.b, 0.95f) : (affordable ? upgradeColor : ColMuted);

            if (buyButtons != null && i < buyButtons.Length && buyButtons[i] != null)
            {
                Image btnImage = buyButtons[i].targetGraphic as Image;
                if (btnImage != null)
                {
                    if (maxed)
                        btnImage.color = new Color(upgradeColor.r, upgradeColor.g, upgradeColor.b, 0.12f);
                    else if (affordable)
                        btnImage.color = selected
                            ? new Color(upgradeColor.r, upgradeColor.g, upgradeColor.b, 0.34f)
                            : new Color(upgradeColor.r, upgradeColor.g, upgradeColor.b, 0.2f);
                    else
                        btnImage.color = new Color(0.45f, 0.45f, 0.45f, 0.16f);
                }
            }
        }

        if (statusText != null && string.IsNullOrEmpty(statusText.text) && ShopUpgrades.Count > 0)
        {
            var selectedDef = ShopUpgrades.GetDefinition(_selectedIndex);
            statusText.text = $"Selected: {selectedDef.title}";
        }
    }
}