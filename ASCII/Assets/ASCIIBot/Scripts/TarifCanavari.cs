using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System;

[Serializable]
public class GeminiResponse
{
    public Candidate[] candidates;
}

[Serializable]
public class Candidate
{
    public Content content;
}

[Serializable]
public class Content
{
    public Part[] parts;
}

[Serializable]
public class Part
{
    public string text;
}

[Serializable]
public class Recipe
{
    public string recipe_name;
    public RecipeIngredient[] ingredients;
    public string description;
}

[Serializable]
public class RecipeIngredient
{
    public string name;
    public string[] required_actions;
}

[Serializable]
public class RecipeList
{
    public Recipe recipe;
}

[System.Serializable]
public class Menu
{
    public string recipeName;
    public string description;
    public List<MenuIngredient> ingredients;

    public Menu(string recipeName, string description)
    {
        this.recipeName = recipeName;
        this.description = description;
        this.ingredients = new List<MenuIngredient>();
    }
}

[System.Serializable]
public class MenuIngredient
{
    public string name;
    public string[] requiredActions;
    public GameObject cratePrefab;
    public GameObject prefab;
    public GameObject spawnedObject;

    public MenuIngredient(string name, string[] requiredActions, GameObject cratePrefab, GameObject prefab)
    {
        this.name = name;
        this.requiredActions = requiredActions;
        this.cratePrefab = cratePrefab;
        this.prefab = prefab;
    }
}

public class IngredientReference : MonoBehaviour
{
    public Ingredient ingredient;
    public string[] requiredActionsOrder;
    private int currentActionIndex = 0;
    
    public bool isWashed = false;
    public bool isCut = false;
    public bool isCooked = false;
    public bool isBaked = false;

    public string GetNextRequiredAction()
    {
        if (requiredActionsOrder != null && currentActionIndex < requiredActionsOrder.Length)
        {
            return requiredActionsOrder[currentActionIndex];
        }
        return null;
    }

    public bool CompleteAction(string action)
    {
        if (GetNextRequiredAction() != action)
        {
            return false;
        }

        switch (action)
        {
            case "wash": isWashed = true; break;
            case "cut": isCut = true; break;
            case "cook": isCooked = true; break;
            case "bake": isBaked = true; break;
        }

        currentActionIndex++;
        return true;
    }

    public bool IsCompleted()
    {
        return currentActionIndex >= requiredActionsOrder.Length;
    }
}

public class TarifCanavari : MonoBehaviour
{
    private static TarifCanavari instance;
    public static TarifCanavari Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<TarifCanavari>();
                if (instance == null)
                {
                    GameObject go = new GameObject("TarifCanavari");
                    instance = go.AddComponent<TarifCanavari>();
                }
                DontDestroyOnLoad(instance.gameObject);
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    [SerializeField] private Ingredient[] ingredients;
    [SerializeField] private Transform[] spawnPoints;
    private readonly string baseUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent";

    [SerializeField] private string apiKey = "31";
    [SerializeField] private bool isDebug = false;

    public Menu currentMenu;

    private async void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            System.Random rnd = new System.Random();
            int malzemeSayisi = 4;
            var secilenMalzemeler = ingredients.OrderBy(x => rnd.Next()).Take(malzemeSayisi).ToList();

            string malzemeListesi = string.Join(", ", secilenMalzemeler.Select(i => i.ingredientName));

            string systemPrompt = "JSON formatında yanıt ver. Verilen malzemelerle yapılabilecek bir yemek tarifi oluştur.";
            string userPrompt = $@"Şu malzemelerden bir yemek tarifi oluştur: {malzemeListesi}. 
                Her malzeme için gerekli işlemleri belirt (cut:kesmek, wash:yıkamak, cook:pişirmek, bake:fırınlamak).
                Şu JSON formatını kullan: {{
                    'recipe_name': 'tarif adı',
                    'description': 'kısa tarif açıklaması',
                    'ingredients': [
                        {{'name': 'malzeme1', 'required_actions': ['işlem1', 'işlem2']}},
                        {{'name': 'malzeme2', 'required_actions': ['işlem1', 'işlem2']}}
                    ]
                }}";

            string response = await CallGeminiAPI(userPrompt, systemPrompt);
            Menu olusturulanMenu = ParseAndLogRecipes(response);

