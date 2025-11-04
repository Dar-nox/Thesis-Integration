using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Illnesses (set weights and rarity in inspector)")]
    public List<Illness> illnesses = new List<Illness>();

    [Header("Customer / spawn settings")]
    public GameObject customerPrefab; // optional - if null we'll create an empty GameObject
    public Transform centerPoint; // target position where customer stops (set in scene)
    public float spawnOffsetX = 10f; // how far off-screen to spawn on the left
    public float moveDuration = 1.2f;
    public bool spawnOnStart = true; // for quick testing
    [Tooltip("Horizontal offset applied to the arrival point relative to centerPoint. Negative moves left.")]
    public float targetOffsetX = -3f;
    [Header("Speech UI (assign scene objects here)")]
    [Tooltip("Assign the Speech Bubble Panel from the scene (keep it disabled). Prefab cannot reference scene objects, so we set this at runtime.")]
    public GameObject speechPanel;
    [Tooltip("Assign the TextMeshProUGUI component inside the speech panel to show dialogue.")]
    public TextMeshProUGUI dialogueText;
    [Tooltip("Vertical offset applied to both spawn and target Y positions (useful to raise/lower customers)")]
    public float verticalOffset = 1f;

    // Rarity multipliers can be tuned as needed
    const float RARITY_COMMON = 1f;
    const float RARITY_UNCOMMON = 0.6f;
    const float RARITY_RARE = 0.3f;

    void Start()
    {
        if (spawnOnStart)
        {
            SpawnCustomer();
        }
    }

    /// <summary>
    /// Spawns a customer from the left and moves them to the centerPoint. The customer's illness
    /// is selected via weighted (roulette-wheel) selection based on per-illness weights and rarity.
    /// </summary>
    public void SpawnCustomer()
    {
        if (centerPoint == null)
        {
            Debug.LogWarning("GameManager: centerPoint is not set. Using Vector3.zero as target.");
        }

        Illness chosen = GetWeightedRandomIllness();

    Vector3 targetPos = centerPoint != null ? centerPoint.position : Vector3.zero;
    // Apply horizontal offset so the arrival point can be left/right of the center
    targetPos.x += targetOffsetX;
    // Adjust target Y and force target Z = 0 so customers always sit on the same plane
    targetPos.y += verticalOffset;
    targetPos.z = 0f;
    Vector3 spawnPos = targetPos + new Vector3(-Mathf.Abs(spawnOffsetX), 0f, 0f);
    spawnPos.y += 0f; // spawn starts with same Y as target + any additional if desired
    spawnPos.z = 0f;

        GameObject go;
        if (customerPrefab != null)
        {
            go = Instantiate(customerPrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            go = new GameObject("Customer");
            go.transform.position = spawnPos;
        }

        // Ensure a Customer component exists to receive the illness data
        Customer cust = go.GetComponent<Customer>();
        if (cust == null)
        {
            cust = go.AddComponent<Customer>();
        }

        // Assign scene UI references to the runtime instance so prefabs don't need scene references
        if (cust != null)
        {
            cust.speechPanel = speechPanel;
            cust.dialogueText = dialogueText;
        }

        if (chosen != null)
        {
            cust.SetIllness(chosen);
        }
        else
        {
            Debug.LogWarning("GameManager: No illness available to assign to customer.");
        }

        // Pass the Customer component so we can notify it when arrival completes
        StartCoroutine(MoveToTarget(go.transform, spawnPos, targetPos, moveDuration, cust));
    }

    IEnumerator MoveToTarget(Transform t, Vector3 from, Vector3 to, float duration, Customer customer)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float p = Mathf.Clamp01(elapsed / duration);
            // Smooth step interpolation
            float ease = p * p * (3f - 2f * p);
            Vector3 pos = Vector3.Lerp(from, to, ease);
            pos.z = 0f; // ensure z stays at zero during movement
            t.position = pos;
            yield return null;
        }
        to.z = 0f;
        t.position = to;
        // Notify the customer that it has arrived
        if (customer != null)
        {
            customer.OnArrived();
        }
    }

    /// <summary>
    /// Weighted (roulette-wheel) selection for illnesses. Each illness's effective weight is:
    /// illness.weight * rarityMultiplier(rarity).
    /// </summary>
    /// <returns>Chosen Illness or null if list empty</returns>
    public Illness GetWeightedRandomIllness()
    {
        if (illnesses == null || illnesses.Count == 0)
            return null;

        // Compute total
        float total = illnesses.Sum(i => Mathf.Max(0f, i.weight) * GetRarityMultiplier(i.rarity));
        if (total <= 0f)
            return null;

        float r = Random.value * total;
        float accum = 0f;
        foreach (var ill in illnesses)
        {
            float w = Mathf.Max(0f, ill.weight) * GetRarityMultiplier(ill.rarity);
            accum += w;
            if (r <= accum)
                return ill;
        }

        // Fallback (shouldn't happen): return last
        return illnesses[illnesses.Count - 1];
    }

    float GetRarityMultiplier(Rarity r)
    {
        switch (r)
        {
            case Rarity.Common: return RARITY_COMMON;
            case Rarity.Uncommon: return RARITY_UNCOMMON;
            case Rarity.Rare: return RARITY_RARE;
            default: return 1f;
        }
    }
}
