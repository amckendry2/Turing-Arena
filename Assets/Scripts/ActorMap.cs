using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorMap : MonoBehaviour
{

    private List<Actor> actorList = new List<Actor>();
    private Dictionary<Actor, Vector2> actorPositions = new Dictionary<Actor, Vector2>();

    // public void AddActor(Actor actor, Vector2 pos){
    //     actorPositions.Add(actor, pos);
    // }

    public static ActorMap Instance {get; private set;}

    void Awake(){
        if(Instance != null){
            Destroy(gameObject);
        } else {
            Instance = this;
        }
    }

    public void AddActor(Actor actor){
        actorList.Add(actor);
    }

    // public void UpdateActor(Actor actor, Vector2 pos){
    //     actorPositions[actor] = pos;
    // }

    public CollisionResult CheckCollision(Vector2 position, float colliderSize){
        return new CollisionResult(false);
    }

    public ActorCollision CheckPointCollision(Vector2 point, Actor self){
        foreach(Actor actor in actorList){
            if(actor == self) continue;
            // Actor actorScript = actor.GetComponent<Actor>();
            Vector2 actorPos = actor.transform.position;
            float distanceSq = (point - actorPos).sqrMagnitude;
            float coll = actor.ColliderSize / 2;
            if(distanceSq < coll * coll){
                return new ActorCollision(actor.gameObject, point);
            }
        }
        return new ActorCollision(false);
    }
}

public class ActorCollision{
    public readonly bool DidCollide = true;
    public readonly Vector2 CollisionPos;
    public readonly GameObject CollidedObj;
    public ActorCollision(bool didCollide){
        this.DidCollide = didCollide;
        CollisionPos = Vector2.zero;
        CollidedObj = null;
    }
    public ActorCollision(GameObject obj, Vector2 pos){
        this.DidCollide = true;
        this.CollidedObj = obj;
        this.CollisionPos = pos;
    }
}
