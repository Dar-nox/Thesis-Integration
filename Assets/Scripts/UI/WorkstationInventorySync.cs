using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Bridges the global InventorySystem to pre-placed Workstation UI slots.
/// Maps Inventory itemIDs (from island collection) to DraggableItem slots
/// so collected herbs appear with correct counts in the Workstation scene.
/// </summary>
public class WorkstationInventorySync : MonoBehaviour
{
    [System.Serializable]
    public class IdRemap
    {
        [Tooltip("Slot itemData.itemID or binding ID to match (e.g., 'Ginger_Raw')")] public string from;
        [Tooltip("InventorySystem itemID to use instead (e.g., 'ginger')")] public string to;
    }

    [System.Serializable]
    public class SlotBinding
    {
        [Tooltip("Inventory itemID to read from InventorySystem (e.g., 'ginger')")] public string inventoryItemID;
        [Tooltip("UI slot to update (expects DraggableItem on the same GameObject)")] public DraggableItem slot;
        [Tooltip("Hide the slot GameObject if quantity is zero")] public bool hideWhenZero = true;
    }

    [Header("Bindings")]
    [Tooltip("Explicit bindings from Inventory itemIDs to UI slots. If left empty, auto-binds using slot.itemData.itemID.")]
    public List<SlotBinding> bindings = new List<SlotBinding>();

    [Header("Auto Bind")]
    [Tooltip("If true and bindings are empty, will auto-bind all DraggableItem children by using their itemData.itemID as the inventory key.")]
    public bool autoBindChildrenIfEmpty = true;

    [Header("Behavior")]
    [Tooltip("If true, will never reduce a pre-placed slot's count; only increases when Inventory has more.")]
    public bool onlyIncreaseCounts = false;
    [Tooltip("Default for auto-bound slots: hide the slot when quantity is zero.")]
    public bool defaultHideWhenZero = false;

    [Header("ID Mapping")]
    [Tooltip("Optional explicit remaps from slot IDs to inventory IDs (e.g., 'Ginger_Raw' → 'ginger')")]
    public List<IdRemap> idRemaps = new List<IdRemap>();

    [Header("Debug")] public bool showDebugLogs = false;

    void Awake()
    {
        // If no explicit bindings, try to auto-bind by scanning children
        if ((bindings == null || bindings.Count == 0) && autoBindChildrenIfEmpty)
        {
            AutoBindFromChildren();
        }
    }

    void OnEnable()
    {
        // Subscribe to inventory updates to live-sync counts
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.onInventoryChangedCallback += RefreshFromInventory;
        }
    }

    void OnDisable()
    {
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.onInventoryChangedCallback -= RefreshFromInventory;
        }
    }

    void Start()
    {
        RefreshFromInventory();
    }

    void AutoBindFromChildren()
    {
        bindings = new List<SlotBinding>();
        DraggableItem[] slots = GetComponentsInChildren<DraggableItem>(true);
        foreach (var slot in slots)
        {
            if (slot == null || slot.itemData == null) continue;
            string key = slot.itemData.itemID;
            if (string.IsNullOrEmpty(key)) continue;

            bindings.Add(new SlotBinding
            {
                inventoryItemID = key,
                slot = slot,
                hideWhenZero = defaultHideWhenZero
            });
        }

        if (showDebugLogs)
        {
            Debug.Log($"[WorkstationSync] Auto-bound {bindings.Count} slots from children");
        }
    }

    public void RefreshFromInventory()
    {
        if (bindings == null || bindings.Count == 0)
        {
            if (showDebugLogs) Debug.LogWarning("[WorkstationSync] No bindings configured.");
            return;
        }

        foreach (var binding in bindings)
        {
            if (binding == null || binding.slot == null) continue;

            int qty = 0;
            if (InventorySystem.Instance != null)
            {
                // Determine the key to use
                string sourceKey = binding.inventoryItemID;
                if (string.IsNullOrEmpty(sourceKey) && binding.slot.itemData != null)
                {
                    sourceKey = binding.slot.itemData.itemID;
                }

                string resolvedKey = ResolveInventoryKey(sourceKey);
                qty = string.IsNullOrEmpty(resolvedKey) ? 0 : InventorySystem.Instance.GetItemQuantity(resolvedKey);
            }

            // Compute the new count honoring behavior flags
            int existing = binding.slot.count;
            int newCount = qty;
            if (onlyIncreaseCounts)
            {
                newCount = Mathf.Max(existing, qty);
            }

            // Update the UI slot's count
            binding.slot.count = newCount;
            binding.slot.UpdateCountDisplay();

            // Hide or show slot based on quantity
            if (binding.hideWhenZero)
            {
                binding.slot.gameObject.SetActive(newCount > 0);
            }

            if (showDebugLogs)
            {
                string slotId = binding.slot.itemData != null ? binding.slot.itemData.itemID : "(null)";
                Debug.Log($"[WorkstationSync] Updated slot '{slotId}' to {newCount} (inv={qty}, existing={existing})");
            }
        }
    }

    string ResolveInventoryKey(string sourceKey)
    {
        if (string.IsNullOrEmpty(sourceKey)) return sourceKey;

        // 1) Explicit remap wins
        foreach (var map in idRemaps)
        {
            if (map == null) continue;
            if (!string.IsNullOrEmpty(map.from) && string.Equals(map.from, sourceKey, System.StringComparison.OrdinalIgnoreCase))
            {
                return map.to;
            }
        }

        // 2) Try exact match as-is
        if (InventorySystem.Instance != null && InventorySystem.Instance.HasItem(sourceKey))
        {
            return sourceKey;
        }

        // 3) Try normalized variants: lowercase, plural/singular, strip common suffixes like _Raw
        string lower = sourceKey.ToLowerInvariant();
        if (InventorySystem.Instance != null && InventorySystem.Instance.HasItem(lower))
        {
            return lower;
        }

        // Try add/remove trailing 's' on the lowercase key
        if (InventorySystem.Instance != null)
        {
            if (lower.EndsWith("s"))
            {
                string singular = lower.Substring(0, lower.Length - 1);
                if (InventorySystem.Instance.HasItem(singular)) return singular;
            }
            else
            {
                string plural = lower + "s";
                if (InventorySystem.Instance.HasItem(plural)) return plural;
            }
        }

        // Strip suffix after first underscore (e.g., Ginger_Raw → ginger)
        int usIndex = sourceKey.IndexOf('_');
        if (usIndex > 0)
        {
            string baseId = sourceKey.Substring(0, usIndex);
            string baseLower = baseId.ToLowerInvariant();
            if (InventorySystem.Instance != null)
            {
                if (InventorySystem.Instance.HasItem(baseId)) return baseId;
                if (InventorySystem.Instance.HasItem(baseLower)) return baseLower;
                // Try plural/singular on baseLower
                if (baseLower.EndsWith("s"))
                {
                    string singular = baseLower.Substring(0, baseLower.Length - 1);
                    if (InventorySystem.Instance.HasItem(singular)) return singular;
                }
                else
                {
                    string plural = baseLower + "s";
                    if (InventorySystem.Instance.HasItem(plural)) return plural;
                }
            }
        }

        // No match found; return original to keep logs intelligible
        return sourceKey;
    }
}


