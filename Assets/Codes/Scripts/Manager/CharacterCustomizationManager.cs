using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Core customization system that handles runtime avatar part swapping.
/// Manages both male and female variants with optimized component caching.
/// 
/// Key Features:
/// - Runtime part swapping without performance hitches
/// - Gender-specific part collections
/// - Component reference caching for performance
/// - UI integration for user interaction
/// </summary>
public class CharacterCustomizationManager : MonoBehaviour
{
    /// <summary>
    /// Container for avatar part attachment slots.
    /// Each Transform represents where a specific part type should be attached.
    /// </summary>
    [System.Serializable]
    public class PartSlots
    {
        public Transform hairSlot;
        public Transform topSlot;
        public Transform bottomSlot;
        public Transform shoesSlot;
        public Transform accessorySlot;
    }

    /// <summary>
    /// Container for gender-specific part arrays.
    /// Allows different part sets for male and female characters.
    /// </summary>
    [System.Serializable]
    public class GenderedPartArray
    {
        public AvatarPartData[] maleParts;
        public AvatarPartData[] femaleParts;
    }

    [Header("Character Models")]
    [SerializeField] private PartSlots maleSlots;
    [SerializeField] private PartSlots femaleSlots;

    [Header("Gendered Part Data")]
    [SerializeField] private GenderedPartArray hairParts;
    [SerializeField] private GenderedPartArray topParts;
    [SerializeField] private GenderedPartArray bottomParts;
    [SerializeField] private GenderedPartArray shoesParts;
    [SerializeField] private GenderedPartArray accessoryParts;

    [Header("UI Buttons")]
    [SerializeField] private Button changeHairButton;
    [SerializeField] private Button changeTopButton;
    [SerializeField] private Button changeBottomButton;
    [SerializeField] private Button changeShoesButton;
    [SerializeField] private Button changeAccessoryButton;

    // Runtime state
    private PartSlots activeSlots;
    private AvatarPartData[] currentHairParts;
    private AvatarPartData[] currentTopParts;
    private AvatarPartData[] currentBottomParts;
    private AvatarPartData[] currentShoesParts;
    private AvatarPartData[] currentAccessoryParts;

    // Cached component references for performance
    private MeshRenderer hairRenderer;
    private MeshFilter hairFilter;
    private SkinnedMeshRenderer topRenderer;
    private SkinnedMeshRenderer bottomRenderer;
    private SkinnedMeshRenderer shoesRenderer;
    private MeshRenderer accessoryRenderer;
    private MeshFilter accessoryFilter;

    // Part indices for cycling
    private int currentHairIndex = 0;
    private int currentTopIndex = 0;
    private int currentBottomIndex = 0;
    private int currentShoesIndex = 0;
    private int currentAccessoryIndex = 0;

    /// <summary>
    /// Sets the active character slot set and updates part arrays.
    /// This is called when switching between male and female characters.
    /// 
    /// Performance Note: Clears and rebuilds component cache to avoid stale references.
    /// </summary>
    /// <param name="isMale">True for male slots, false for female slots</param>
    public void SetActiveSlots(bool isMale)
    {
        // Switch to appropriate slot set
        activeSlots = isMale ? maleSlots : femaleSlots;

        currentHairParts = isMale ? hairParts.maleParts : hairParts.femaleParts;
        currentTopParts = isMale ? topParts.maleParts : topParts.femaleParts;
        currentBottomParts = isMale ? bottomParts.maleParts : bottomParts.femaleParts;
        currentShoesParts = isMale ? shoesParts.maleParts : shoesParts.femaleParts;
        currentAccessoryParts = isMale ? accessoryParts.maleParts : accessoryParts.femaleParts;

        // Rebuild component cache for new slots
        ClearComponentReferences();
        UpdateComponentReferences();

        Debug.Log($"Active slots set to: {(isMale ? "Male" : "Female")}");
    }

