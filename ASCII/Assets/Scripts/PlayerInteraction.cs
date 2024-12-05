using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerInteraction : MonoBehaviour
{
    public float interactionRadius = 2f;
    public Transform holdPosition;
    private GameObject nearestCrate;
    private GameObject detectionObject;
    private bool isHolding;

    void Start()
    {
        detectionObject = new GameObject("DetectionZone");
        detectionObject.transform.parent = transform;
        detectionObject.transform.localPosition = Vector3.zero;
        
        SphereCollider sphereCollider = detectionObject.AddComponent<SphereCollider>();
        sphereCollider.isTrigger = true;
        sphereCollider.radius = interactionRadius;

        DetectionZone detectionZone = detectionObject.AddComponent<DetectionZone>();
        detectionZone.controller = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && nearestCrate != null && !isHolding)
        {
            InteractWithCrate();
        }
        
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            if (!IsPointerOverUI() && nearestCrate != null && !isHolding)
            {
                InteractWithCrate();
            }
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            if (!IsPointerOverUI() && nearestCrate != null && !isHolding)
            {
                InteractWithCrate();
            }
        }
    }

    private bool IsPointerOverUI()
    {
        if (Input.touchCount > 0)
        {
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        }
        
        return EventSystem.current.IsPointerOverGameObject();
    }

    private void InteractWithCrate()
    {
        Transform pickupObject = FindPickupTagInChildren(nearestCrate.transform);
            
        if (pickupObject != null)
        {
            GameObject pickedItem = Instantiate(pickupObject.gameObject, holdPosition.position, Quaternion.identity);
            pickedItem.transform.SetParent(holdPosition);
            pickupObject.gameObject.SetActive(false);
            isHolding = true;
        }
    }

    private Transform FindPickupTagInChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag("Pickup"))
            {
                return child;
            }
        }
        return null;
    }

    public void SetNearestCrate(GameObject crate)
    {
        nearestCrate = crate;
    }

    public void ClearNearestCrate(GameObject crate)
    {
        if (nearestCrate == crate)
        {
            nearestCrate = null;
        }
    }
}

public class DetectionZone : MonoBehaviour
{
    public PlayerInteraction controller;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Crate"))
        {
            controller.SetNearestCrate(other.gameObject);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Crate"))
        {
            controller.ClearNearestCrate(other.gameObject);
        }
    }
}