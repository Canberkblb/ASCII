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

public class TestScript : MonoBehaviour
{
    [SerializeField] private string apiKey = "31";
    private readonly string baseUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent";
    
    [SerializeField] private string[] ingredients = new string[]
    {
        "domates", "soğan", "patates", "havuç", "patlıcan",
        "biber", "sarımsak", "kabak", "pirinç", "makarna",
        "kıyma", "tavuk", "mantar", "bezelye", "mısır",
        "fasulye", "mercimek", "bulgur", "nohut", "ıspanak"
    };

    private async void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            System.Random rnd = new System.Random();
            int malzemeSayisi = 5;
            var secilenMalzemeler = ingredients.OrderBy(x => rnd.Next()).Take(malzemeSayisi).ToList();
            
            string malzemeListesi = string.Join(", ", secilenMalzemeler);
            
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
            ParseAndLogRecipes(response);
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

    private void ParseAndLogRecipes(string jsonResponse)
    {
        Debug.Log($"API yanıtının türü: {jsonResponse?.GetType()}");
        if (string.IsNullOrEmpty(jsonResponse))
        {
            Debug.LogError("API yanıtı boş veya null geldi.");
            return;
        }

        try
        {
            var jsonData = JsonUtility.FromJson<GeminiResponse>(jsonResponse);
            string recipeJson = jsonData.candidates[0].content.parts[0].text;
            
            recipeJson = recipeJson.Trim();
            var recipe = JsonUtility.FromJson<Recipe>(recipeJson);
            
            Debug.Log("----------------------------------------");
            Debug.Log($"\n<color=yellow>Yemek Adı: {recipe.recipe_name}</color>");
            Debug.Log($"\n<color=white>Tarif Açıklaması: {recipe.description}</color>");
            Debug.Log("\n<color=cyan>Malzemeler ve İşlemler:</color>");
            
            foreach (var ingredient in recipe.ingredients)
            {
                Debug.Log($"\n<color=green>• {ingredient.name}</color>");
                Debug.Log($"  Gerekli İşlemler: {string.Join(", ", ingredient.required_actions)}");
            }
            Debug.Log("----------------------------------------");
        }
        catch (Exception e)
        {
            Debug.LogError($"Tarif ayrıştırma hatası: {e.Message}");
        }
    }
}
