using UnityEngine;

/// <summary>
/// ScriptableObject that defines avatar part data for the customization system.
/// This is the core data structure that holds mesh, materials, and optimization settings.
/// 
/// Design Pattern: Data-Driven Architecture
/// </summary>
[CreateAssetMenu(fileName = "New Avatar Part", menuName = "Avatar/Avatar Part")]
public class AvatarPartData : ScriptableObject
{
    [Header("Part Info")]
    public string avatarPartName;
    public AvatarPartType avatarPartType;
    public Mesh avatarPartMesh;

    [Header("Materials")]
    public Material[] materials;

    [Header("Optimization")]
    public bool useGPUInstancing = true;
}

/// <summary>
/// Enum defining the available avatar part categories.
/// Extensible design allows easy addition of new part types.
/// </summary>
public enum AvatarPartType
{
    Hair,
    Top,
    Bottom,
    Shoes,
    Accessory
}