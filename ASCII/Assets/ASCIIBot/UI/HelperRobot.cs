using UnityEngine;
using System.Collections.Generic;

public class HelperRobot : MonoBehaviour
{
    [SerializeField] private GameObject chatPanel;
    [SerializeField] private GameObject robotImage;
    [SerializeField] private TMPro.TextMeshProUGUI chatText;
    [SerializeField] private GameObject joystickCanvas;
    
    private Queue<string> messageQueue = new Queue<string>();
    private bool isShowingMessage = false;
    
    private void Start()
    {
        SetUIVisibility(false);
    }
    
    public void ShowMessage(string[] messages)
    {
        foreach (var message in messages)
        {
            messageQueue.Enqueue(message);
        }

        if (!isShowingMessage)
        {
            DisplayNextMessage();
        }
    }
    
    private void DisplayNextMessage()
    {
        if (messageQueue.Count > 0)
        {
            isShowingMessage = true;
            string nextMessage = messageQueue.Dequeue();
            chatText.text = nextMessage;
            SetUIVisibility(true);
        }
        else
        {
            isShowingMessage = false;
            SetUIVisibility(false);

            if(!joystickCanvas.activeSelf)
            {
                joystickCanvas.SetActive(true);
            }
        }
    }
    
    private void SetUIVisibility(bool isVisible)
    {
        chatPanel.SetActive(isVisible);
        robotImage.SetActive(isVisible);
    }
    
    private void Update()
    {
        // Mobil için dokunma kontrolü
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began && isShowingMessage)
            {
                DisplayNextMessage();
            }
        }
        // PC için fare kontrolü devam etsin
        else if (Input.GetMouseButtonDown(0) && isShowingMessage)
        {
            DisplayNextMessage();
        }
    }
}
