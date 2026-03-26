using UnityEngine;

public class ShopkeeperNPC : MonoBehaviour
{
    [Header("Interaction")]
    public string playerTag = "Player";
    public GameObject interactPrompt;

    bool _playerInRange;

    void Start()
    {
        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    void Update()
    {
        if (!_playerInRange)
            return;

        if (Input.GetKeyDown(KeyCode.E))
            ToggleShop();

        if (Input.GetKeyDown(KeyCode.X))
        {
            ShopUI shop = FindFirstObjectByType<ShopUI>();
            if (shop != null)
                shop.Hide();
        }
    }

    void ToggleShop()
    {
        ShopUI shop = FindFirstObjectByType<ShopUI>();
        if (shop == null)
        {
            Debug.LogWarning("[ShopkeeperNPC] No ShopUI found in scene.");
            return;
        }

        if (ShopUI.IsAnyShopOpen)
            shop.Hide();
        else
            shop.Show();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
            return;

        _playerInRange = true;
        if (interactPrompt != null)
            interactPrompt.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
            return;

        _playerInRange = false;
        if (interactPrompt != null)
            interactPrompt.SetActive(false);

        ShopUI shop = FindFirstObjectByType<ShopUI>();
        if (shop != null)
            shop.Hide();
    }
}