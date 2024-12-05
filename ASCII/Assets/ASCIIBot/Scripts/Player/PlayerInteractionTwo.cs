using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInteractionTwo : MonoBehaviour
{
    [Header("Raycast Ayarları")]
    [SerializeField] private float rayDistance = 2f;
    [SerializeField] private LayerMask interactionLayer;

    [Header("Etkileşim")]
    [SerializeField] private Transform holdPosition;
    private bool isHolding = false;
    
    [Header("ProgressBar")]
    [SerializeField] private ProgressBar progressBarPrefab;
    [SerializeField] private GameObject canvasPrefab;
    [SerializeField] private float progressBarScale = 0.5f;
    private ProgressBar activeProgressBar;
    private GameObject activeProgressCanvas;
    private Camera mainCamera;
    
    [Header("FoodRelated")]
    [SerializeField] private List<GameObject> CookedRecipes;
    private GameObject currentStove;
    private bool isWashing = false;
    private bool isCutting = false;
    private bool isCooking = false;
    
    private Transform playerTransform;
    private RaycastHit hit;

    void Start()
    {
        playerTransform = transform;
        mainCamera = Camera.main;
    }

    void Update()
    {
        HandleRaycast();
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            HandleInteraction();
        }

        if(Input.GetKeyDown(KeyCode.Q))
        {
            NPCManager.Instance.SendNextNPCToEndPoint();
        }
        
        if (activeProgressCanvas != null)
        {
            activeProgressCanvas.transform.LookAt(mainCamera.transform);
            activeProgressCanvas.transform.Rotate(0, 180, 0);
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
                case "Stove":
                    Debug.Log("Ocak tespit edildi!");
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
                case "Stove":
                    InteractWithStove(hitObject);
                    break;
                case "Counter":
                    InteractWithCounter(hitObject);
                    break;
                case "Wash":
                    InteractWithWash(hitObject);
                    break;
                case "CuttingBoard":
                    InteractWithCuttingBoard(hitObject);
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

    private void InteractWithStove(GameObject stoveObject)
    {
        Transform stoveHoldPosition = stoveObject.transform.Find("HoldPosition");
        currentStove = stoveObject;

        if (isHolding && holdPosition.childCount > 0)
        {
            if (stoveHoldPosition != null && stoveHoldPosition.childCount <= 5)
            {
                Transform heldItem = holdPosition.GetChild(0);
                IngredientReference ingredientRef = heldItem.GetComponent<IngredientReference>();
                
                if (ingredientRef != null)
                {
                    string nextAction = ingredientRef.GetNextRequiredAction();
                    if (nextAction == "cook")
                    {
                        heldItem.SetParent(stoveHoldPosition);
                        heldItem.localPosition = Vector3.zero;
                        heldItem.localRotation = Quaternion.identity;
                
                        isHolding = false;
                        if (CheckAllIngredientsOnStove(stoveObject))
                        {
                            StartCooking(stoveObject, ingredientRef);
                        }
                    }
                    else
                    {
                        Debug.Log("Bu malzeme şu anda pişirilemez! Önce " + nextAction + " işlemi yapılmalı.");
                    }
                }
            }
            else
            {
                Debug.Log("Counter dolu, nesne bırakılamaz!");
            }
        }
        else if (!isHolding && stoveHoldPosition != null && stoveHoldPosition.childCount > 0 && !isCooking)
        {
            Transform counterItem = stoveHoldPosition.GetChild(0);

            counterItem.SetParent(holdPosition);
            counterItem.localPosition = Vector3.zero;
            counterItem.localRotation = Quaternion.identity;

            isHolding = true;
            Debug.Log("Nesne counter'dan alındı.");
        }
    }

    private void StartCooking(GameObject stoveObject, IngredientReference ingredient)
    {
        if (activeProgressBar == null)
        {
            isCooking = true;
            activeProgressCanvas = Instantiate(canvasPrefab, stoveObject.transform);
            activeProgressCanvas.transform.localPosition = new Vector3(0, 3f, 0);
            activeProgressCanvas.transform.localScale = Vector3.one * progressBarScale;
            
            activeProgressBar = Instantiate(progressBarPrefab, activeProgressCanvas.transform);
            activeProgressBar.transform.localPosition = Vector3.zero;
            
            var progressBar = activeProgressBar.GetComponent<ProgressBar>();
            progressBar.ProcessCompleted += OnCookingComplete;
            progressBar.StartProcess();
        }
    }

    private void OnCookingComplete()
    {
        Debug.Log("Pişirme tamamlandı!");
        isCooking = false;
        
        if (currentStove != null)
        {
            Transform holdPos = currentStove.transform.Find("HoldPosition");
            if (holdPos != null)
            {
                foreach (Transform child in holdPos)
                {
                    IngredientReference ingredientRef = child.GetComponent<IngredientReference>();
                    if (ingredientRef != null)
                    {
                        ingredientRef.CompleteAction("cook");
                    }
                }
            }
        }
        
        if (activeProgressCanvas != null)
        {
            Destroy(activeProgressCanvas);
            activeProgressCanvas = null;
            activeProgressBar = null;
        }
        
        if (CookedRecipes != null && CookedRecipes.Count > 0)
        {
            int randomIndex = Random.Range(0, CookedRecipes.Count);
            Transform spawnPosition = currentStove.transform.Find("SpawnPosition");
            if (spawnPosition != null)
            {
                Instantiate(CookedRecipes[randomIndex], spawnPosition.position, Quaternion.identity);
            }
        }
    }

    private void InteractWithWash(GameObject washObject)
    {
        Transform washHoldPosition = washObject.transform.Find("HoldPosition");

        if (isHolding && holdPosition.childCount > 0)
        {
            if (washHoldPosition != null && washHoldPosition.childCount == 0)
            {
                Transform heldItem = holdPosition.GetChild(0);
                IngredientReference ingredientRef = heldItem.GetComponent<IngredientReference>();
                
                if (ingredientRef != null)
                {
                    string nextAction = ingredientRef.GetNextRequiredAction();
                    if (nextAction == "wash")
                    {
                        heldItem.SetParent(washHoldPosition);
                        heldItem.localPosition = Vector3.zero;
                        heldItem.localRotation = Quaternion.identity;
                
                        isHolding = false;
                        StartWashing(washObject, ingredientRef);
                    }
                    else
                    {
                        Debug.Log("Bu malzeme şu anda yıkanamaz! Önce " + nextAction + " işlemi yapılmalı.");
                    }
                }
            }
            else
            {
                Debug.Log("Wash dolu, nesne bırakılamaz!");
            }
        }
        else if (!isHolding && washHoldPosition != null && washHoldPosition.childCount > 0 && !isWashing)
        {
            Transform counterItem = washHoldPosition.GetChild(0);

            counterItem.SetParent(holdPosition);
            counterItem.localPosition = Vector3.zero;
            counterItem.localRotation = Quaternion.identity;

            isHolding = true;
            Debug.Log("Nesne wash'dan alındı.");
        }
    }

    private void StartWashing(GameObject washObject, IngredientReference ingredient)
    {
        if (activeProgressBar == null)
        {
            isWashing = true;
            
            activeProgressCanvas = Instantiate(canvasPrefab, washObject.transform);
            activeProgressCanvas.transform.localPosition = new Vector3(0, 2f, 0);
            activeProgressCanvas.transform.localScale = Vector3.one * progressBarScale;
            
            activeProgressBar = Instantiate(progressBarPrefab, activeProgressCanvas.transform);
            activeProgressBar.transform.localPosition = Vector3.zero;
            
            var progressBar = activeProgressBar.GetComponent<ProgressBar>();
            progressBar.ProcessCompleted += OnWashingComplete;
            progressBar.StartProcess();
        }
    }

    private void OnWashingComplete()
    {
        Debug.Log("Yıkama tamamlandı!");
        isWashing = false;
        
        Transform washObject = activeProgressCanvas.transform.parent;
        if (washObject != null)
        {
            Transform holdPos = washObject.Find("HoldPosition");
            if (holdPos != null && holdPos.childCount > 0)
            {
                IngredientReference ingredientRef = holdPos.GetChild(0).GetComponent<IngredientReference>();
                if (ingredientRef != null)
                {
                    ingredientRef.CompleteAction("wash");
                }
            }
        }
        
        if (activeProgressCanvas != null)
        {
            Destroy(activeProgressCanvas);
            activeProgressCanvas = null;
            activeProgressBar = null;
        }
    }

    private void InteractWithCuttingBoard(GameObject cuttingBoardObject)
    {
        Transform cutHoldPosition = cuttingBoardObject.transform.Find("HoldPosition");

        if (isHolding && holdPosition.childCount > 0)
        {
            if (cutHoldPosition != null && cutHoldPosition.childCount == 0)
            {
                Transform heldItem = holdPosition.GetChild(0);
                IngredientReference ingredientRef = heldItem.GetComponent<IngredientReference>();
                
                if (ingredientRef != null)
                {
                    string nextAction = ingredientRef.GetNextRequiredAction();
                    if (nextAction == "cut")
                    {
                        heldItem.SetParent(cutHoldPosition);
                        heldItem.localPosition = Vector3.zero;
                        heldItem.localRotation = Quaternion.identity;
                
                        isHolding = false;
                        StartCutting(cuttingBoardObject, ingredientRef);
                    }
                    else
                    {
                        Debug.Log("Bu malzeme şu anda kesilemez! Önce " + nextAction + " işlemi yapılmalı.");
                    }
                }
            }
            else
            {
                Debug.Log("Wash dolu, nesne bırakılamaz!");
            }
        }
        else if (!isHolding && cutHoldPosition != null && cutHoldPosition.childCount > 0 && !isCutting)
        {
            Transform counterItem = cutHoldPosition.GetChild(0);

            counterItem.SetParent(holdPosition);
            counterItem.localPosition = Vector3.zero;
            counterItem.localRotation = Quaternion.identity;

            isHolding = true;
            Debug.Log("Nesne wash'dan alındı.");
        }
    }
    
    private void StartCutting(GameObject cuttingBoardObject, IngredientReference ingredient)
    {
        if (activeProgressBar == null)
        {
            isCutting = true;
            activeProgressCanvas = Instantiate(canvasPrefab, cuttingBoardObject.transform);
            activeProgressCanvas.transform.localPosition = new Vector3(0, 2f, 0);
            activeProgressCanvas.transform.localScale = Vector3.one * progressBarScale;
            
            activeProgressBar = Instantiate(progressBarPrefab, activeProgressCanvas.transform);
            activeProgressBar.transform.localPosition = Vector3.zero;
            
            var progressBar = activeProgressBar.GetComponent<ProgressBar>();
            progressBar.ProcessCompleted += OnCuttingComplete;
            progressBar.StartProcess();
        }
    }
    
    private void OnCuttingComplete()
    {
        Debug.Log("Kesme tamamlandı!");
        isCutting = false;
        
        Transform cutObject = activeProgressCanvas.transform.parent;
        if (cutObject != null)
        {
            Transform holdPos = cutObject.Find("HoldPosition");
            if (holdPos != null && holdPos.childCount > 0)
            {
                IngredientReference ingredientRef = holdPos.GetChild(0).GetComponent<IngredientReference>();
                if (ingredientRef != null)
                {
                    ingredientRef.CompleteAction("cut");
                }
            }
        }
        
        if (activeProgressCanvas != null)
        {
            Destroy(activeProgressCanvas);
            activeProgressCanvas = null;
            activeProgressBar = null;
        }
    }

    private bool CheckAllIngredientsOnStove(GameObject stove)
    {
        var menu = TarifCanavari.Instance.currentMenu;
        if (menu == null) return false;

        Transform stoveHoldPosition = stove.transform.Find("HoldPosition");
        var ingredients = stoveHoldPosition.GetComponentsInChildren<IngredientReference>();
        
        return menu.ingredients.All(menuIngredient => 
            ingredients.Any(i => i.ingredient.ingredientName == menuIngredient.name));
    }
}
