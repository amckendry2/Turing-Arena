using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class CircleGuyState
{
    protected CircleGuy circleGuy;

    public abstract void Tick();

    public virtual void OnStateEnter(){}
    public virtual void OnStateExit(){}
    
    public virtual void QueueMovementAction(MovementAction action, Action targetedCallback, Action moveFinishedCallback){}
    public virtual void QueueWandering(){}

    public CircleGuyState(CircleGuy circleGuy){
        this.circleGuy = circleGuy;
    }    
}

public class Idle : CircleGuyState 
{
    public Idle(CircleGuy circleGuy) : base(circleGuy){}

    public override void Tick(){}

    public override void QueueMovementAction(MovementAction action, Action targetedCallback, Action moveFinishedCallback){
        circleGuy.SetState(new FollowingPath(action, targetedCallback, moveFinishedCallback, circleGuy));
    }

    public override void QueueWandering(){
        circleGuy.SetState(new Wandering(circleGuy));
    }
}

public class Waiting : CircleGuyState
{
    private float waitTime;
    private float frame = 0;
    private CircleGuyState nextState;

    public Waiting(float waitTime, CircleGuyState initialState, CircleGuy circleGuy) : base(circleGuy){
        this.waitTime = UnityEngine.Random.Range(0, waitTime);
        this.nextState = initialState;
    }

    public override void Tick(){
        if(frame > waitTime)
            circleGuy.SetState(nextState);
        frame++;        
    }

    public override void QueueMovementAction(MovementAction action, Action targetedCallback, Action moveFinishedCallback){
        nextState = new FollowingPath(action, targetedCallback, moveFinishedCallback, circleGuy);
    }

    public override void QueueWandering(){
        nextState = new Wandering(circleGuy);
    }
}


public class Wandering : CircleGuyState
{   
    bool routineFinished = false;
    bool followingPathQueued = false;
    FollowingPath nextFollowState;

    public Wandering(CircleGuy circleGuy) : base (circleGuy){}

    private void onRoutineFinished(){
        routineFinished = true;
    }

    public override void OnStateEnter() {
        Room room;
        if(circleGuy.InRoom){
            room = circleGuy.CurrentRoom;
        } else {
            room = Navigator.Instance.GetClosestRoom(circleGuy.transform.position);
        }
         
        if(room == null)Debug.Log("not in room: " + circleGuy.transform.position);
        Vector2 roomCenter = room.transform.position;
        float area = circleGuy.WanderSize;
        
        float randomX = UnityEngine.Random.Range(roomCenter.x - area, roomCenter.x + area);
        float randomY = UnityEngine.Random.Range(roomCenter.y - area, roomCenter.y + area);
        bool diagRandom = UnityEngine.Random.Range(0, 2) == 1;
        
        PathNode nextNode = new PathNode(room.ID, new Vector2(randomX, randomY));
        IEnumerator cr = circleGuy.MoveToNode(nextNode, false, diagRandom, true, onRoutineFinished);
        
        circleGuy.StartCoroutine(cr);
    }

    public override void Tick(){
        if(routineFinished){
            CircleGuyState nextState;
            if(followingPathQueued){
                nextState = nextFollowState;
            } else {
                nextState = new Waiting(circleGuy.WanderWait, new Wandering(circleGuy), circleGuy);
            }
            circleGuy.SetState(nextState);
        }
    }

    public override void QueueMovementAction(MovementAction action, Action targetedCallback, Action moveFinishedCallback){
        followingPathQueued = true;
        nextFollowState = new FollowingPath(action, targetedCallback, moveFinishedCallback, circleGuy);
    }
}


public class FollowingPath : CircleGuyState
{
    private bool firstNode = true;
    private bool firstOrSecondNode = true;
    private bool didTargetedCallback = false;
    private bool reachedNode = false;
    private bool startedInHall = false;

    private MovementAction action;

    private Action targetedCallback;
    private Action moveFinishedCallback;

    public FollowingPath(MovementAction action, Action targetedCallback, Action moveFinishedCallback, CircleGuy circleGuy) : base(circleGuy){
        this.targetedCallback = targetedCallback;
        this.moveFinishedCallback = moveFinishedCallback;
        this.action = action;
        this.startedInHall = !circleGuy.InRoom;
    }

    public override void OnStateEnter(){
        SetDestination();
    }

    public override void Tick(){
        if(!didTargetedCallback && ChanceByDistance()){
            targetedCallback();
            didTargetedCallback = true;
        }
        if(reachedNode)
            onRoutineFinished();      
    }

    private void SetDestination(){

        reachedNode = false;

        // if(!firstNode)
            // ShiftPos();
        
        Vector3 target = action.path.currentTarget.pos;
        Vector3 currentPos = circleGuy.transform.position;
        float diagThreshold = circleGuy.DiagThreshold;
        
        float xDist = Mathf.Abs(target.x - currentPos.x);
        float yDist = Mathf.Abs(target.y - currentPos.y);
        bool horizontal = xDist > yDist;
        bool stopShort = !firstNode;
        bool shouldGoDiagonal = firstNode && (horizontal ? yDist > diagThreshold : xDist > diagThreshold);
        bool noSmooth = firstOrSecondNode;
        if(startedInHall){
            stopShort = true;
            shouldGoDiagonal = false;
            noSmooth = false;
        }
        IEnumerator cr = circleGuy.MoveToNode(action.path.currentTarget, stopShort, shouldGoDiagonal, noSmooth, onRoutineFinished);
        circleGuy.StartCoroutine(cr);
    }


    private void onRoutineFinished(){
        
        if(action.path.currentTarget.distance == 0){
            circleGuy.hitWall = false;
            moveFinishedCallback();
            circleGuy.SetState(new Wandering(circleGuy));
            return;
        }
        
        reachedNode = true;
        if(!circleGuy.hitWall) action.path.Advance();
        circleGuy.hitWall = false;

        if(firstNode){
            firstNode = false;
        } else if(firstOrSecondNode){
            firstOrSecondNode = false;
        }

        SetDestination();
    }

    public override void QueueMovementAction(MovementAction movementAction, Action targetedCallback, Action moveFinishedCallback){
        circleGuy.StopAllCoroutines();
        CircleGuyState nextState = new FollowingPath(movementAction, targetedCallback, moveFinishedCallback, circleGuy);
        circleGuy.SetState(new Waiting(circleGuy.WanderWait, nextState, circleGuy));
    }


    private bool ChanceByDistance(){
        float chance = 100 - action.path.GetRemainingPercentage(circleGuy.transform.position);
        float rnd = UnityEngine.Random.Range(0, 1f) - (chance * chance);
        if(rnd < circleGuy.BaseTargetedMoveChance)
            return true;
        return false;
    }

    
    // private void ShiftPos(){
    //     bool lastNode = action.path.currentTarget.distance == 0;
    //     action.path.currentTarget.pos += new Vector2(1, 1) * (lastNode ?
    //         UnityEngine.Random.Range(-2, 3) : 
    //         UnityEngine.Random.Range(-3, 4) * .1f);
    // }
}
