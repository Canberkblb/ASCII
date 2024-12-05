using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class NPCManager : MonoBehaviour
{
    public static NPCManager Instance { get; private set; } // Singleton instance

    public GameObject npcPrefab;
    public Transform meetPoint;
    public Transform spawnPoint;
    public Transform endPoint;
    public GameObject linePoint;
    public int lineLength;
    public float pointSpacing = 2f;

    private Transform[] linePoints;
    private List<GameObject> npcs = new List<GameObject>();
    private Material sharedMaterial;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Oyun sahnesi değişse bile bu nesneyi yok etme
        }
        else
        {
            Destroy(gameObject); // Zaten bir instance varsa, bu nesneyi yok et
        }
    }

    void Start()
    {
        // NPC'lerin paylaşacağı materyali al
        Renderer npcRenderer = npcPrefab.GetComponentInChildren<Renderer>();
        if (npcRenderer != null)
        {
            sharedMaterial = new Material(npcRenderer.sharedMaterial);
        }

        CreateLinePoints();
        InvokeRepeating("SpawnNPC", 0f, 5f);
        ///InvokeRepeating("SendNextNPCToEndPoint", 7f, 25f);
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
        if (npcs.Count >= lineLength)
        {
            return;
        }

        GameObject npc = Instantiate(npcPrefab, spawnPoint.position, Quaternion.identity);
        
        // Random renk oluştur
        Color randomColor = new Color(
            Random.Range(0f, 1f),
            Random.Range(0f, 1f),
            Random.Range(0f, 1f)
        );

        // NPC'nin renderer komponentini bul ve materyali uygula
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
            NavMeshAgent agent = npc.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.SetDestination(linePoints[i].position);
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
            }

            ArrangeNPCsInLine();
        }
    }

    void Update()
    {
        // NPC animasyon kontrolü artık NPC sınıfında
    }

    void OnDrawGizmos()
    {
        if (linePoints != null)
        {
            Gizmos.color = Color.red; // Gizmo rengi
            foreach (Transform point in linePoints)
            {
                if (point != null)
                {
                    Gizmos.DrawSphere(point.position, 0.2f); // Küçük bir küre çiz
                }
            }
        }
    }
}
