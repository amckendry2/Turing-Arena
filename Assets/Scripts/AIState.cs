using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIState 
{
    protected AIController aiController;

    protected int freezeFrame = 0;
    protected int freezeTime;

    public AIState(AIController aiController){
        this.aiController = aiController;
        this.freezeTime = aiController.TemporaryFreezeFrames;
    }

    public virtual void FrozenTick(){
        freezeFrame++;
        if(freezeFrame > freezeTime){
            aiController.frozen = false;
        }
    }
    
    public abstract void Tick();

    public virtual void OnStateEnter(){}
    public virtual void OnStateExit(){}

    public virtual void TemporaryFreeze(){
        aiController.frozen = true;   
    }


}

public class ShufflePaths : AIState
{   
    private float lastPath = Time.time;
    private float startTime = Time.time;

    public ShufflePaths(AIController aiController) : base(aiController){}

    public override void Tick(){
        if(aiController.frozen){
            FrozenTick();
            return;
        }
        bool timeForPath = Time.time - lastPath > 1 &&
            (Mathf.Floor(Time.time - lastPath) % 6 == 0 || 
            Random.Range(0, 3600) == 0);
        if(timeForPath && aiController.aiPositions.Keys.Count > 0){
            lastPath = Time.time;
            aiController.StartRandomChange();
        }
        if(Time.time - startTime > 30 && Random.Range(0, 7200) == 0)
            aiController.SetState(new MoveAllToRoom(aiController));
    }
}

public class Freeze : AIState
{
    public Freeze(AIController aiController) : base(aiController){}

    public override void Tick(){}

}


public class MoveAllToRoom : AIState
{
    private int targetRoom;

    public MoveAllToRoom(AIController aiController) : base(aiController){}

    public override void OnStateEnter(){
        targetRoom = Random.Range(0, 9);
        aiController.QueueMoveAllToRoom(targetRoom);
    }

    public override void Tick(){
        if(aiController.frozen){
            FrozenTick();
            return;
        }
        if(aiController.AllInSameRoom()){
            aiController.Exodus(targetRoom);
            aiController.SetState(new ShufflePaths(aiController));
        }
    }
}