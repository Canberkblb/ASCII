using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class NPCManager : MonoBehaviour
{
    public static NPCManager Instance { get; private set; }

    [Header("NPC Ayarları")]
    public GameObject npcPrefab;
    public Transform meetPoint;
    public Transform spawnPoint;
    public Transform endPoint;

    [Header("Sıra Ayarları")] 
    public GameObject linePoint;
    public int lineLength;
    public float pointSpacing = 2f;
    public float spawnRate = 25f;
    public float npcStartDelay = 0f;

    private Transform[] linePoints;
    private List<GameObject> npcs = new List<GameObject>();
    private Material sharedMaterial;
    private int totalSpawnedNPCs = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Renderer npcRenderer = npcPrefab.GetComponentInChildren<Renderer>();
        if (npcRenderer != null)
        {
            sharedMaterial = new Material(npcRenderer.sharedMaterial);
        }

        CreateLinePoints();
        InvokeRepeating("SpawnNPC", npcStartDelay, spawnRate);
    }

    void CreateLinePoints()
    {
        linePoints = new Transform[lineLength];
        
        for (int i = 0; i < lineLength; i++)
        {
            Vector3 position = linePoint.transform.position + (linePoint.transform.forward * i * pointSpacing);
            GameObject point = Instantiate(linePoint, position, linePoint.transform.rotation);
            linePoints[i] = point.transform;
        }
    }

    void SpawnNPC()
    {
        if (!LevelManager.Instance.CanSpawnNPC())
        {
            return;
        }

        totalSpawnedNPCs++;
        GameObject npc = Instantiate(npcPrefab, spawnPoint.position, Quaternion.identity);
        
        Color randomColor = new Color(
            Random.Range(0f, 1f),
            Random.Range(0f, 1f),
            Random.Range(0f, 1f)
        );

        Renderer npcRenderer = npc.GetComponentInChildren<Renderer>();
        if (npcRenderer != null && sharedMaterial != null)
        {
            Material instanceMaterial = new Material(sharedMaterial);
            instanceMaterial.color = randomColor;
            npcRenderer.material = instanceMaterial;
        }

        npcs.Add(npc);
        ArrangeNPCsInLine();
    }

    public void ArrangeNPCsInLine()
    {
        for (int i = 0; i < npcs.Count && i < linePoints.Length; i++)
        {
            GameObject npc = npcs[i];
            if (npc == null || linePoints[i] == null)
            {
                Debug.LogWarning("NPC veya linePoint null: " + i);
                continue;
            }

            NavMeshAgent agent = npc.GetComponent<NavMeshAgent>();
            NPC npcScript = npc.GetComponent<NPC>();
            
            if (agent != null)
            {
                agent.SetDestination(linePoints[i].position);
                
                if (i == 0 && npcScript != null)
                {
                    npcScript.isMyTime = true;
                }
            }
        }
    }

    public void SendNextNPCToEndPoint()
    {
        if (npcs.Count > 0)
        {
            GameObject npc = npcs[0];
            npcs.RemoveAt(0);

            NavMeshAgent agent = npc.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.SetDestination(endPoint.position);
                Destroy(npc, 5f);
                LevelManager.Instance.OnNPCCompleted();
            }

            ArrangeNPCsInLine();
        }
    }
    
    public void ResetForNewLevel()
    {
        totalSpawnedNPCs = 0;
        CancelInvoke("SpawnNPC");
        InvokeRepeating("SpawnNPC", npcStartDelay, spawnRate);
    }

    public void ResetNPCs()
    {
        totalSpawnedNPCs = 0;
        foreach (GameObject npc in npcs)
        {
            Destroy(npc);
        }
        npcs.Clear();
        InvokeRepeating("SpawnNPC", npcStartDelay, spawnRate);
    }
}
