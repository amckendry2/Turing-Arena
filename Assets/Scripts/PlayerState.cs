using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class PlayerState
{
    protected Player player;
    public abstract void Tick();
    public virtual void FireDown(){ }
    public virtual void FireUp(){ }
    public virtual void StickAimFire() { }
    public virtual void StickAimDown(Vector2 input){ }
    public virtual void StickAimUp(){ }
    public virtual void StickDirDown(Vector2 input){ }
    public virtual void StickDirUp() { }
    public virtual void DodgeDown(){ }
    public virtual void DodgeUp(){ }
    public virtual void DirDown(Vector2 input){ }
    public virtual void DirUp(){ }
    public virtual void OnStateEnter(){ }
    public virtual void OnStateExit(){ }

    public PlayerState(Player player){
        this.player = player;
    }
}

public class Walking : PlayerState 
{
    public Walking(Player player) : base(player){}

    public override void Tick(){
        player.Accelerate8(player.InputVector, player.WalkSpeed, player.WalkMaxSpeed);
    }

    public override void FireDown(){
        if(player.bullets > 0){
            player.bullets --;
            player.SetState(new Aiming(player));
        }
    }

    public override void StickAimDown(Vector2 input){
        player.SetState(new Aiming(player));
    }

    public override void DodgeDown(){
        if(player.GotDirInput() && player.DodgeCooldown == 0){
            player.SetState(new Dodging(player));
        } else {
            player.SetState(new Running(player));
        }
    }
}

public class Dodging : PlayerState
{
    private int frame = 0;
    private int endFrame;
    private bool dodgeReleased = false;

    public Dodging(Player player) : base(player){}

    public override void OnStateEnter(){
        player.DodgeCooldown = player.DodgeCooldownFrames;
        endFrame = player.DodgeFrames;
        player.SetDrag(player.DodgeDrag);
    }

    public override void Tick(){
        if(frame > endFrame){
            if(dodgeReleased){
                player.SetState(new Walking(player));
            } else {
                player.SetState(new Running(player));
            }
            return;
        } 
        player.Accelerate(player.InputVector, player.DodgeSpeed, player.DodgeMaxSpeed);
        frame++;
    }

    public override void FireDown(){
        player.SetState(new Aiming(player));
    }

    public override void StickAimDown(Vector2 input){
        player.SetState(new Aiming(player));
    }

    public override void DodgeUp(){
        dodgeReleased = true;
    }

    public override void OnStateExit(){
        player.SetDrag(player.DragSpeed);
    }

}

public class Running : PlayerState
{
    public Running(Player player) : base(player){}

    public override void Tick(){
        player.Accelerate(player.InputVector, player.RunSpeed, player.RunMaxSpeed);
    }

    public override void FireDown(){
        player.SetState(new Aiming(player));
    }

    public override void StickAimDown(Vector2 input){
        player.SetState(new Aiming(player));
    }

    public override void DodgeUp(){
        player.SetState(new Walking(player));
    }
}


public class Aiming : PlayerState
{
    private int frame = 1;
    private int animFrames = 0;
    private bool aiming = false;
    public Aiming(Player player) : base(player){}

    public override void OnStateEnter(){
        player.SetDrag(player.AimingDrag);
        player.LaserDirection = player.AimVector;
        if(player.GotAimInput())
            aiming = true;
        animFrames = player.AimFrames;
    }

    public override void DirDown(Vector2 input){
        if(!aiming)
            player.LaserDirection = input;
        aiming = true;
    }

    public override void DirUp(){
        // aiming = false;
        // frame = 1;
    }


    public override void StickAimUp(){
        // player.SetState(new Walking(player));
    }

    public override void FireUp(){
        if(frame > animFrames){
            player.SetState(new Firing(player));
        } else {
            // player.SetState(new Walking(player));
        }
    }

    public override void StickAimFire(){
        if(frame > animFrames)
            player.SetState(new Firing(player));
    }

    public override void DodgeDown(){
        if(player.GotDirInput() && player.DodgeCooldown == 0){
            player.SetState(new Dodging(player));
        } else {
            player.SetState(new Running(player));
        }
    }

    public override void Tick(){
        player.MoveLaser();
        if(aiming){
            if(frame < animFrames - player.RedLaserFrames){
                player.DrawArmingLaser((float)frame/((float)animFrames - player.RedLaserFrames));
            } else if(frame < animFrames){
                player.DrawArmedLaser();
            } else {
                player.SetState(new Firing(player));
            }
            frame++;
        }
    }

    public override void OnStateExit(){
        player.SetDrag(player.DragSpeed);
    }
}


public class Firing : PlayerState
{
    public Firing(Player player) : base(player){}

    private int frame = 1;
    private int recoveryFrames;
    private int gunshotFrames; 
    private Vector2 bulletDirection;
    private Vector3 bulletOrigin;
    bool bulletHit = false;

    public override void OnStateEnter(){
        recoveryFrames = player.RecoveryFrames;
        gunshotFrames = player.GunshotFrames;
        bulletDirection = player.LaserDirection;
        Vector2 bulletOrigin2D = player.GetLaserOrigin(bulletDirection);
        bulletOrigin = new Vector3(bulletOrigin2D.x, bulletOrigin2D.y, -1);
        player.StartGunshot(bulletOrigin);
    }

    public override void Tick(){
        if(!bulletHit){
            LaserCollision hit = player.CheckBulletHit(bulletOrigin, bulletDirection, frame);
            if(hit != null){
                bulletHit = true;
                player.BulletHit(hit.Point, bulletDirection, hit.CollisionType == CollisionType.ACTOR);
                if(hit.CollisionType == CollisionType.ACTOR){
                    Player hitPlayer = hit.Obj.GetComponent<Player>();
                    if(hitPlayer == null){
                        hit.Obj.GetComponent<SpriteRenderer>().color = Color.white;
                        return;
                    }
                    PlayerColor hitColor = hitPlayer.myColor;
                    hitPlayer.EndGunshot();
                    // if(MatchManager.Instance.CheckForKill(player.myColor, hitColor)){
                        // Debug.Log("kill?");
                    hitPlayer.GetComponent<SpriteRenderer>().sprite = hitPlayer.SkullSprite;
                    // hitPlayer.GetComponent<SpriteRenderer>().color = Color.white;
                    // MatchManager.Instance.FreezeGame();
                    // MatchManager.Instance.DisplayWinMessage(player.myColor);
                    // } else {
                    //     hitPlayer.SetState(new Stunned(hitPlayer));
                    // }
                }
            }
        }
        if(frame > gunshotFrames){
            player.EndGunshot();
        }
        if(frame < recoveryFrames){
           frame++; 
        } else {
            player.SetState(new Walking(player));
        }
    }
}

public class Stunned : PlayerState
{
    public Stunned(Player player) : base(player){}

    private int frame = 0;
    private int length = 0;

    public override void OnStateEnter(){
        length = player.ShotStunFrames;
    }

    public override void Tick(){
        if(frame > length)
            player.SetState(new Walking(player));
        frame++;
    }

}

public class Frozen : PlayerState
{
    public Frozen(Player player) : base(player){}

    public override void OnStateEnter(){
        player.InputVector = Vector2.zero;
    }
    public override void Tick(){}
}




