using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CircleGuy : ActorWithPhysics
{   
    [SerializeField]
    private int MinSmoothTurnFrames = 3;
    [SerializeField]
    private int MaxSmoothTurnFrames = 6;
    [SerializeField]
    private float StopShortAmount = 2;
    [SerializeField] 
    public float BaseTargetedMoveChance = .00001f;
    [SerializeField]
    public float WanderSize = 3f;
    [SerializeField]
    public float DiagThreshold = 1;

    [SerializeField]
    protected float AccelSpeed = .1f;
    [SerializeField]
    private float Drag = .1f;
    [SerializeField]
    protected float MaxSpeed = 1f;
    [SerializeField]
    public float StartWait = 3f;
    [SerializeField]
    public float WanderWait = 1.5f;


    public MovementAction movementAction {get; private set;}

    public bool hitWall = false;
    bool wander = true;
    bool stillWandering = true;
    bool firstWander = true;

    public CircleGuyState currentState;

    public bool InRoom = false;
    public Room CurrentRoom = null;

    void Start(){
        UpdateRoom();
        CircleGuyState initState = new Wandering(this);
        SetState(new Waiting(StartWait, initState, this));
        SetDrag(Drag);
    }

    protected override void Update(){
        base.Update();
        UpdateRoom();
        currentState.Tick();
    }

    private void UpdateRoom(){
        Room roomResult = Navigator.Instance.GetRoomFromPoint(transform.position);
        if(roomResult != null){
            CurrentRoom = roomResult;
            InRoom = true;
        } else {
            CurrentRoom = null;
            InRoom = false;
        }
    }

    public void SetState(CircleGuyState state){
        if(currentState != null)
            currentState.OnStateExit();

        currentState = state;

        gameObject.name = "CircleGuy - " + state.GetType().Name;

        if(currentState != null)
            currentState.OnStateEnter();
    }

    public void StartWander(Room room){
        // wander = true;
        currentState.QueueWandering();
        // StartCoroutine(WanderInRoom(room, WanderSize));
    }

    public void StopWander(){
        
        // wander = false;
    }

    // public void SetMovementAction(MovementAction action){
    //     movementAction = action;
    // }

    // public void ClearMovementAction(){
    //     movementAction = null;
    // }

    public void Freeze(){
        // StopAllCoroutines();
        SetState(new Idle(this));
    }

    public void TemporaryFreeze(int frames){
        SetState(new Waiting(frames, currentState, this));
    }

    public void StartFollowPath(MovementAction action, Action targetedCallback, Action moveFinishedCallback){
        // movementAction = action;
        // Action finishedCallback = () => moveFinishedCallback();
        // StartCoroutine(ChanceByDistance(targetedCallback));
        // StartCoroutine(FollowPath(moveFinishedCallback));
        currentState.QueueMovementAction(action, targetedCallback, moveFinishedCallback);
    }


    // IEnumerator ChanceByDistance(Action targetedChangeCallback){
    //     while(true){
    //         float chance = 100 - movementAction.path.GetRemainingPercentage(transform.position);
    //         float rnd = UnityEngine.Random.Range(0, 1f) - (chance * chance);
    //         if(rnd < BaseTargetedMoveChance){
    //             targetedChangeCallback();
    //             break;
    //         }
    //         yield return null;
    //     }
    // }

    // IEnumerator FollowPath(Action finishedCallback){
    //     while(stillWandering){
    //         yield return null;
    //     }
    //     int firstNode = 2;
    //     while(true){
    //         Vector3 target = movementAction.path.currentTarget.pos; 
    //         bool lastNode = movementAction.path.currentTarget.distance == 0;
            
    //         if (firstNode < 2)
    //             target += new Vector3(1, 1, 0) * (lastNode ? 
    //                     UnityEngine.Random.Range(-2, 3) :
    //                     UnityEngine.Random.Range(-7, 8) * .1f);

    //         float xDist = Mathf.Abs(target.x - transform.position.x);
    //         float yDist = Mathf.Abs(target.y - transform.position.y);
    //         bool horizontal = xDist > yDist;
    //         bool shouldGoDiagonal = firstNode == 2 && (horizontal ? yDist > DiagThreshold : xDist > DiagThreshold);
    //         IEnumerator moveRoutine = MoveToNode(movementAction.path.currentTarget, firstNode < 2, shouldGoDiagonal, firstNode > 0);
    //         yield return StartCoroutine(moveRoutine);
    //         if(movementAction.path.currentTarget.distance == 0){
    //             finishedCallback();
    //             ClearMovementAction();
    //             break;
    //         }
    //         if(!hitWall) movementAction.path.Advance();
    //         hitWall = false;
    //         firstNode = firstNode > 0 ? firstNode - 1 : 0;
    //     }
    // }

    // public IEnumerator WanderInRoom(Room room, float area){
    //     stillWandering = true;
    //     Vector2 roomCenter = room.transform.position;
    //     if(firstWander){
    //         firstWander = false;
    //         yield return new WaitForSeconds(UnityEngine.Random.Range(0, 3f));
    //     }
    //     while(true){
    //         if(!wander)
    //             break;
    //         float randomX = UnityEngine.Random.Range(roomCenter.x - area, roomCenter.x + area);
    //         float randomY = UnityEngine.Random.Range(roomCenter.y - area, roomCenter.y + area);
    //         bool diagRandom = UnityEngine.Random.Range(0, 2) == 1;
    //         PathNode node = new PathNode(room.ID, new Vector2(randomX, randomY));
    //         yield return StartCoroutine(MoveToNode(node, false, diagRandom, true));
    //         if(!wander)
    //             break;
    //         float randomWait = UnityEngine.Random.Range(.01f, 1.5f);
    //         yield return new WaitForSeconds(randomWait);
    //     }
    //     stillWandering = false;
    // }

    public IEnumerator MoveToNode(PathNode node, bool stopShort, bool diagonal, bool noSmooth, Action callback){
        Vector3 target = node.pos;
        float xDist = target.x - transform.position.x;
        float yDist = target.y - transform.position.y;
        int xDir = (int)Mathf.Sign(xDist);
        int yDir = (int)Mathf.Sign(yDist);
        bool horizontalMove = Mathf.Abs(xDist) > Mathf.Abs(yDist);

        if(!noSmooth){
            if(horizontalMove ? Mathf.Abs(YVel) > .15 : Mathf.Abs(XVel) > .15){
                float xSmooth = horizontalMove ? xDir : XVel == 0 ? 0 : Mathf.Sign(XVel);
                float ySmooth = !horizontalMove ? yDir : YVel == 0 ? 0 : Mathf.Sign(YVel);
                int framesNum = UnityEngine.Random.Range(MinSmoothTurnFrames, MaxSmoothTurnFrames);
                for (int frame = 0; frame < framesNum; frame++){
                    Accelerate(new Vector2(xSmooth, ySmooth), AccelSpeed, MaxSpeed);
                    yield return null;
                }
            }
        }

        int xCurrentDir = xDir;
        int yCurrentDir = yDir;

        while(true){
            if(hitWall)
                horizontalMove = !horizontalMove;

            float xTargetVal = target.x;
            float yTargetVal = target.y;
            if(!hitWall && (stopShort || diagonal)){
                float ssa = diagonal ? 1 : UnityEngine.Random.Range(0, StopShortAmount);
                xTargetVal -= ssa * xDir;
                yTargetVal -= ssa * yDir;
            }
            xCurrentDir = (int)Mathf.Sign(xTargetVal - transform.position.x);
            yCurrentDir = (int)Mathf.Sign(yTargetVal - transform.position.y);
            float xMove = horizontalMove ? xDir : 0;
            float yMove = horizontalMove ? 0 : yDir;

            Accelerate((diagonal && !hitWall) ? new Vector2(xDir, yDir) : new Vector2(xMove, yMove), AccelSpeed, MaxSpeed);
            
            if(diagonal){
                if(xCurrentDir != xDir || yCurrentDir != yDir){
                    callback();
                    break;
                }
            } else {
                if(horizontalMove ? xCurrentDir != xDir : yCurrentDir != yDir){
                    callback();
                    break;
                }
            }
            yield return null;
        }
    }

    

    
    // private float directionChangeTime = 0;
    
    protected override void OnWallCollide(Vector2 collPoint){

        // if(movementAction != null){// && Time.time - directionChangeTime > 1f){
            SetVelocity(0,0);
            // directionChangeTime = Time.time;
            hitWall = true;
        // }
        return;
    }

    protected override void OnActorCollide(Vector2 collPoint){
        return;
    }
}

public class MovementAction {
    
    public Path path;
    public CircleGuy player;
    public int endRoom;
    // public bool hitWall = false;

    public MovementAction(Path path, CircleGuy player, int endRoom){
        this.path = path;
        this.player = player;
        this.endRoom = endRoom;
    }

    // public void HitWall(){
    //     hitWall = true;
    // }

    // public void ResetHitWall(){
    //     hitWall = false;
    // }
}
