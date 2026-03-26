#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class ShopDebugTools
{
    const string ContinueSaveKey = "HasSaveData";

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

    [MenuItem("Tools/Sprint & Steel/Debug/Reset Continue Save Flag")]
    static void ResetContinueSaveFlag()
    {
        PlayerPrefs.DeleteKey(ContinueSaveKey);
        PlayerPrefs.Save();
        Debug.Log("[Sprint & Steel] Continue save flag cleared.");
    }

    [MenuItem("Tools/Sprint & Steel/Debug/Reset ALL Save Data")]
    static void ResetAllSaveData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        if (Application.isPlaying)
            ShopUpgradeRuntime.ApplyToCurrentPlayer();

        Debug.Log("[Sprint & Steel] All PlayerPrefs save data cleared.");
    }
}
#endif