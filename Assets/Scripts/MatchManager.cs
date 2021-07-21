using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class MatchManager : MonoBehaviour
{

    [SerializeField]
    private bool testing;

    [SerializeField]
    private GameObject AIController;
    [SerializeField]
    private GameObject Player;
    [SerializeField]
    private GameObject CircleGuy;    
    [SerializeField]
    private float SlowMoSpeed = .25f;

    public float TimeScale = 1;
    private float slowMoScale = .25f;

    private Dictionary<PlayerColor, Color> colorDict = new Dictionary<PlayerColor, Color>{
        {PlayerColor.RED, Color.red},
        {PlayerColor.BLUE, Color.blue},
        {PlayerColor.GREEN, Color.green},
        {PlayerColor.YELLOW, Color.magenta}
    };




    // [SerializeField]
    // private GameObject redKill;
    // [SerializeField]
    // private GameObject blueKill;
    // [SerializeField]
    // private GameObject greenKill;
    // [SerializeField]
    // private GameObject yellowKill;

    // [SerializeField]
    // private Image Red;
    // [SerializeField]
    // private Image Blue;
    // [SerializeField]
    // private Image Green;
    // [SerializeField]
    // private Image Yellow;


    // private Navigator navigator;

    [SerializeField]
    private GameObject WinMessage;
    [SerializeField]
    private TextMeshProUGUI WinText;

    public static MatchManager Instance {get; set;}

    List<Player> spawnedPlayers = new List<Player>();
    List<AIController> spawnedAIControllers = new List<AIController>();
    private Dictionary<AIController, Player> aiPlayers = new Dictionary<AIController, Player>();
    private Dictionary<FreezeTimer, AIController> freezeTimerDict = new Dictionary<FreezeTimer, AIController>();

    private Dictionary<PlayerColor, PlayerColor> killList;
    private List<PlayerColor> availableTargets = new List<PlayerColor>();

    private List<Tilemap> roomTilemaps;

    private Dictionary<PlayerColor, Image> colorImageDict;
    private List<int> spawnedBullets;

    void Awake(){
        // QualitySettings.vSyncCount = 0;  // VSync must be disabled
        // Application.targetFrameRate = 60;

        // colorImageDict = new Dictionary<PlayerColor, Image>{
        //     {PlayerColor.RED, Red},
        //     {PlayerColor.BLUE, Blue},
        //     {PlayerColor.GREEN, Green},
        //     {PlayerColor.YELLOW, Yellow}
        // };


        if(Instance != null){
            Debug.Log("Trying to instantiate multiple singletons!");
            Destroy(gameObject);
        } else {
            Instance = this;
        }
        // navigator = Navigator.Instance;
        // SpawnActors();
    }

    void Start(){
        SpawnActors();
        spawnedBullets = new List<int>();
        // SpawnBullet(Random.Range(0, Navigator.Instance.rooms.Count));
        StartCoroutine(UpdateFreezeTimers());
    }

    void Update(){
        SetTimeScale();
    }

    IEnumerator UpdateFreezeTimers(){
        foreach(FreezeTimer ft in freezeTimerDict.Keys){
            
            AIController ai = freezeTimerDict[ft];
            
            if(ft.frozen){
                if(ai.frozen){
                    yield return new WaitForSeconds(1f);
                } else {
                    ft.frozen = false;
                    ft.counter = Random.Range(20, 60);
                    ft.counterObj.text = ft.counter.ToString();
                    yield return new WaitForSeconds(1f);
                }
            }            
            if(ft.counter == 0){
                ai.TemporaryFreeze();
                ft.counterObj.text = "FROZEN";
                ft.frozen = true;
            } else {
                ft.counter--;
                ft.counterObj.text = ft.counter.ToString();
            }
            yield return new WaitForSeconds(1f);
        }
    }
    

    void SetTimeScale(){
        if(spawnedPlayers.Any(p => p.currentState is Aiming)){
            TimeScale = slowMoScale;
        } else {
            TimeScale = 1;
        }
    }

    private void SpawnBullet(int spawnRoom){
        // Room room = Navigator.Instance.GetRoom(spawnRoom);
        // room.SpawnBullet();
        // spawnedBullets.Add(spawnRoom);
    }

    // public bool CheckForKill(PlayerColor shooter, PlayerColor hit){
    //     return killList[shooter] == hit;
    // }

    public void RegisterTestingPlayer(Player player){
        spawnedPlayers.Add(player);
    }

    private List<int> ListWithMissing(int listSize, int numRemoved){
        List<int> returnList = new List<int>(listSize);
        for(int i = 0; i < listSize; i ++){
            returnList.Add(i);
        }
        for(int i = 0; i < numRemoved; i ++){
            returnList.RemoveAt(Random.Range(0, returnList.Count));
        }
        return returnList;
    }

    private void SpawnActors(){
        //FOR TESTING
        if(testing){
            List<PlayerColor> ColorList = new List<PlayerColor>{PlayerColor.RED, PlayerColor.BLUE, PlayerColor.GREEN, PlayerColor.YELLOW};
            int spawnPoint = 0;
            foreach(PlayerColor color in ColorList){
                GameObject aiControllerObj = (GameObject)Instantiate(AIController, Vector3.zero, Quaternion.identity);
                AIController aiController = aiControllerObj.GetComponent<AIController>();
                spawnedAIControllers.Add(aiController);
                // for(int i = 0; i < navigator.RoomCount - 1; i++){
                //     SpawnBot(color, i, spawnPoint, aiController);
                // }
                List<int> listWithMissing = ListWithMissing(Navigator.Instance.RoomCount, 5);
                foreach(int room in listWithMissing){
                    SpawnBot(color, room, spawnPoint, aiController);
                }
                // aiController.StartControl();
                spawnPoint++;
            }
            killList = new Dictionary<PlayerColor, PlayerColor>(){
                {PlayerColor.RED, PlayerColor.BLUE}, 
                {PlayerColor.BLUE, PlayerColor.GREEN},
                {PlayerColor.GREEN, PlayerColor.YELLOW},
                {PlayerColor.YELLOW, PlayerColor.RED}
            };
            // GenerateKillChart();
            return;
        }
        //////////////


        foreach(PlayerConfiguration config in PlayerManager.Instance.playerConfigs){
            GameObject aiControllerObj = (GameObject)Instantiate(AIController, Vector3.zero, Quaternion.identity);
            AIController aiController = aiControllerObj.GetComponent<AIController>();
            spawnedAIControllers.Add(aiController);
            Player player = SpawnPlayer(config);
            availableTargets.Add(player.myColor);
            spawnedPlayers.Add(player);
            aiPlayers.Add(aiController, player);

            for(int i = 0; i < Navigator.Instance.RoomCount; i++){
                if(i != config.StartingRoom)
                    SpawnBot(config.Color, i, config.PlayerIndex, aiController);     
            }
            
            // aiController.StartControl();
        }

        // availableTargets.Shuffle();
        // killList = new Dictionary<PlayerColor, PlayerColor>();
        // bool threeOrMore = spawnedPlayers.Count > 2;
        // foreach(Player player in spawnedPlayers){
        //     PlayerColor targetColor = availableTargets.First(c =>
        //         c != player.myColor 
        //         && (threeOrMore ? killList.ContainsKey(c) ? killList[c] != player.myColor : true : true));
        //     killList[player.myColor] = targetColor;
        //     availableTargets.Remove(targetColor);
        // }
        // GenerateKillChart();
    }

    // private void GenerateKillChart(){
    //     foreach(PlayerColor color in killList.Keys){
    //         Debug.Log(color);
    //         switch(color){
    //             case PlayerColor.RED:
    //                 Instantiate(colorImageDict[killList[color]], redKill.transform);
    //                 break;
    //             case PlayerColor.BLUE:
    //                Instantiate(colorImageDict[killList[color]], blueKill.transform);
    //                 break;
    //             case PlayerColor.GREEN:
    //                 Instantiate(colorImageDict[killList[color]], greenKill.transform);
    //                 break;
    //             case PlayerColor.YELLOW:
    //                 Instantiate(colorImageDict[killList[color]], yellowKill.transform);
    //                 break;
    //         }
    //     }
    // }

    private Player SpawnPlayer(PlayerConfiguration config){
        // Debug.LogError("nav = " + navigator.rooms);
        // Debug.LogError("startingroom = " + config.StartingRoom);
        // if(navigator.rooms[config.StartingRoom] == null) return null;
        Room room = Navigator.Instance.rooms[config.StartingRoom];
        GameObject playerObj = Instantiate(
            Player,
            room.spawns[config.PlayerIndex].transform.position,
            Quaternion.identity);
        playerObj.GetComponent<SpriteRenderer>().color = colorDict[config.Color];
        Player player = playerObj.GetComponent<Player>();
        // player.LinkInputs(config);
        config.playerController.AddInputListener(player.HandleInputEvent);
        player.SetColor(config.Color);
        return player;
    }

    private void SpawnBot(PlayerColor color, int roomNum, int spawn, AIController aiController){
        GameObject botObj = Instantiate(
            CircleGuy,
            Navigator.Instance.rooms[roomNum].spawns[spawn].transform.position, 
            Quaternion.identity,
            aiController.transform);
        
        botObj.GetComponent<SpriteRenderer>().color = colorDict[color];
        CircleGuy bot = botObj.GetComponent<CircleGuy>();
        aiController.AddBot(bot, roomNum);
    }

    // public void FreezeGame(){
    //     FreezePlayers();
    //     FreezeBots();
    // }

    // public void FreezePlayers(){
    //     foreach(Player player in spawnedPlayers){
    //         player.SetState(new Frozen(player));
    //     }
    // }

    // public void FreezeBots(){
    //     foreach(AIController ai in spawnedAIControllers){
    //         ai.Freeze();
    //     }
    // }

    public bool PlayerIsFiring(){
        return spawnedPlayers.Any(p => p.currentState is Aiming);
    }

    public void DisplayWinMessage(PlayerColor color){
        string colorText = "";
        switch(color){
            case PlayerColor.RED:
                colorText = "RED";
                break;
            case PlayerColor.BLUE:
                colorText = "BLUE";
                break;
            case PlayerColor.GREEN:
                colorText = "GREEN";
                break;
            case PlayerColor.YELLOW:
                colorText = "PINK";
                break;
            default:
                colorText = "????";
                break;
        }
        WinText.text = colorText + " PLAYER WINS!";
        WinMessage.SetActive(true);
    }

    private class FreezeTimer{
        public bool frozen = false;
        public int counter;
        public TextMeshProUGUI counterObj;
    }

}

public enum PlayerColor{
    RED, BLUE, YELLOW, GREEN
}

