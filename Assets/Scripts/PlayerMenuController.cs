using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerMenuController : MonoBehaviour
{
    private int PlayerIndex;

    [SerializeField]
    private TextMeshProUGUI titleText;
    [SerializeField]
    private GameObject readyPanel;
    [SerializeField]
    private GameObject menuPanel;
    [SerializeField]
    private GameObject startingRoomPanel;
    [SerializeField]
    private Button readyButton;
    [SerializeField]
    private Button secretButton;
    // [SerializeField]
    // private Button emptyButton;
    [SerializeField]
    public Button Red;
    [SerializeField]
    public Button Blue;
    [SerializeField]
    public Button Green;
    [SerializeField]
    public Button Yellow;
    [SerializeField]
    private Image image;

    private float ignoreInputTime = .1f;
    private float ignoreUntil = 0f;
    private bool inputEnabled; 

    private Vector2 inputVector = new Vector2();

    private InputActions inputActions;

    void Awake(){
        inputActions = new InputActions();
    }


    public void SetPlayerIndex(int pi){
        PlayerIndex = pi;
        titleText.SetText("Player" + (pi + 1).ToString());
        menuPanel.SetActive(true);
        StartIgnore();
    }

    public void StartIgnore(){
        ignoreUntil = Time.time + ignoreInputTime;
    }

    void Update()
    {
        if(Time.time > ignoreUntil)
            inputEnabled = true;
        
    }

    public void RedButton(){
        if(!inputEnabled)
            return;
        SetColor(PlayerColor.RED);
        image.color = Color.red;
    }

    public void BlueButton(){
        if(!inputEnabled)
            return;
        SetColor(PlayerColor.BLUE);
        image.color = Color.blue;
    }

    public void GreenButton(){
        if(!inputEnabled)
            return;
        SetColor(PlayerColor.GREEN);
        image.color = Color.green;
    }

    public void YellowButton(){
        if(!inputEnabled)
            return;
        SetColor(PlayerColor.YELLOW);
        image.color = Color.magenta;
    }

    public void SetColor(PlayerColor color){
        PlayerManager.Instance.SetPlayerColor(PlayerIndex, color);
        MenuCoordinator.Instance.DisableButtonColor(color);
        startingRoomPanel.SetActive(true);
        menuPanel.SetActive(false);
        secretButton.Select();
        menuPanel.SetActive(false);
        StartIgnore();
    }

    public void ChooseRoom(){
        float x = inputVector.x;
        float y = inputVector.y;
        if(Mathf.Abs(x) > 0 || Mathf.Abs(y) > 0){
            Vector3 v3 = new Vector3(x, y, 0);
            Vector2 v8 = Utility.SnapTo(v3, 45);
            float x8 = (float)Mathf.Round(v8.x * 100f) / 100f;
            float y8 = (float)Mathf.Round(v8.y * 100f) / 100f;
            int startingRoom = GetStartingRoom(x8, y8);
            if(startingRoom == 42) return;
            PlayerManager.Instance.SetPlayerRoom(PlayerIndex ,startingRoom);
            GoToReady();
        }
        return;
    }

    private int GetStartingRoom(float x, float y){
        if(x == -.71f && y == .71f)
            return 0;
        if(x == 0 && y == 1)
            return 1;
        if(x == .71f && y == .71f)
            return 2;
        if(x == -1 && y == 0)
            return 3;
        if(x == 0 && y == 0)
            return 4;
        if(x == 1 && y == 0)
            return 5;
        if(x == -.71f && y == -.71f)
            return 6;
        if(x == 0 && y == -1)
            return 7;
        if(x == .71f && y == -.71f)
            return 8;;
        return 42;
    }

    public void GoToReady(){
        if(!inputEnabled)
            return;
        startingRoomPanel.SetActive(false);
        readyPanel.SetActive(true);
        readyButton.gameObject.SetActive(true);
        readyButton.Select();
        StartIgnore();
    }

    public void ReadyPlayer(){
        if(!inputEnabled)
            return;
        PlayerManager.Instance.ReadyPlayer(PlayerIndex);
        // secretButton.Select();
        readyButton.gameObject.SetActive(false);
    }

    public void HandleInputEvent(InputAction.CallbackContext context){
        if(context.action.name == inputActions.UI.Navigate.name){
            inputVector = context.ReadValue<Vector2>();
        }
    }



}
