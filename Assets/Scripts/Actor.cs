using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class Actor : MonoBehaviour
{
    [SerializeField]
    private float MoveIncrement = .05f;
    [SerializeField]
    public float ColliderSize = 1.5f;
    [SerializeField]
    protected SolidMap solidMap;
    [SerializeField]
    protected ActorMap actorMap;

    private float xRemainder = 0;
    private float yRemainder = 0;

    protected abstract void OnActorCollide(Vector2 collPoint);
    protected abstract void OnWallCollide(Vector2 collPoint);

    private void Awake(){
        solidMap = FindObjectOfType<SolidMap>();
        actorMap = FindObjectOfType<ActorMap>();
        actorMap.AddActor(this);
    }

    public void MoveX(float amount){
        // float transformX = 0;
        xRemainder += amount;// * Time.deltaTime;
        float move = xRemainder - (xRemainder % MoveIncrement);
        if(move != 0){
            xRemainder -= move;
            // float nudgeX =  Mathf.Sign(move) * MoveIncrement;
            Vector3 nudge = Mathf.Sign(move) * MoveIncrement * Vector3.right;
            while(move != 0){
                // Vector3 newPos = transform.position + Vector3.right*(nudgeX + transformX);
                Vector3 newPos = transform.position + nudge;
                CollisionResult actorCollision = actorMap.CheckCollision(newPos, ColliderSize);
                CollisionResult wallCollision = solidMap.CheckCollision(newPos, ColliderSize);
                if (actorCollision.DidCollide)
                    OnActorCollide(actorCollision.CollisionPoint);
                if (wallCollision.DidCollide)
                    OnWallCollide(wallCollision.CollisionPoint);
                if (actorCollision.DidCollide || wallCollision.DidCollide)
                    break;
                // transformX += nudgeX;
                transform.position += nudge;
                // rigidBody.MovePosition(transform.position + nudge);
                float newMove = move - nudge.x;
                // float newMove = move - nudgeX;
                move = newMove - (float)(newMove % MoveIncrement);
            }
            // return transformX;
        }
        // return 0;
    }

    public void MoveY(float amount){
        yRemainder += amount;// * Time.deltaTime;
        float move = yRemainder - (yRemainder % MoveIncrement);
        if(move != 0){
            yRemainder -= move;
            Vector3 nudge = Mathf.Sign(move) * MoveIncrement * Vector3.up;
            while(move != 0){
                Vector3 newPos = transform.position + nudge;
                CollisionResult actorCollision = actorMap.CheckCollision(newPos, ColliderSize);
                CollisionResult wallCollision = solidMap.CheckCollision(newPos, ColliderSize);
                if (actorCollision.DidCollide)
                    OnActorCollide(actorCollision.CollisionPoint);
                if (wallCollision.DidCollide)
                    OnWallCollide(wallCollision.CollisionPoint);
                if (actorCollision.DidCollide || wallCollision.DidCollide)
                    break;
                transform.position += nudge;
                float newMove = move - nudge.y;
                move = newMove - (float)(newMove % MoveIncrement);
            }
        }
    }
}
