#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class ShopDebugTools
{
    [MenuItem("Tools/Sprint & Steel/Debug/Reset Shop Upgrades + Currency")]
    static void ResetShopUpgradesAndCurrency()
    {
        ShopUpgrades.ResetAll();
        CurrencyWallet.Reset();

        if (Application.isPlaying)
            ShopUpgradeRuntime.ApplyToCurrentPlayer();

        Debug.Log("[Sprint & Steel] Shop upgrades reset and currency set to 0.");
    }

    [MenuItem("Tools/Sprint & Steel/Debug/Give 10000 Currency")]
    static void GiveTenThousandCurrency()
    {
        CurrencyWallet.Add(10000);
        Debug.Log("[Sprint & Steel] Added 10000 currency for testing.");
    }
}
#endif