using UnityEngine;

[CreateAssetMenu(fileName = "NewIngredient", menuName = "CookingGame/Ingredient")]
public class Ingredient : ScriptableObject
{
    public string ingredientName;
    public GameObject ingredientCratePrefab;
    public GameObject ingredientPrefab;

    [Tooltip("Bu malzeme kesilmelidir.")]
    public bool requiresCutting;
    [Tooltip("Bu malzeme yıkanmalıdır.")]
    public bool requiresWashing;
    [Tooltip("Bu malzeme pişirilmelidir.")]
    public bool requiresCooking;
    [Tooltip("Bu malzeme fırınlanmalıdır.")]
    public bool requiresBaking;
}