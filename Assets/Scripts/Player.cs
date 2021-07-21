using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Player : ActorWithPhysics
{
    [SerializeField]
    public Sprite SkullSprite;

    //Color
    public PlayerColor myColor;
    [SerializeField]
    private bool red;
    [SerializeField]
    private bool blue;
    [SerializeField]
    private bool green;
    [SerializeField]
    private bool yellow;

    //Movement
    [SerializeField]
    public float WalkSpeed = .02f;
    [SerializeField]
    public float DragSpeed = .01f;
    [SerializeField]
    public float DodgeDrag = .005f;
    [SerializeField]
    public float WalkMaxSpeed = .2f;
    [SerializeField]
    public int DodgeFrames = 30;
    [SerializeField]
    public float DodgeSpeed = .1f;
    [SerializeField]
    public float DodgeMaxSpeed = .6f;
    [SerializeField]
    public float RunSpeed = .04f;
    [SerializeField]
    public float RunMaxSpeed = .3f;
    [SerializeField]
    public float AimingDrag = .02f;
    [SerializeField]
    public int DodgeCooldownFrames = 60;
    [SerializeField]
    public int ShotStunFrames = 120;

    public int DodgeCooldown = 0;

    public Vector2 InputVector = new Vector2();
    public Vector2 AimVector = new Vector2();
    public bool GotDirInput(){return InputVector.x != 0 || InputVector.y != 0;}
    public bool GotAimInput(){return AimVector.x != 0 || AimVector.y != 0;}

    //Aiming Laser
    [SerializeField]
    private float LaserRaycastResolution = .25f;
    [SerializeField]
    private int LaserRaycastDistance = 1000;
    [SerializeField]
    private Material LaserMaterial;
    [SerializeField]
    float LaserWidth = 4;
    [SerializeField]
    public int AimFrames = 20;
    [SerializeField]
    public float LaserAnimWidth = 10f;
    [SerializeField]
    public float LaserTurnSmoothing = 3f;
    [SerializeField]
    public Color AimingColor = Color.yellow;
    [SerializeField]
    public Color ArmedColor = Color.red;
    [SerializeField]
    public Color BulletColor = Color.white;
    [SerializeField]
    public int RedLaserFrames = 30;

    public Vector2 LaserDirection = new Vector2(0, 1);    
    public Vector2 LaserDirectionL = new Vector2(0, 1);
    public Vector2 LaserDirectionR = new Vector2(0, 1);

    //Gunshot
    [SerializeField]
    public int GunshotFrames = 10;
    [SerializeField]
    public float BulletSpeed = 1f;
    [SerializeField]
    public int RecoveryFrames = 30;

    public GameObject gunshotObject;
    private SpriteRenderer gunshotSprite;
    public GameObject bulletsparkObj;
    public GameObject bloodObj;
    private ParticleSystemRenderer bulletSparkParticles;
    private ParticleSystemRenderer bloodParticles;

    public ParticleSystemRenderer currentParticles;

    //State
    private PlayerInput playerInput;
    public PlayerState currentState;
   
    [SerializeField]
    public int bullets = 0;
    public bool InRoom = false;
    public Room CurrentRoom = null;

    private List<Line> linesToRender = new List<Line>();

    private InputActions inputActions;

    [SerializeField]
    public bool testing;

    void Start(){
        if(testing) TestInput();
        inputActions = new InputActions();
        bulletSparkParticles = bulletsparkObj.GetComponent<ParticleSystemRenderer>();
        bloodParticles = bloodObj.GetComponent<ParticleSystemRenderer>();
        gunshotSprite = gunshotObject.GetComponent<SpriteRenderer>();
        SetState(new Walking(this));
        SetDrag(DragSpeed);
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

    public void TestInput(){
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if(red){
            myColor = PlayerColor.RED;
            sr.color = Color.red;
        }
        if(blue){
            myColor = PlayerColor.BLUE;
            sr.color = Color.blue;
        }
        if(green){
            myColor = PlayerColor.GREEN;
            sr.color = Color.green;
        }
        if(yellow){
            myColor = PlayerColor.YELLOW;
            sr.color = Color.yellow;
        }
        GetComponent<PlayerInput>().onActionTriggered += HandleInputEvent;
        MatchManager.Instance.RegisterTestingPlayer(this);
    }

    public void SetColor(PlayerColor color){
        myColor = color;
    }

    public void HandleInputEvent(InputAction.CallbackContext callback){
        // if(callback.action.name == inputActions.Player.ResetGame.name){
        //     Destroy(PlayerManager.Instance.gameObject);
        //     SceneManager.LoadScene("Character Select");
        // }
        if(callback.action.name == inputActions.Player.ResetGame.name){
            // Destroy(PlayerManager.Instance.gameObject);
            SceneManager.LoadScene("9rooms");
        }
        if(callback.action.name == inputActions.Player.Quit.name){
            Application.Quit();
        }
        if(currentState is Frozen) return;   
        if(callback.action.name == inputActions.Player.StickAim.name)
            OnStickAim(callback.ReadValue<Vector2>());
        if(callback.action.name == inputActions.Player.StickAimFire.name)
            OnStickAimFire();
        if(callback.action.name == inputActions.Player.StickMove.name)
            OnStickMove(callback.ReadValue<Vector2>());
        if(callback.action.name == inputActions.Player.Move.name)
            OnMove(callback.ReadValue<Vector2>());
        if(callback.action.name == inputActions.Player.Fire.name)
            OnFire(callback.ReadValue<float>());
        if(callback.action.name == inputActions.Player.Interact.name)
            OnInteract();
        if(callback.action.name == inputActions.Player.Dodge.name)
            OnDodge(callback.ReadValue<float>());          
    }

    protected override void Update(){
        base.Update();
        UpdateRoom();
        linesToRender.Clear();
        if(DodgeCooldown > 0 ) DodgeCooldown --;
        currentState.Tick();
    }

    public void OnMove(Vector2 input){
        // Vector2 input = value.Get<Vector2>();
        AimVector = input;
        InputVector = input;
        if(input.x != 0 || input.y != 0){
            currentState.DirDown(input);
        } else {
            currentState.DirUp();
        }
    }

    public void OnStickMove(Vector2 input){
        InputVector = input;
        if(input.x != 0 || input.y != 0){
            currentState.StickDirDown(input);
        } else {
            currentState.StickDirUp();
        }
    }

    public void OnFire(float value){
        if(value > 0){
            currentState.FireDown();
        } else {
            currentState.FireUp();
        }
    }

    public void OnDodge(float value){
        if(value > 0){
            currentState.DodgeDown();
        } else {
            currentState.DodgeUp();
        }
    }

    public void OnStickAim(Vector2 value){
        AimVector = value;
        if(value.x != 0 || value.y != 0){
            currentState.StickAimDown(value);
        } else {
            currentState.StickAimUp();
        }
    }

    public void OnStickAimFire(){
        currentState.StickAimFire();
    }

    public void OnInteract(){
        if(InRoom && CurrentRoom.hasBullet){
            CurrentRoom.TakeBullet();
            bullets += 1;
        }
    }

    protected override void OnWallCollide(Vector2 collPoint){

    }

    protected override void OnActorCollide(Vector2 collPoint){
        
    }

    public void SetState(PlayerState state){
        if(currentState != null)
            currentState.OnStateExit();

        currentState = state;

        gameObject.name = "NewPlayer - " + state.GetType().Name;

        if(currentState != null)
            currentState.OnStateEnter();
    }

    public void MoveLaser(){
        float aimDiff = Vector2.SignedAngle(LaserDirection, AimVector);
        aimDiff = Mathf.Abs(aimDiff) > 180f ? -(360 - aimDiff) : aimDiff;
        float turnAmount = aimDiff / LaserTurnSmoothing;
        LaserDirection = Quaternion.AngleAxis(turnAmount, Vector3.forward) * LaserDirection;
    }

    public void DrawArmingLaser(float framePercentage){
        float sideAngle = LaserAnimWidth - LaserAnimWidth * framePercentage;
        LaserDirectionL = Quaternion.AngleAxis(-sideAngle, Vector3.forward) * LaserDirection;
        LaserDirectionR = Quaternion.AngleAxis(sideAngle, Vector3.forward) * LaserDirection;
        Vector2 laserOriginL = GetLaserOrigin(LaserDirectionL);
        Vector2 laserEndL = GetLaserCollision(transform.position, LaserDirectionL).Point;
        Vector2 laserOriginR = GetLaserOrigin(LaserDirectionR);
        Vector2 laserEndR = GetLaserCollision(transform.position, LaserDirectionR).Point;
        Vector2 laserOrigin = GetLaserOrigin(LaserDirection);
        Vector2 laserEnd = GetLaserCollision(transform.position, LaserDirection).Point;
        DrawLaser(laserOriginL, laserEndL, AimingColor);
        DrawLaser(laserOriginR, laserEndR, AimingColor);
        DrawLaser(laserOrigin, laserEnd, AimingColor);
    }

    public void DrawArmedLaser(){
        Vector2 laserOrigin = GetLaserOrigin(LaserDirection);
        Vector2 laserEnd = GetLaserCollision(transform.position, LaserDirection).Point;
        DrawLaser(laserOrigin, laserEnd, ArmedColor);
    }

    public void StartGunshot(Vector3 origin){
        gunshotObject.transform.position = origin;
        Vector3 rotationVector = new Vector3(0, 0, Vector2.SignedAngle(Vector2.left, LaserDirection));
        gunshotObject.transform.localEulerAngles = rotationVector;
        gunshotSprite.enabled = true;
    }

    public LaserCollision CheckBulletHit(Vector2 origin, Vector2 direction, int frame){
        LaserCollision laserColl = GetLaserCollision(origin, direction);
        float distance = (laserColl.Point - origin).magnitude;
        if(distance < frame * BulletSpeed){
            return laserColl;
        } else {
            return null;
        }
    }

    public void BulletHit(Vector2 hitLocation, Vector2 direction, bool player){
        float angle = Vector2.SignedAngle(Vector2.left, direction);
        ParticleSystemRenderer particles = player ? bloodParticles : bulletSparkParticles;
        currentParticles = particles;
        GameObject.Instantiate(particles, hitLocation, Quaternion.Euler(-angle, 90, 90));
    }

    public void EndGunshot(){
        gunshotSprite.enabled = false;
    }

    public Vector2 GetLaserOrigin(Vector3 direction){
        return transform.position + direction.normalized * ColliderSize / 2;
    }

    public LaserCollision GetLaserCollision(Vector3 origin, Vector3 direction){
        direction = direction.normalized;
        for(int i = 0; i < LaserRaycastDistance; i++){
            Vector3 checkPoint = origin + (direction * i * LaserRaycastResolution);
            ActorCollision actorCol = actorMap.CheckPointCollision(checkPoint, (Actor) this);
            if(actorCol.DidCollide){
                return new LaserCollision(CollisionType.ACTOR, actorCol.CollisionPos, actorCol.CollidedObj);
            }
            CollisionResult solidCol = solidMap.CheckPointCollision(checkPoint, origin, LaserRaycastResolution);
            if(solidCol.DidCollide){
                return new LaserCollision(CollisionType.SOLID, solidCol.CollisionPoint, null);
            }
        }
        return null;
    }

    public void DrawLaser(Vector2 origin, Vector2 end, Color color){    
        linesToRender.Add(new Line(origin, end, color));
    }

    public void OnRenderObject(){
        if(linesToRender.Count > 0){
            foreach(Line line in linesToRender){
                RenderLine(line.origin, line.end, line.color);
            }
        }
    }    


    // private void RenderLine(Vector3 origin, Vector3 end, Color color){
    //     //should add in laser material
    //     GL.PushMatrix();
    //     GL.Begin(GL.LINES);
    //     LaserMaterial.SetPass(0);
    //     GL.Color(color);
    //     GL.Vertex(origin);
    //     GL.Vertex(end);
    //     GL.End();
    //     GL.PopMatrix();
    // }
    private void RenderLine(Vector3 origin, Vector3 end, Color color){
        //should add in laser material
        GL.PushMatrix();
        GL.Begin(GL.QUADS);
        LaserMaterial.SetPass(0);
        GL.Color(color);
        Vector3 angleVector = (end - origin);
        // float angle = 90 + Vector2.SignedAngle(origin, end);
        float angle =  -Mathf.Atan2(angleVector.x, angleVector.y);// * Mathf.Rad2Deg;
        // if(angle < 0) angle = angle + 360;
        float Xoffset = Mathf.Cos(angle) * LaserWidth;
        float Yoffset = Mathf.Sin(angle) * LaserWidth;
        Vector3 offset = new Vector3(Xoffset, Yoffset, 0);
        Vector3 origin1 = origin + offset;
        Vector3 origin2 = origin - offset;
        Vector3 end1 = end + offset;
        Vector3 end2 = end - offset;
        GL.Vertex(origin1);
        GL.Vertex(origin2);
        GL.Vertex(end2);
        GL.Vertex(end1);
        GL.End();
        GL.PopMatrix();
    }

    struct Line
    {
        public readonly Vector3 origin;
        public readonly Vector3 end;
        public readonly Color color;

        public Line(Vector3 origin, Vector3 end, Color color){
            this.origin = origin;
            this.end = end;
            this.color = color;
        }
    }
}

public enum CollisionType{
    ACTOR, SOLID
}

public class LaserCollision {
    public readonly CollisionType CollisionType;
    public readonly Vector2 Point;
    public readonly GameObject Obj;

    public LaserCollision(CollisionType CollisionType, Vector2 Point, GameObject obj){
        this.CollisionType = CollisionType;
        this.Point = Point;
        this.Obj = obj;
    }
}
