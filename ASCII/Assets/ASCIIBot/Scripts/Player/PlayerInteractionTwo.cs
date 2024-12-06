using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInteractionTwo : MonoBehaviour
{
    [Header("Raycast Ayarları")] [SerializeField]
    private float rayDistance = 2f;

    [SerializeField] private LayerMask interactionLayer;

    [Header("Etkileşim")] [SerializeField] private Transform holdPosition;
    private bool isHolding = false;

    [Header("ProgressBar")] [SerializeField]
    private ProgressBar progressBarPrefab;

    [SerializeField] private GameObject canvasPrefab;
    [SerializeField] private float progressBarScale = 0.5f;
    private ProgressBar activeProgressBar;
    private GameObject activeProgressCanvas;
    private Camera mainCamera;

    [Header("FoodRelated")] [SerializeField]
    private List<GameObject> CookedRecipes;

    [SerializeField] private List<GameObject> platePrefabs;
    [SerializeField] private GameObject potPrefab;
    private GameObject currentStove;
    private bool isWashing = false;
    private bool isCutting = false;
    private bool isCooking = false;
    private int plateSpawnCount = 0;
    private int MAX_PLATES = 2;

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

        if (Input.GetKeyDown(KeyCode.Q))
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
        Vector3 rayOrigin = playerTransform.position + Vector3.up * 0.5f;
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
                case "npctable":
                    Debug.Log("NPC Masası tespit edildi!");
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
                case "npctable":
                    InteractWithNPCTable(hitObject);
                    break;
                case "Trash":
                    InteractWithTrash(hitObject);
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

    private void InteractWithNPCTable(GameObject tableObject)
    {
        Transform tableHoldPosition = tableObject.transform.Find("HoldPosition");

        if (isHolding && holdPosition.childCount > 0)
        {
            if (tableHoldPosition != null && tableHoldPosition.childCount == 0)
            {
                Transform heldItem = holdPosition.GetChild(0);

                heldItem.SetParent(tableHoldPosition);
                heldItem.localPosition = Vector3.zero;
                heldItem.localRotation = Quaternion.identity;

                isHolding = false;
                Debug.Log("Yemek NPC masasına bırakıldı.");
            }
            else
            {
                Debug.Log("NPC masası dolu, yemek bırakılamaz!");
            }
        }
        else if (!isHolding && tableHoldPosition != null && tableHoldPosition.childCount > 0)
        {
            Transform tableItem = tableHoldPosition.GetChild(0);

            tableItem.SetParent(holdPosition);
            tableItem.localPosition = Vector3.zero;
            tableItem.localRotation = Quaternion.identity;

            isHolding = true;
            Debug.Log("Yemek NPC masasından alındı.");
        }
    }

    private void InteractWithStove(GameObject stoveObject)
    {
        Transform stoveHoldPosition = stoveObject.transform.Find("HoldPosition");
        Transform spawnPosition = stoveObject.transform.Find("SpawnPosition");
        currentStove = stoveObject;
        var menu = TarifCanavari.Instance.currentMenu;

        if (!isHolding && spawnPosition != null && spawnPosition.childCount > 0)
        {
            if (plateSpawnCount <= MAX_PLATES && platePrefabs.Count > 0)
            {
                int randomPlateIndex = Random.Range(0, platePrefabs.Count);
                GameObject spawnedPlate = Instantiate(platePrefabs[randomPlateIndex], holdPosition.position,
                    holdPosition.rotation);
                spawnedPlate.transform.SetParent(holdPosition);
                spawnedPlate.tag = "yemek";

                isHolding = true;
                plateSpawnCount++;
            }
            else
            {
                foreach (Transform child in spawnPosition)
                {
                    Destroy(child.gameObject);
                }

                if (potPrefab != null)
                {
                    GameObject newPot = Instantiate(potPrefab, stoveHoldPosition.position, stoveHoldPosition.rotation);
                    newPot.transform.SetParent(currentStove.transform);
                }
            }

            return;
        }

        Debug.Log(isHolding);
        if (isHolding && holdPosition.childCount > 0)
        {
            Debug.Log(holdPosition.GetChild(0));
            if (stoveHoldPosition != null && stoveHoldPosition.childCount < menu.ingredients.Count)
            {
                Transform heldItem = holdPosition.GetChild(0);
                Debug.Log(heldItem.name);
                IngredientReference ingredientRef = heldItem.GetComponent<IngredientReference>();

                bool ingredientAlreadyAdded = false;
                foreach (Transform child in stoveHoldPosition)
                {
                    var childIngredient = child.GetComponent<IngredientReference>();
                    if (childIngredient != null && childIngredient.ingredient.ingredientName ==
                        ingredientRef.ingredient.ingredientName)
                    {
                        ingredientAlreadyAdded = true;
                        break;
                    }
                }

                if (ingredientRef != null && !ingredientAlreadyAdded)
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
        plateSpawnCount = 0;

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

                    Destroy(child.gameObject);
                }
            }

            Transform potTransform = currentStove.transform.Find("Tencere(Clone)");
            if (potTransform != null && potTransform.CompareTag("Tencere"))
            {
                Destroy(potTransform.gameObject);
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
                GameObject cookedItem =
                    Instantiate(CookedRecipes[randomIndex], spawnPosition.position, Quaternion.identity);
                cookedItem.transform.SetParent(spawnPosition);
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

        Transform cuttingBoardTransform = activeProgressCanvas.transform.parent;
        if (cuttingBoardTransform != null)
        {
            Transform holdPos = cuttingBoardTransform.Find("HoldPosition");
            if (holdPos != null && holdPos.childCount > 0)
            {
                Transform originalItemTransform = holdPos.GetChild(0);
                IngredientReference originalIngredientRef = originalItemTransform.GetComponent<IngredientReference>();

                if (originalIngredientRef != null)
                {
                    originalIngredientRef.CompleteAction("cut");

                    Ingredient ingredient = originalIngredientRef.ingredient;

                    if (ingredient.slicedPrefab != null)
                    {
                        GameObject slicedObject = Instantiate(ingredient.slicedPrefab, holdPosition);

                        IngredientReference slicedIngredientRef = slicedObject.AddComponent<IngredientReference>();

                        slicedIngredientRef.ingredient = originalIngredientRef.ingredient;
                        slicedIngredientRef.requiredActionsOrder = originalIngredientRef.requiredActionsOrder;
                        slicedIngredientRef.isWashed = originalIngredientRef.isWashed;
                        slicedIngredientRef.isCooked = originalIngredientRef.isCooked;
                        slicedIngredientRef.isCut = originalIngredientRef.isCut;
                        slicedIngredientRef.currentActionIndex = originalIngredientRef.currentActionIndex;
                        slicedObject.transform.localPosition = Vector3.zero;
                        slicedObject.transform.localRotation = Quaternion.identity;
                        slicedObject.transform.SetParent(holdPosition);
                        isHolding = true;

                        Destroy(originalIngredientRef.gameObject);
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
    }

    private void InteractWithTrash(GameObject trash)
    {
        if (isHolding && holdPosition.childCount > 0)
        {
            Transform holdPos = holdPosition.GetChild(0);
            Destroy(holdPos.gameObject);
            isHolding = false;
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