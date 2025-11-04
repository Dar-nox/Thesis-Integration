using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class Customer : MonoBehaviour
{
    public Illness illness;

    [Header("Dialogue UI")]
    [Tooltip("Panel GameObject (disable in inspector). Will be enabled when customer arrives.")]
    public GameObject speechPanel;
    [Tooltip("TextMeshProUGUI component inside the panel to show the dialogue.")]
    public TextMeshProUGUI dialogueText;
    [Header("Speech Animation")]
    [Tooltip("Scale multiplier for the pop animation (1 = normal, 1.3 = 30% larger at peak)")]
    public float popScale = 1.25f;
    [Tooltip("Total duration of the pop animation in seconds")]
    public float popDuration = 0.18f;
    [Tooltip("Delay after pop before starting typewriter")]
    public float popToTypeDelay = 0.06f;
    [Tooltip("Seconds per character for the typewriter effect (smaller = faster)")]
    public float typewriterSpeed = 0.025f;
    [Tooltip("Optional: auto-hide the speech panel after this many seconds (0 = never hide)")]
    public float hideAfterSeconds = 0f;

    // Internal coroutine trackers so we can cancel if needed
    Coroutine popCoroutine;
    Coroutine typeCoroutine;

    // Dialogue map: illness name -> array of possible lines
    // You can edit these defaults in code or extend via inspector logic later
    private static readonly Dictionary<string, string[]> DialogueMap = new Dictionary<string, string[]>
    {
        { "Cough", new string[] { "Hey there, I've been coughing nonstop for days.", "This cough won’t stop no matter what I do. Can you make me something to ease it?" } },
        { "Colds", new string[] { "I can’t stop sneezing and my nose is clogged up. I think I caught a bad cold.", "My nose keeps running and my head feels heavy. Got any herbal cure for colds?" } },
        { "Asthma", new string[] { "I’m having trouble catching my breath again. My asthma’s flaring up.", "It feels like my chest is tightening. Do you have something to help me breathe easier?" } },
        { "Nausea", new string[] { "My stomach feels unsettled and I feel like throwing up. Please, help me out.", "I’ve been feeling queasy all morning. Can you brew something to calm my stomach?" } },
        { "Sore Throat", new string[] { "It hurts every time I try to talk. My throat feels raw and irritated.", "I can barely swallow anything. Can you make something to soothe my throat?" } },
        { "High Blood Pressure", new string[] { "I’ve been feeling dizzy and my heart feels like it’s pounding harder than usual.", "My doctor warned me about my blood pressure. I need something to help calm it down." } },
        { "Fever", new string[] { "Hey there, I think I have a fever. My body’s burning up.", "I feel hot and weak all over. Do you have anything to bring my temperature down?" } },
        { "Stomach Pain", new string[] { "My stomach’s cramping badly after I ate. It feels like it’s twisting inside.", "I’ve been hunched over from this pain. Can you make something for stomach aches?" } },
        { "Headache", new string[] { "My head’s pounding like it’s about to burst. I can’t focus on anything.", "There’s this sharp pain behind my eyes. Do you have any herbs for headaches?" } },
        { "Body Pain", new string[] { "My whole body feels sore and stiff. I can barely move.", "Every muscle hurts when I try to get up. Can you help ease the pain?" } },
        { "Inflammation", new string[] { "This swelling won’t go away. It’s red and sore to touch.", "My arm’s inflamed and feels warm. I need something to reduce it." } },
        { "Wound", new string[] { "I accidentally cut myself while working. It’s not healing well.", "This wound keeps stinging and looks like it might get infected. Can you treat it?" } },
        { "Diarrhea", new string[] { "I’ve been running to the bathroom all day. My stomach won’t settle.", "I think I ate something bad. Can you give me something to stop this diarrhea?" } },
        { "Gum problems", new string[] { "My gums are sore and bleed whenever I eat.", "It hurts to chew. I think my gums are infected — can you help?" } },
        { "Eye Pain", new string[] { "My eyes hurt whenever I blink. They’ve been red for hours.", "I can barely keep my eyes open — they sting and water constantly." } },
        { "Menstrual Cramps", new string[] { "My stomach’s cramping so badly I can’t even stand straight.", "These menstrual pains are unbearable. I need something to ease them." } },
    };

    /// <summary>
    /// Assigns an illness to this customer. Also updates the GameObject name for debugging.
    /// </summary>
    /// <param name="i"></param>
    public void SetIllness(Illness i)
    {
        illness = i;
        if (i != null)
        {
            gameObject.name = "Customer - " + i.name;
            Debug.Log($"Customer assigned illness: {i.name} (rarity={i.rarity}, weight={i.weight})");
        }
        else
        {
            gameObject.name = "Customer - (no illness)";
        }
    }

    /// <summary>
    /// Called when the customer has finished moving to the target.
    /// Enables the speech panel and shows a randomized dialogue line based on the assigned illness.
    /// </summary>
    public void OnArrived()
    {
        if (speechPanel != null)
            speechPanel.SetActive(true);

        // Start pop animation then typewriter
        // Cancel existing coroutines if any
        if (popCoroutine != null) StopCoroutine(popCoroutine);
        if (typeCoroutine != null) StopCoroutine(typeCoroutine);

        // Decide the line first
        string line = PickDialogueLine();

        // Start the pop animation and then the typewriter
        popCoroutine = StartCoroutine(PlayPopThenType(line));
    }

    void OnDisable()
    {
        CancelDialogueCoroutines();
    }

    void OnDestroy()
    {
        CancelDialogueCoroutines();
    }

    void CancelDialogueCoroutines()
    {
        if (popCoroutine != null)
        {
            StopCoroutine(popCoroutine);
            popCoroutine = null;
        }
        if (typeCoroutine != null)
        {
            StopCoroutine(typeCoroutine);
            typeCoroutine = null;
        }
    }

    string PickDialogueLine()
    {
        if (illness == null)
        {
            Debug.LogWarning("Customer: illness is null, cannot pick dialogue");
            return "...";
        }

        string key = (illness.name ?? string.Empty).Trim();
        Debug.Log($"PickDialogueLine: Looking up dialogue for illness name: '{key}'");

        // Try direct lookup first (exact key including case)
        if (DialogueMap.TryGetValue(key, out var lines) && lines.Length > 0)
        {
            string chosen = lines[Random.Range(0, lines.Length)];
            Debug.Log($"PickDialogueLine: Found exact match for '{key}': {chosen}");
            return chosen;
        }

        // Try case-insensitive match
        string normalized = key.ToLowerInvariant();
        foreach (var kvp in DialogueMap)
        {
            if (kvp.Key != null && kvp.Key.Trim().ToLowerInvariant() == normalized && kvp.Value != null && kvp.Value.Length > 0)
            {
                string chosen = kvp.Value[Random.Range(0, kvp.Value.Length)];
                Debug.Log($"PickDialogueLine: Found case-insensitive match for '{key}' (matched '{kvp.Key}'): {chosen}");
                return chosen;
            }
        }

        // Try simple singular/plural fallback
        string alt = normalized.EndsWith("s") ? normalized.Substring(0, normalized.Length - 1) : normalized + "s";
        foreach (var kvp in DialogueMap)
        {
            if (kvp.Key != null && kvp.Key.Trim().ToLowerInvariant() == alt && kvp.Value != null && kvp.Value.Length > 0)
            {
                string chosen = kvp.Value[Random.Range(0, kvp.Value.Length)];
                Debug.Log($"PickDialogueLine: Found plural/singular fallback for '{key}' -> '{alt}' (matched '{kvp.Key}'): {chosen}");
                return chosen;
            }
        }

        // If nothing matched, log detailed error and return placeholder
        Debug.LogError($"PickDialogueLine: NO MATCH FOUND for illness name '{key}'. Available keys in DialogueMap: {string.Join(", ", DialogueMap.Keys)}");
        return "...";
    }

    System.Collections.IEnumerator PlayPopThenType(string fullText)
    {
        // Ensure panel scale starts at 0 for a clear pop; preserve original scale
        Transform panelTransform = speechPanel != null ? speechPanel.transform : null;
        Vector3 originalScale = panelTransform != null ? panelTransform.localScale : Vector3.one;

        // Start small then pop
        if (panelTransform != null)
            panelTransform.localScale = Vector3.zero;

        float half = popDuration * 0.5f;
        // grow to popScale
        float elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / half);
            float s = Mathf.SmoothStep(0f, popScale, t);
            panelTransform.localScale = originalScale * s;
            yield return null;
        }

        // shrink back to normal scale
        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / half);
            float s = Mathf.SmoothStep(popScale, 1f, t);
            panelTransform.localScale = originalScale * s;
            yield return null;
        }

        // Ensure final scale
        if (panelTransform != null)
            panelTransform.localScale = originalScale;

        // small delay before typing
        if (popToTypeDelay > 0f)
            yield return new WaitForSeconds(popToTypeDelay);

        // Start typewriter
        typeCoroutine = StartCoroutine(Typewriter(fullText));
    }

    System.Collections.IEnumerator Typewriter(string fullText)
    {
        if (dialogueText == null)
            yield break;

        dialogueText.text = string.Empty;
        for (int i = 0; i < fullText.Length; i++)
        {
            dialogueText.text += fullText[i];
            yield return new WaitForSeconds(typewriterSpeed);
        }

        typeCoroutine = null;

        // Optionally hide after a delay
        if (hideAfterSeconds > 0f)
        {
            yield return new WaitForSeconds(hideAfterSeconds);
            if (speechPanel != null)
                speechPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Picks a dialogue line for the current illness and sets it into the dialogueText.
    /// </summary>
    public void ShowDialogueForIllness()
    {
        if (illness == null)
        {
            if (dialogueText != null)
                dialogueText.text = "...";
            return;
        }

        string key = (illness.name ?? string.Empty).Trim();

        // Try direct lookup first (exact key)
        if (DialogueMap.TryGetValue(key, out var lines) && lines.Length > 0)
        {
            string chosen = lines[Random.Range(0, lines.Length)];
            if (dialogueText != null)
                dialogueText.text = chosen;
            Debug.Log($"Showing dialogue for {key}: {chosen}");
            return;
        }

        // Try case-insensitive match by normalizing keys
        string normalized = key.ToLowerInvariant();
        var match = DialogueMap.FirstOrDefault(kvp => kvp.Key != null && kvp.Key.Trim().ToLowerInvariant() == normalized);
        if (match.Value != null && match.Value.Length > 0)
        {
            string chosen = match.Value[Random.Range(0, match.Value.Length)];
            if (dialogueText != null)
                dialogueText.text = chosen;
            Debug.Log($"Showing dialogue for (case-insensitive) {key}: {chosen}");
            return;
        }

        // Try simple singular/plural fallback (remove/add trailing 's')
        string alt = normalized.EndsWith("s") ? normalized.Substring(0, normalized.Length - 1) : normalized + "s";
        match = DialogueMap.FirstOrDefault(kvp => kvp.Key != null && kvp.Key.Trim().ToLowerInvariant() == alt);
        if (match.Value != null && match.Value.Length > 0)
        {
            string chosen = match.Value[Random.Range(0, match.Value.Length)];
            if (dialogueText != null)
                dialogueText.text = chosen;
            Debug.Log($"Showing dialogue for (plural/singular fallback) {key}: {chosen}");
            return;
        }

        // Last resort: show the illness name and warn
        if (dialogueText != null)
            dialogueText.text = illness.name;
        Debug.LogWarning($"No dialogue entry found for illness '{key}' (checked case-insensitive and plural/singular variants)");
    }
}

