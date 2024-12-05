using UnityEngine;
using UnityEngine.AI;

public class NPC : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent agent;

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
                    //Debug.Log("Hedefe ulaşıldı.");
                }
            }
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
            agent.SetDestination(NPCManager.Instance.endPoint.position); // NPCManager'dan endPoint al
            Destroy(gameObject, 5f);
        }
    }
}
