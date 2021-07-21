using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    public List<PlayerConfiguration> playerConfigs = new List<PlayerConfiguration>();
    public GameObject playerSetupMenuPrefab;
    private int MaxPlayers = 4;

    public static PlayerManager Instance { get; private set; }

    void Awake(){
        if(Instance != null){
            Debug.Log("Trying to instantiate multiple singletons!");
            Destroy(gameObject);
        } else {
            Instance = this;
            DontDestroyOnLoad(Instance);
        }
    }

    public void Reset(Scene scene, LoadSceneMode mode){
        playerConfigs.Clear();
        GetComponent<PlayerInputManager>().EnableJoining();
    }

    public void HandlePlayerJoin(PlayerInput pi){
        if(!playerConfigs.Any(p => p.PlayerIndex == pi.playerIndex))
        {
            pi.transform.SetParent(transform);
            PlayerConfiguration pConfig = new PlayerConfiguration(pi);
            var rootMenu = GameObject.Find("MainCanvas");
            if (rootMenu != null){
                var menu = Instantiate(playerSetupMenuPrefab, rootMenu.transform);
                pi.uiInputModule = menu.GetComponentInChildren<InputSystemUIInputModule>();
                PlayerMenuController pmc = menu.GetComponent<PlayerMenuController>();
                pmc.SetPlayerIndex(pi.playerIndex);
                pConfig.playerController.AddInputListener(pmc.HandleInputEvent);
                MenuCoordinator.Instance.RegisterMenu(pmc);
                MenuCoordinator.Instance.RedisableButtons();
                playerConfigs.Add(pConfig);
            }   
        }
    }

    public void SetPlayerColor(int index, PlayerColor color){
        playerConfigs[index].Color = color;
    }

    public void SetPlayerRoom(int index, int room){
        playerConfigs[index].StartingRoom = room;
    }

    public void ReadyPlayer(int index){
        playerConfigs[index].IsReady = true;
        if(playerConfigs.All(p => p.IsReady == true) && playerConfigs.Count > 0){
            GetComponent<PlayerInputManager>().DisableJoining();
            SceneManager.LoadScene("9Rooms");
        }
    }

}

public class PlayerConfiguration
{
    public PlayerInput Input { get; set; }
    public PlayerController playerController { get; set;}
    public int PlayerIndex { get; set; }
    public bool IsReady { get; set; }
    public PlayerColor Color { get; set; }
    public int StartingRoom { get; set; }
    // public PlayerController playerController {get; set;}

    public PlayerConfiguration(PlayerInput pi){
        PlayerIndex = pi.playerIndex;
        Input = pi;
        playerController = pi.GetComponent<PlayerController>();
        // playerController = pi.GetComponent<PlayerController>();
    }
}
