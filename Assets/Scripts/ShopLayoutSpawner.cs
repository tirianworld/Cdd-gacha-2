using UnityEngine;
using UnityEngine.UI;

public class ShopLayoutSpawner : MonoBehaviour
{
    public Transform parent;
    public GameObject productPrefab;
    public ShopManager shopManager;

    private void Start()
    {
        if (shopManager == null) shopManager = ShopManager.Instance;
        Spawn();
    }

    public void Spawn()
    {
        if (parent == null || productPrefab == null || shopManager == null) return;

        foreach (Transform t in parent)
            Destroy(t.gameObject);

        foreach (var prod in shopManager.products)
        {
            if (prod == null) continue;
            var obj = Instantiate(productPrefab, parent);
            var ui = obj.GetComponent<ShopProductUI>();
            if (ui != null)
            {
                ui.product = prod;
                ui.Refresh();
            }
        }
    }
}
