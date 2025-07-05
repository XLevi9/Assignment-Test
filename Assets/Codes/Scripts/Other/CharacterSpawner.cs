using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Manages the spawning and management of character prefabs in a Unity scene.
/// Supports both manual and automatic spawning with configurable settings and UI integration.
/// 
/// Features:
/// - Spawns male and female character prefabs at designated spawn points
/// - Automatic and manual spawning with configurable limits and intervals
/// - Prevents spawn point overlap by checking proximity to existing characters
/// - UI updates for spawn count and button interactability
/// - Visual debugging with Gizmos for spawn points
/// - Movement component management for display and spawned characters
/// </summary>
public class CharacterSpawner : MonoBehaviour
{
    [Header("Character References")]
    [SerializeField] private GameObject malePrefab;
    [SerializeField] private GameObject femalePrefab;

    [Header("Spawn Settings")]
    [SerializeField] private Transform[] spawnPoints;

    // Maximum number of characters to spawn
    [SerializeField] private int maxCharacters = 5;
    [SerializeField] private float spawnInterval = 2f;

    [Header("Auto Spawn")]
    [SerializeField] private bool autoSpawn = true;
    [SerializeField] private float autoSpawnDelay = 1f;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI spawnCountText;
    [SerializeField] private Button spawnButton;

    private List<GameObject> spawnedCharacters = new List<GameObject>();
    private float spawnTimer;

    /// <summary>
    /// Initializes the spawner, sets up prefabs, and configures auto-spawning.
    /// </summary>
    void Start()
    {
        // Get child references if not assigned
        if (malePrefab == null || femalePrefab == null)
        {
            GetChildReferences();
        }

        // Disable movement components on display prefabs
        DisableMovementOnDisplayPrefabs();

        if (autoSpawn)
        {
            spawnTimer = autoSpawnDelay;
        }

        // Update UI
        UpdateSpawnCountUI();
    }

