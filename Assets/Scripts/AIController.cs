using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AIController : MonoBehaviour
{

    private Navigator navigator;

    List<int> allRooms;
    // public List<int> occupiedRooms;
    List<int> targetedRooms;

    bool changed = false;
    public bool frozen = false;
    float changedTime = 0;

    [SerializeField]
    public int TemporaryFreezeFrames = 120;

    public Dictionary<int, List<CircleGuy>> aiPositions;
    public List<CircleGuy> allBots = new List<CircleGuy>();

    private AIState currentState;

    void Awake(){

    //SHOULD MAYBE IMPLEMENT SYSTEM WHERE EACH PLAYER/BOT IS AWARE OF WHICH ROOM IT IS IN, FOR USE IN MATCHMANAGER

        navigator = FindObjectOfType<Navigator>();
        allRooms = navigator.GetRoomInts;
        // occupiedRooms = new List<int>();
        targetedRooms = new List<int>();
        aiPositions = new Dictionary<int, List<CircleGuy>>();
    }

    void Start(){
        currentState = new ShufflePaths(this);
    }

    void Update(){
        currentState.Tick();
        // if(frozen) return;
        // if(Time.time < 1) return;
        
        // if(aiPositions.Keys.Count > 0 && !changed 
        // && (Mathf.Floor(Time.time) % 7 == 0 || UnityEngine.Random.Range(0, 300) == 0)){
        //     changed = true;
        //     changedTime = Time.time;
        //     StartRandomChange();
        // }
        // if(changed && Time.time - changedTime > 4) 
        //     changed = false;
    }

    public void AddBot(CircleGuy bot, int room){
        // occupiedRooms.Add(room);
        aiPositions.Add(room, new List<CircleGuy>(){bot});
        allBots.Add(bot);
    }

    // public void StartControl(){
    //     foreach(int roomID in aiPositions.Keys){
    //         Room room = navigator.GetRoom(roomID);
    //         foreach(CircleGuy bot in aiPositions[roomID]){
    //             bot.StartWander(room);
    //         }
            
    //     }
    // }

    public void Freeze(){
        foreach(List<CircleGuy> bots in aiPositions.Values){
            foreach(CircleGuy bot in bots){
                bot.Freeze();
            } 
        }
        // frozen = true;
        SetState(new Freeze(this));
    }

    

    public void SetState(AIState state){
        if(currentState != null)
            currentState.OnStateExit();

        currentState = state;
        gameObject.name = "AIController - " + state.GetType().Name;

        if(currentState != null)
            currentState.OnStateEnter();
    }

    private List<int> GetOccupiedRooms(){
        return aiPositions.Keys.ToList();
    }

    private void RemoveFromRoom(int roomID, CircleGuy bot){
        aiPositions[roomID].Remove(bot);
        if(aiPositions[roomID].Count == 0)
            aiPositions.Remove(roomID);
    }

    private void AddToRoom(int roomID, CircleGuy bot){
        if(aiPositions.ContainsKey(roomID)){
            aiPositions[roomID].Add(bot);
        } else {
            aiPositions.Add(roomID, new List<CircleGuy>(){bot});
        }
        
    }

    public void StartRandomChange(){
        List<int> occupiedRooms = GetOccupiedRooms();
        int startRoom = occupiedRooms[UnityEngine.Random.Range(0, occupiedRooms.Count)];
        RoomChange(startRoom);
    }

    private void StartTargetedChange(int startRoom){
        if(!GetOccupiedRooms().Contains(startRoom)) return;
        RoomChange(startRoom);
    }

    public void Exodus(int room){
        foreach(CircleGuy bot in allBots){
            StartTargetedChange(room);
        }
    }

    public void TemporaryFreeze(){

        currentState.TemporaryFreeze();
        foreach(CircleGuy bot in allBots){
            bot.TemporaryFreeze(TemporaryFreezeFrames);
        }
    }

    private void RoomChange(int startRoom){
        // if(aiPositions[startRoom].gameObject.activeSelf == false) return; //for testing
        CircleGuy bot = aiPositions[startRoom].First();
        RemoveFromRoom(startRoom, bot);
        List<int> endRooms = allRooms.Where(r => 
            r != startRoom && !targetedRooms.Contains(r)).ToList();
        int endRoom = endRooms[UnityEngine.Random.Range(0, endRooms.Count)];
        targetedRooms.Add(endRoom);
        Path path = navigator.GeneratePathFromRoom(startRoom, endRoom);
        // aiPositions.Remove(startRoom);
        MovementAction moveAction = new MovementAction(path, bot, endRoom);
        // bot.SetMovementAction(moveAction);
        Action targetedCallback = () => StartTargetedChange(endRoom);
        Action finishedCallback = () => MoveFinishedCallback(bot, endRoom);
        // bot.StopWander();
        bot.StartFollowPath(moveAction, targetedCallback, finishedCallback);
    }

    private void MoveFinishedCallback(CircleGuy bot, int endRoom){
        if(targetedRooms.Contains(endRoom))
            targetedRooms.Remove(endRoom);
        AddToRoom(endRoom, bot);
        // bot.ClearMovementAction();
        // Room room = navigator.GetRoom(endRoom);
        // bot.StartWander(room);
    }
    
    public void QueueMoveAllToRoom(int targetRoom){
        Debug.Log("Moving all to room " + targetRoom);
        targetedRooms.Clear();
        aiPositions.Clear();
        foreach (CircleGuy bot in allBots){
            // bot.StopWander();
            Action targetedCallback = () => {};
            Action finishedCallback = () => MoveFinishedCallback(bot, targetRoom);
            Path path;
            if(bot.InRoom){
                path = navigator.GeneratePathFromRoom(bot.CurrentRoom.ID, targetRoom);
            } else {
                path = navigator.GeneratePathInHall(bot.transform.position, targetRoom);
            }
            MovementAction moveAction = new MovementAction(path, bot, targetRoom);
            // bot.SetMovementAction(moveAction);
            bot.StartFollowPath(moveAction, targetedCallback, finishedCallback);
        }
    }

    public bool AllInSameRoom(){
        Room firstRoom = allBots[0].CurrentRoom;
        if(firstRoom == null) return false;
        int firstID = firstRoom.ID;
        foreach(CircleGuy bot in allBots){
            if(bot.currentState is FollowingPath) return false;
            if(bot.CurrentRoom == null) return false;
            if(bot.CurrentRoom.ID != firstID) return false;
        }
        return true;
    }
}


