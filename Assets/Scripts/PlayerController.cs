using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{

    InputActions inputActions;
    private event EventHandler InputEvent;

    public void AddInputListener(Action<InputAction.CallbackContext> handler){
        EventHandler eventHandler = new EventHandler((object obj, EventArgs args) => {
            InputEventArgs inpArgs = (InputEventArgs) args;
            handler(inpArgs.data);
        });
        InputEvent += eventHandler;
    } 

    void Awake(){
        inputActions = new InputActions();
        GetComponent<PlayerInput>().onActionTriggered += GetStick;
    }

    public Vector2 inputVector {get; private set;} = new Vector2();

    void GetStick(InputAction.CallbackContext context){
        InputEventArgs inputEventArgs = new InputEventArgs(context);
        InputEvent?.Invoke(this, inputEventArgs);
        // if(context.action.name == inputActions.UI.Navigate.name)
        //     inputVector = context.ReadValue<Vector2>();
        if(context.action.name == inputActions.Player.Reboot.name){
            Destroy(PlayerManager.Instance.gameObject);
            SceneManager.LoadScene("Character Select");
        }
    }

    public void DeregisterInputs(){
        GetComponent<PlayerInput>().onActionTriggered -= GetStick;
    }

}

public class InputEventArgs : EventArgs
{
    public readonly InputAction.CallbackContext data;
    public InputEventArgs(InputAction.CallbackContext data){
        this.data = data;
    }
}