    /// <summary>
    /// Handles automatic spawning based on timer and character limit.
    /// </summary>
    void Update()
    {
        if (autoSpawn && spawnedCharacters.Count < maxCharacters)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0)
            {
                SpawnCharacter();
                spawnTimer = spawnInterval;
            }
        }
    }

    /// <summary>
    /// Retrieves male and female prefab references from child objects if not assigned.
    /// </summary>
    void GetChildReferences()
    {
        // Find male and female child objects
        foreach (Transform child in transform)
        {
            if (child.name.ToLower().Contains("male"))
            {
                if (child.name.ToLower().Contains("female"))
                {
                    femalePrefab = child.gameObject;
                }
                else
                {
                    malePrefab = child.gameObject;
                }
            }
            else if (child.name.ToLower().Contains("female"))
            {
                femalePrefab = child.gameObject;
            }
        }
    }

    /// <summary>
    /// Disables movement-related components on display prefabs to prevent unwanted behavior.
    /// </summary>
    void DisableMovementOnDisplayPrefabs()
    {
        if (malePrefab != null)
        {
            DisableMovementComponents(malePrefab);
        }

        if (femalePrefab != null)
        {
            DisableMovementComponents(femalePrefab);
        }
    }

    /// <summary>
    /// Disables movement components (CharacterSpawnController and Animator) on a prefab.
    /// </summary>
    /// <param name="prefab">The prefab to modify.</param>
    void DisableMovementComponents(GameObject prefab)
    {
        // Disable CharacterSpawnController
        CharacterSpawnController controller = prefab.GetComponent<CharacterSpawnController>();
        if (controller != null)
        {
            controller.enabled = false;
        }

        // Disable Animator (opsional - jika tidak ingin animasi sama sekali)
        Animator animator = prefab.GetComponent<Animator>();
        if (animator != null)
        {
            animator.enabled = false;
        }
    }

    /// <summary>
    /// Spawns a character at a random, valid spawn point.
    /// </summary>
    public void SpawnCharacter()
    {
        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning("No spawn points assigned!");
            return;
        }

        // Determine which character to spawn based on active child
        GameObject prefabToSpawn = GetActivePrefab();

        if (prefabToSpawn == null)
        {
            Debug.LogWarning("No active character prefab found!");
            return;
        }

        // Get random spawn point
        Transform spawnPoint = GetRandomSpawnPoint();

        // Spawn character
        GameObject spawnedCharacter = Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);

        // Enable the character and its components
        spawnedCharacter.SetActive(true);

        // Enable movement components untuk spawned character
        EnableMovementComponents(spawnedCharacter);

        // Add to spawned list
        spawnedCharacters.Add(spawnedCharacter);

        // Update UI
        UpdateSpawnCountUI();

        Debug.Log($"Spawned {prefabToSpawn.name} at {spawnPoint.name}");
    }

    /// <summary>
    /// Enables movement components (CharacterSpawnController and Animator) on a spawned character.
    /// </summary>
    /// <param name="spawnedCharacter">The spawned character to modify.</param>
    void EnableMovementComponents(GameObject spawnedCharacter)
    {
        // Enable CharacterSpawnController
        CharacterSpawnController controller = spawnedCharacter.GetComponent<CharacterSpawnController>();
        if (controller != null)
        {
            controller.enabled = true;
        }

        // Enable Animator
        Animator animator = spawnedCharacter.GetComponent<Animator>();
        if (animator != null)
        {
            animator.enabled = true;
        }
    }

    /// <summary>
    /// Determines which prefab (male or female) is currently active for spawning.
    /// </summary>
    /// <returns>The active prefab, or null if none is active.</returns>
    GameObject GetActivePrefab()
    {
        // Check which child is active
        if (malePrefab != null && malePrefab.activeInHierarchy)
        {
            return malePrefab;
        }
        else if (femalePrefab != null && femalePrefab.activeInHierarchy)
        {
            return femalePrefab;
        }

        return null;
    }

    /// <summary>
    /// Selects a random spawn point, avoiding proximity to existing characters.
    /// </summary>
    /// <returns>A suitable spawn point transform.</returns>
    Transform GetRandomSpawnPoint()
    {
        // Get a random spawn point that's not too close to existing characters
        List<Transform> availableSpawnPoints = new List<Transform>(spawnPoints);

        // Remove spawn points that are too close to existing characters
        for (int i = availableSpawnPoints.Count - 1; i >= 0; i--)
        {
            foreach (GameObject character in spawnedCharacters)
            {
                if (character != null)
                {
                    float distance = Vector3.Distance(availableSpawnPoints[i].position, character.transform.position);
                    if (distance < 3f) // Minimum distance between characters
                    {
                        availableSpawnPoints.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        // If no available spawn points, use any spawn point
        if (availableSpawnPoints.Count == 0)
        {
            availableSpawnPoints = new List<Transform>(spawnPoints);
        }

        return availableSpawnPoints[UnityEngine.Random.Range(0, availableSpawnPoints.Count)];
    }

    /// <summary>
    /// Destroys all spawned characters and clears the list.
    /// </summary>
    public void ClearAllCharacters()
    {
        foreach (GameObject character in spawnedCharacters)
        {
            if (character != null)
            {
                DestroyImmediate(character);
            }
        }
        spawnedCharacters.Clear();

        // Update UI
        UpdateSpawnCountUI();
    }

    /// <summary>
    /// Handles spawn button click, spawning a character if within limits.
    /// </summary>
    public void OnSpawnButtonClicked()
    {
        if (spawnedCharacters.Count < maxCharacters)
        {
            SpawnCharacter();
        }
        else
        {
            Debug.Log("Maximum characters reached!");
        }
    }

    /// <summary>
    /// Updates the UI to reflect the current spawn count and button state.
    /// </summary>
    void UpdateSpawnCountUI()
    {
        if (spawnCountText != null)
        {
            spawnCountText.text = $"Characters: {spawnedCharacters.Count}/{maxCharacters}";
        }

        // Update button interactability
        if (spawnButton != null)
        {
            spawnButton.interactable = spawnedCharacters.Count < maxCharacters;
        }
    }

    /// <summary>
    /// Handles clear button click to remove all spawned characters.
    /// </summary>
    public void OnClearButtonClicked()
    {
        ClearAllCharacters();
    }

    /// <summary>
    /// Re-disables movement components on display prefabs when switching characters.
    /// </summary>
    public void OnCharacterSwitched()
    {
        DisableMovementOnDisplayPrefabs();
    }

    /// <summary>
    /// Draws Gizmos in the Scene view for visualizing spawn points.
    /// </summary>
    void OnDrawGizmos()
    {
        if (spawnPoints != null)
        {
            Gizmos.color = Color.green;
            foreach (Transform spawnPoint in spawnPoints)
            {
                if (spawnPoint != null)
                {
                    Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
                    Gizmos.DrawRay(spawnPoint.position, spawnPoint.forward * 2f);
                }
            }
        }
    }
}