    void Start()
    {
        SetupButtonListeners();
        InitializeCustomization();
    }

    /// <summary>
    /// Connects UI buttons to their respective customization functions.
    /// Uses lambda expressions for clean button-to-function mapping.
    /// </summary>
    private void SetupButtonListeners()
    {
        changeHairButton?.onClick.AddListener(() => CyclePart(currentHairParts, ref currentHairIndex, AvatarPartType.Hair));
        changeTopButton?.onClick.AddListener(() => CyclePart(currentTopParts, ref currentTopIndex, AvatarPartType.Top));
        changeBottomButton?.onClick.AddListener(() => CyclePart(currentBottomParts, ref currentBottomIndex, AvatarPartType.Bottom));
        changeShoesButton?.onClick.AddListener(() => CyclePart(currentShoesParts, ref currentShoesIndex, AvatarPartType.Shoes));
        changeAccessoryButton?.onClick.AddListener(() => CyclePart(currentAccessoryParts, ref currentAccessoryIndex, AvatarPartType.Accessory));
    }

    // <summary>
    /// Initializes the customization system with default parts.
    /// Should be called after SetActiveSlots to ensure proper setup.
    /// </summary>
    public void InitializeCustomization()
    {
        if (activeSlots == null)
        {
            Debug.LogWarning("Active slots not set! Make sure to call SetActiveSlots first.");
            return;
        }

        UpdateComponentReferences();

        // Initialize all parts to their first index
        UpdatePart(currentHairParts, ref currentHairIndex, AvatarPartType.Hair);
        UpdatePart(currentTopParts, ref currentTopIndex, AvatarPartType.Top);
        UpdatePart(currentBottomParts, ref currentBottomIndex, AvatarPartType.Bottom);
        UpdatePart(currentShoesParts, ref currentShoesIndex, AvatarPartType.Shoes);
        UpdatePart(currentAccessoryParts, ref currentAccessoryIndex, AvatarPartType.Accessory);
    }

    /// <summary>
    /// Clears all cached component references.
    /// Used when switching between character models to prevent stale references.
    /// </summary>
    private void ClearComponentReferences()
    {
        hairRenderer = null;
        hairFilter = null;
        topRenderer = null;
        bottomRenderer = null;
        shoesRenderer = null;
        accessoryRenderer = null;
        accessoryFilter = null;
    }

    // <summary>
    /// Updates cached component references for the current active slots.
    /// </summary>
    private void UpdateComponentReferences()
    {
        if (activeSlots == null) return;

        if (activeSlots.hairSlot != null)
        {
            hairRenderer = activeSlots.hairSlot.GetComponentInChildren<MeshRenderer>();
            hairFilter = activeSlots.hairSlot.GetComponentInChildren<MeshFilter>();
        }

        if (activeSlots.topSlot != null)
        {
            topRenderer = activeSlots.topSlot.GetComponentInChildren<SkinnedMeshRenderer>();
        }

        if (activeSlots.bottomSlot != null)
        {
            bottomRenderer = activeSlots.bottomSlot.GetComponentInChildren<SkinnedMeshRenderer>();
        }

        if (activeSlots.shoesSlot != null)
        {
            shoesRenderer = activeSlots.shoesSlot.GetComponentInChildren<SkinnedMeshRenderer>();
        }

        if (activeSlots.accessorySlot != null)
        {
            accessoryRenderer = activeSlots.accessorySlot.GetComponentInChildren<MeshRenderer>();
            accessoryFilter = activeSlots.accessorySlot.GetComponentInChildren<MeshFilter>();
        }
    }