            if (olusturulanMenu != null && isDebug)
            {
                Debug.Log($"Oluşturulan menü: {olusturulanMenu.recipeName}");
            }
        }
    }

    private async Task<string> CallGeminiAPI(string userPrompt, string systemPrompt)
    {
        using (var client = new UnityWebRequest($"{baseUrl}?key={apiKey}", "POST"))
        {
            userPrompt = userPrompt.Replace("\"", "'").Replace("\n", "\\n");
            systemPrompt = systemPrompt.Replace("\"", "'").Replace("\n", "\\n");

            string jsonBody = $@"{{
                ""contents"": [
                    {{
                        ""parts"": [
                            {{
                                ""text"": ""{systemPrompt}\\n{userPrompt}""
                            }}
                        ]
                    }}
                ],
                ""generationConfig"": {{
                    ""response_mime_type"": ""application/json""
                }}
            }}";

            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

            client.uploadHandler = new UploadHandlerRaw(bodyRaw);
            client.downloadHandler = new DownloadHandlerBuffer();
            client.SetRequestHeader("Content-Type", "application/json");

            await client.SendWebRequest();

            if (client.result == UnityWebRequest.Result.Success)
            {
                return client.downloadHandler.text;
            }
            else
            {
                Debug.LogError($"API Hatası: {client.error}\nYanıt: {client.downloadHandler?.text}");
                return null;
            }
        }
    }

    private Menu ParseAndLogRecipes(string jsonResponse)
    {
        try
        {
            var jsonData = JsonUtility.FromJson<GeminiResponse>(jsonResponse);
            string recipeJson = jsonData.candidates[0].content.parts[0].text;

            recipeJson = recipeJson.Trim();
            var recipe = JsonUtility.FromJson<Recipe>(recipeJson);

            currentMenu = new Menu(recipe.recipe_name, recipe.description);

            if (isDebug)
            {
                Debug.Log("----------------------------------------");
                Debug.Log($"\n<color=yellow>Yemek Adı: {currentMenu.recipeName}</color>");
                Debug.Log($"\n<color=white>Tarif Açıklaması: {currentMenu.description}</color>");
                Debug.Log("\n<color=cyan>Malzemeler ve İşlemler:</color>");
            }

            int spawnIndex = 0;

            foreach (var recipeIngredient in recipe.ingredients)
            {
                if (isDebug)
                {
                    Debug.Log($"\n<color=green>• {recipeIngredient.name}</color>");
                    Debug.Log($"  Gerekli İşlemler: {string.Join(", ", recipeIngredient.required_actions)}");
                }

                Ingredient matchingIngredient = ingredients.FirstOrDefault(i =>
                    i.ingredientName.ToLower() == recipeIngredient.name.ToLower());

                if (matchingIngredient != null && matchingIngredient.ingredientCratePrefab != null)
                {
                    Transform spawnPoint = spawnPoints[spawnIndex % spawnPoints.Length];
                    spawnIndex++;

                    GameObject spawnedIngredient = Instantiate(
                        matchingIngredient.ingredientCratePrefab,
                        spawnPoint.position,
                        spawnPoint.rotation
                    );
                    
                    foreach(Transform child in spawnedIngredient.transform) {
                        if(child.CompareTag("Pickup")) {
                            var ingredientRef = child.gameObject.AddComponent<IngredientReference>();
                            ingredientRef.ingredient = matchingIngredient;
                            ingredientRef.requiredActionsOrder = matchingIngredient.requiredActionsOrder;
                        }
                    }

                    spawnedIngredient.name = recipeIngredient.name;

                    var menuIngredient = new MenuIngredient(
                        recipeIngredient.name,
                        recipeIngredient.required_actions,
                        matchingIngredient.ingredientCratePrefab,
                        matchingIngredient.ingredientPrefab
                    );
                    menuIngredient.spawnedObject = spawnedIngredient;
                    currentMenu.ingredients.Add(menuIngredient);
                }
                else
                {
                    if (isDebug)
                    {
                        Debug.LogWarning($"'{recipeIngredient.name}' için prefab bulunamadı!");
                    }
                }
            }
            if (isDebug)
            {
                Debug.Log("----------------------------------------");
            }

            return currentMenu;
        }
        catch (Exception e)
        {
            Debug.LogError($"Tarif ayrıştırma hatası: {e.Message}");
            return null;
        }
    }
}
