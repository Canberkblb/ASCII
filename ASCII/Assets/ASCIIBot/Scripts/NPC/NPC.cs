using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class NPC : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent agent;
    [SerializeField] private ProgressBar myBar;
    [SerializeField] private GameObject smile;
    [SerializeField] private GameObject hate;
    public bool isMyTime = false;
    private bool isProcessStarted = false;
    public float npcWaitTime = 5f;
    public float rotationSpeed = 5f;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (agent != null)
        {
            bool isMoving = agent.velocity.magnitude > 0.2f;
            SetAnimation(isMoving);

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    if (isMyTime && !isProcessStarted)
                    {
                        GameObject npcTable = GameObject.FindGameObjectWithTag("npctable");
                        Vector3 directionToTable = (npcTable.transform.position - transform.position).normalized;
                        StartCoroutine(RotateTowardsTable(directionToTable));
                        
                        myBar.gameObject.SetActive(true);
                        //myBar.StartProcess();
                        //myBar.ProcessCompleted += OnProcessCompleted;

                        checkFood();
                        isProcessStarted = true;
                    }
                }
            }
        }
    }

    IEnumerator RotateTowardsTable(Vector3 direction)
    {
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            yield return null;
        }
    }

    public void checkFood()
    {
        if (agent != null)
        {
            GameObject npcTable = GameObject.FindGameObjectWithTag("npctable");

            if (npcTable != null)
            {
                Transform holdPosition = npcTable.transform.Find("HoldPosition");

                if (holdPosition != null && holdPosition.childCount > 0)
                {
                    if (isMyTime)
                    {
                        Transform child = holdPosition.GetChild(0);

                        if (child.CompareTag("yemek"))
                        {
                            Debug.Log("NPC masasında yemek bulundu!");
                            myBar.gameObject.SetActive(false);
                            smile.SetActive(true);
                            hate.SetActive(false);
                            GameObject yemek = child.gameObject;
                            StartCoroutine(WaitAndSendToEndPoint(npcWaitTime, yemek));
                        }
                    }
                }
            }
        }
    }

    IEnumerator WaitAndSendToEndPoint(float time, GameObject yemek)
    {
        yield return new WaitForSeconds(time);
        Destroy(yemek);
        SendToEndPoint();
    }

    public void OnProcessCompleted(float value)
    {
        if (value >= 100f)
        {
            SendToEndPoint();
            smile.SetActive(false);
            hate.SetActive(true);
        }
        else if (value % 2.5f < 0.1f)
        {
            checkFood();
        }
    }

    void SetAnimation(bool isMoving)
    {
        if (animator != null)
        {
            animator.SetBool("isRunning", isMoving);
            animator.SetBool("isIdle", !isMoving);
        }
    }

    void SendToEndPoint()
    {
        if (agent != null)
        {
            isMyTime = false;
            myBar.gameObject.SetActive(false);
            NPCManager.Instance.SendNextNPCToEndPoint();
        }
    }
}
