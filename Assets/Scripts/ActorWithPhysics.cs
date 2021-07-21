using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class ActorWithPhysics : Actor
{
    
    public float XVel {get; private set;} = 0;
    public float YVel {get; private set;} = 0;
    private Vector2 internalAccelVector = new Vector2();
    private float internalAccelSpeed = 0;
    private float internalMaxSpeed = 0;
    private float internalDragSpeed = 0;
    private bool gotAccel = false;

    public void Accelerate(Vector2 vector, float speed, float max){
        internalAccelVector = vector;
        internalAccelSpeed = speed;
        internalMaxSpeed = max;
        gotAccel = true;
    }

    public void Accelerate8(Vector2 vector, float speed, float max){
        Vector3 v3 = new Vector3(vector.x, vector.y, 0);
        Vector2 v8 = Utility.SnapTo(v3, 45);
        Accelerate(v8, speed, max);
    }

    public void SetDrag(float speed){
        internalDragSpeed = speed;
    }

    public void SetVelocity(float x, float y){
        XVel = x;
        YVel = y;
    }


    protected virtual void Update(){
        PhysicsApplyDrag(internalDragSpeed);
        if(gotAccel)
            PhysicsAccelerate(internalAccelVector, internalAccelSpeed, internalMaxSpeed);
        PhysicsMove();
        gotAccel = false;
    }

    private void PhysicsMove(){
        MoveX(XVel * MatchManager.Instance.TimeScale);// * Time.deltaTime;
        MoveY(YVel * MatchManager.Instance.TimeScale);
    }

    private void PhysicsApplyDrag(float amount){
        Vector2 dragVec = new Vector2(XVel, YVel).normalized * (amount * Time.deltaTime);
        // Debug.Log(dragVec.x);
        XVel -= dragVec.x;
        YVel -= dragVec.y;
        if(Mathf.Abs(XVel) < .005f)
            XVel = 0;
        if(Mathf.Abs(YVel) < .005f)
            YVel = 0;
    }


    private void PhysicsAccelerate(Vector2 direction, float amount, float maxSpeed){
        Vector2 accelVec = direction.normalized * amount * Time.deltaTime; // <------------
        Vector2 currentVec = new Vector2(XVel, YVel);
        int xDir = (int)Mathf.Sign(currentVec.x);
        int yDir = (int)Mathf.Sign(currentVec.y);
        XVel += accelVec.x;
        YVel += accelVec.y;
        float speed = new Vector2(XVel, YVel).sqrMagnitude;
        float max = maxSpeed * maxSpeed;
        if(speed > max) {
            float limiter = max / speed;
            XVel *= limiter;
            YVel *= limiter;
        }
    }

    
}
