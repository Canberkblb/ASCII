using UnityEngine;

public class PlayerInteractionTwo : MonoBehaviour
{
    [Header("Raycast Ayarları")]
    [SerializeField] private float rayDistance = 2f;
    [SerializeField] private LayerMask interactionLayer;

    [Header("Etkileşim")]
    [SerializeField] private Transform holdPosition;
    private bool isHolding = false;
    
    private Transform playerTransform;
    private RaycastHit hit;

    void Start()
    {
        playerTransform = transform;
    }

    void Update()
    {
        HandleRaycast();
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            HandleInteraction();
        }
    }

    private void HandleRaycast()
    {
        Vector3 rayOrigin = playerTransform.position + Vector3.down * 0.5f;
        Vector3 rayDirection = playerTransform.forward;
        
        Debug.DrawRay(rayOrigin, rayDirection * rayDistance, Color.blue);

        if (Physics.Raycast(rayOrigin, rayDirection, out hit, rayDistance, interactionLayer))
        {
            switch (hit.collider.tag)
            {
                case "Crate":
                    Debug.Log("Kasa tespit edildi!");
                    break;
                case "Oven":
                    Debug.Log("Fırın tespit edildi!");
                    break;
                case "Wash":
                    Debug.Log("Yıkama alanı tespit edildi!");
                    break;
            }
        }
    }

    private void HandleInteraction()
    {
        if (hit.collider != null)
        {
            GameObject hitObject = hit.collider.gameObject;
            switch (hit.collider.tag)
            {
                case "Crate":
                    InteractWithCrate(hitObject);
                    break;
                case "Oven":
                    InteractWithOven(hitObject);
                    break;
                case "Counter":
                    InteractWithCounter(hitObject);
                    break;
                case "Wash":
                    InteractWithWash(hitObject);
                    break;
            }
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

    private void InteractWithCrate(GameObject crateObject)
    {
        if (!isHolding)
        {
            Transform pickupObject = FindPickupTagInChildren(crateObject.transform);
            
            if (pickupObject != null)
            {
                GameObject copiedObject = Instantiate(pickupObject.gameObject);
                copiedObject.transform.position = holdPosition.position;
                copiedObject.transform.rotation = holdPosition.rotation;
                copiedObject.transform.SetParent(holdPosition);
                
                isHolding = true;
                Debug.Log("Nesne kopyalandı ve holdPosition'a yerleştirildi.");
            }
            else
            {
                Debug.Log("Pickup tag'li nesne bulunamadı!");
            }
        }
    }

    private void InteractWithCounter(GameObject counterObject)
    {
        Transform counterHoldPosition = counterObject.transform.Find("HoldPosition");

        if (isHolding && holdPosition.childCount > 0)
        {
            if (counterHoldPosition != null && counterHoldPosition.childCount == 0)
            {
                Transform heldItem = holdPosition.GetChild(0);
                
                heldItem.SetParent(counterHoldPosition);
                heldItem.localPosition = Vector3.zero;
                heldItem.localRotation = Quaternion.identity;
                
                isHolding = false;
                Debug.Log("Nesne counter'a bırakıldı.");
            }
            else
            {
                Debug.Log("Counter dolu, nesne bırakılamaz!");
            }
        }
        else if (!isHolding && counterHoldPosition != null && counterHoldPosition.childCount > 0)
        {
            Transform counterItem = counterHoldPosition.GetChild(0);

            counterItem.SetParent(holdPosition);
            counterItem.localPosition = Vector3.zero;
            counterItem.localRotation = Quaternion.identity;

            isHolding = true;
            Debug.Log("Nesne counter'dan alındı.");
        }
    }

    private void InteractWithOven(GameObject ovenObject)
    {
        if (isHolding && holdPosition.childCount > 0)
        {
            Transform heldItem = holdPosition.GetChild(0);
            IngredientReference ingredientRef = heldItem.GetComponent<IngredientReference>();

            if (ingredientRef != null && ingredientRef.ingredient.requiresCooking)
            {
                // Pişirme işlemi kodları buraya
                Debug.Log($"{ingredientRef.ingredient.ingredientName} pişiriliyor.");
            }
            else
            {
                Debug.Log("Bu nesne pişirilemez.");
            }
        }
    }

    private void InteractWithWash(GameObject washObject)
    {
        if (isHolding && holdPosition.childCount > 0)
        {
            Transform heldItem = holdPosition.GetChild(0);
            IngredientReference ingredientRef = heldItem.GetComponent<IngredientReference>();

            if (ingredientRef != null && ingredientRef.ingredient.requiresWashing)
            {
                // Yıkama işlemi kodları buraya
                Debug.Log($"{ingredientRef.ingredient.ingredientName} yıkanıyor.");
            }
            else
            {
                Debug.Log("Bu nesne yıkanamaz.");
            }
        }
    }
}
