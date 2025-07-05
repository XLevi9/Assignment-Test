using UnityEngine;

/// <summary>
/// Manages switching between male and female character models in a character customization system.
/// Integrates with the Animator, CharacterCustomizationManager, and CharacterSpawner.
/// 
/// Features:
/// - Toggles between male and female character models
/// - Updates Animator avatars for gender-specific animations
/// - Synchronizes with CharacterCustomizationManager for part updates
/// - Notifies CharacterSpawner of gender switches
/// - Maintains state tracking for current gender
/// </summary>
public class GenderSwitcher : MonoBehaviour
{
    [Header("Models")]
    [SerializeField] private GameObject maleModel;
    [SerializeField] private GameObject femaleModel;

    [Header("Avatars")]
    [SerializeField] private Avatar maleAvatar;
    [SerializeField] private Avatar femaleAvatar;

    private Animator characterAnimator;
    [Header("Customization")]
    public CharacterCustomizationManager customizationManager;
    [Header("Spawner Reference")]
    [SerializeField] private CharacterSpawner characterSpawner;
    private bool isMale = true;

    /// <summary>
    /// Initializes the gender switcher, sets up the animator, and ensures initial male model state.
    /// </summary>
    void Awake()
    {
        characterAnimator = GetComponent<Animator>();

        // Ensure initial state
        if (maleModel != null) maleModel.SetActive(true);
        if (femaleModel != null) femaleModel.SetActive(false);

        if (characterAnimator != null && maleAvatar != null)
        {
            characterAnimator.avatar = maleAvatar;
        }

        customizationManager.SetActiveSlots(true);
    }

    void Start()
    {
        ShowMale();
    }

    /// <summary>
    /// Switches to the male character model and updates related systems.
    /// </summary>
    public void ShowMale()
    {
        if (isMale) return; // Already showing male

        Debug.Log("Switching to Male");

        // Switch avatar for animator
        if (characterAnimator != null && maleAvatar != null)
        {
            characterAnimator.avatar = maleAvatar;
        }

        // Switch models
        if (maleModel != null) maleModel.SetActive(true);
        if (femaleModel != null) femaleModel.SetActive(false);

        isMale = true;

        // Update customization system for new model
        if (customizationManager != null)
        {
            customizationManager.SetActiveSlots(true);
            customizationManager.ResetPartIndices();
            customizationManager.InitializeCustomization();
        }

        if (characterSpawner != null)
        {
            characterSpawner.OnCharacterSwitched();
        }
    }

    /// <summary>
    /// Switches to the female character model and updates related systems.
    /// </summary>
    public void ShowFemale()
    {
        if (!isMale) return; // Already showing female

        Debug.Log("Switching to Female");

        // Switch avatar for animator
        if (characterAnimator != null && femaleAvatar != null)
        {
            characterAnimator.avatar = femaleAvatar;
        }

        // Switch models
        if (maleModel != null) maleModel.SetActive(false);
        if (femaleModel != null) femaleModel.SetActive(true);

        isMale = false;

        // Update customization system for new model
        if (customizationManager != null)
        {
            customizationManager.SetActiveSlots(false);
            customizationManager.ResetPartIndices();
            customizationManager.InitializeCustomization();
        }

        if (characterSpawner != null)
        {
            characterSpawner.OnCharacterSwitched();
        }
    }

    /// <summary>
    /// Returns the current gender state.
    /// </summary>
    /// <returns>True if the male model is active, false if the female model is active.</returns>
    public bool IsMale()
    {
        return isMale;
    }
}