    /// <summary>
    /// Updates a specific part with new data.
    /// Handles both MeshRenderer and SkinnedMeshRenderer components.
    /// 
    /// Performance: Uses cached component references to avoid GetComponent calls.
    /// </summary>
    /// <param name="parts">Array of available parts for this slot</param>
    /// <param name="index">Current index in the parts array</param>
    /// <param name="partType">Type of part being updated</param>
    private void UpdatePart(AvatarPartData[] parts, ref int index, AvatarPartType partType)
    {
        if (parts == null || parts.Length == 0) return;

        // Clamp index to valid range
        if (index < 0 || index >= parts.Length) index = 0;

        AvatarPartData partData = parts[index];
        if (partData == null) return;

        // Route to appropriate renderer type
        switch (partType)
        {
            case AvatarPartType.Hair:
                UpdateMeshRendererPart(hairRenderer, hairFilter, partData);
                break;
            case AvatarPartType.Top:
                UpdateSkinnedMeshRendererPart(topRenderer, partData);
                break;
            case AvatarPartType.Bottom:
                UpdateSkinnedMeshRendererPart(bottomRenderer, partData);
                break;
            case AvatarPartType.Shoes:
                UpdateSkinnedMeshRendererPart(shoesRenderer, partData);
                break;
            case AvatarPartType.Accessory:
                UpdateMeshRendererPart(accessoryRenderer, accessoryFilter, partData);
                break;
        }
    }

    /// <summary>
    /// Updates MeshRenderer-based parts (hair, accessories).
    /// Handles null meshes by disabling the renderer.
    /// </summary>
    /// <param name="renderer">Target MeshRenderer component</param>
    /// <param name="filter">Target MeshFilter component</param>
    /// <param name="partData">Part data to apply</param>
    private void UpdateMeshRendererPart(MeshRenderer renderer, MeshFilter filter, AvatarPartData partData)
    {
        if (renderer == null || filter == null) return;

        // Handle null mesh (hide part)
        if (partData.avatarPartMesh == null)
        {
            filter.sharedMesh = null;
            renderer.sharedMaterials = new Material[0];
            renderer.enabled = false;
            return;
        }

        // Apply mesh and materials
        filter.sharedMesh = partData.avatarPartMesh;
        renderer.sharedMaterials = partData.materials ?? new Material[0];
        renderer.enabled = true;
    }

    /// <summary>
    /// Updates SkinnedMeshRenderer-based parts (top, bottom, shoes).
    /// Handles null meshes by disabling the renderer.
    /// </summary>
    /// <param name="renderer">Target SkinnedMeshRenderer component</param>
    /// <param name="partData">Part data to apply</param>
    private void UpdateSkinnedMeshRendererPart(SkinnedMeshRenderer renderer, AvatarPartData partData)
    {
        if (renderer == null) return;

        // Handle null mesh (hide part)
        if (partData.avatarPartMesh == null)
        {
            renderer.sharedMesh = null;
            renderer.sharedMaterials = new Material[0];
            renderer.enabled = false;
            return;
        }

         // Apply mesh and materials
        renderer.sharedMesh = partData.avatarPartMesh;
        renderer.sharedMaterials = partData.materials ?? new Material[0];
        renderer.enabled = true;
    }

    /// <summary>
    /// Cycles to the next part in the specified array.
    /// Wraps around to the beginning when reaching the end.
    /// </summary>
    /// <param name="parts">Array of parts to cycle through</param>
    /// <param name="index">Current index reference</param>
    /// <param name="partType">Type of part being cycled</param>
    private void CyclePart(AvatarPartData[] parts, ref int index, AvatarPartType partType)
    {
        if (parts == null || parts.Length == 0) return;

        // Cycle to next index with wrap-around
        index = (index + 1) % parts.Length;
        UpdatePart(parts, ref index, partType);
    }

    /// <summary>
    /// Resets all part indices to 0.
    /// Used when switching between character models.
    /// </summary>
    public void ResetPartIndices()
    {
        currentHairIndex = 0;
        currentTopIndex = 0;
        currentBottomIndex = 0;
        currentShoesIndex = 0;
        currentAccessoryIndex = 0;
    }
}
