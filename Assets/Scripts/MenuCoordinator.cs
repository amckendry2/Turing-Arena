using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCoordinator : MonoBehaviour
{
    private List<PlayerMenuController> menuControllers = new List<PlayerMenuController>();

    private bool redDisabled = false;
    private bool blueDisabled = false;
    private bool greenDisabled = false;
    private bool yellowDisabled = false;


    public static MenuCoordinator Instance { get; private set; }

    void Awake(){
        if(Instance != null){
            Debug.Log("Trying to instantiate multiple singletons!");
            Destroy(gameObject);
        } else {
            Instance = this;
        }
    }

    public void RegisterMenu(PlayerMenuController menu){
        menuControllers.Add(menu);
    }

    public void RedisableButtons(){
        if(redDisabled)
            DisableButtonColor(PlayerColor.RED);
        if(blueDisabled)
            DisableButtonColor(PlayerColor.BLUE);
        if(greenDisabled)
            DisableButtonColor(PlayerColor.GREEN);
        if(yellowDisabled)
            DisableButtonColor(PlayerColor.YELLOW);
        
    }

    public void DisableButtonColor(PlayerColor color){
        foreach(PlayerMenuController menu in menuControllers){
            switch(color){
                case PlayerColor.RED:
                    menu.Red.interactable = false;
                    redDisabled = true;
                    break;
                case PlayerColor.BLUE:
                    menu.Blue.interactable = false;
                    blueDisabled = true;
                    break;
                case PlayerColor.GREEN:
                    menu.Green.interactable = false;
                    greenDisabled = true;
                    break;
                case PlayerColor.YELLOW:
                    menu.Yellow.interactable = false;
                    yellowDisabled = true;
                    break;
                default:
                    break;
            }
        }
    }
}
