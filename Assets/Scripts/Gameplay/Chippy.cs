using UnityEngine;
using System.Collections;

public class Chippy : MonoBehaviour
{
	public AudioSource bushRustleSource;

	public float speed = 0.0f;
	public bool paused = false;
	public bool inShrubs = false;
    public bool inHole = false;
	public bool onRamp = false;
	public bool onStump = true;
	public bool collidingBranch = false;
	public bool collidingShroom = false;
	public bool crashing = false;

    public Animation anim;
    public Rigidbody body;

    [HideInInspector] public StateMachine stateMachine;
    public bool allowJump = true;
    public bool tutorialMode = false;
    public int jumpCount = 0;

	void Awake()
	{
        anim = GetComponent<Animation>();
        body = GetComponent<Rigidbody>();

		Physics.IgnoreLayerCollision(Layer.Player, Layer.Stumps);
		Physics.IgnoreLayerCollision(Layer.Player, Layer.Ramps);

		anim["w_walk"].speed = 4.0f;
		anim["w_run"].speed = 4.0f;
		anim["w_jump"].speed = 2.0f;
		anim["w_fall"].speed = 0.75f;
		anim.Play("w_idle");

		speed = 0.0f;

		stateMachine = new StateMachine(new IdleState(this));
	}

	void Update() {
		stateMachine.Update();
	}
	
    void OnEnable() {
        Game.OnPauseChanged += OnPause;
    }

    void OnDisable() {
        Game.OnPauseChanged -= OnPause;
    }

    void OnPause(bool didPause, float duration) {
        paused = didPause;
    }

	void OnCollisionEnter(Collision collision)
	{
		switch (collision.gameObject.layer)
		{
		case Layer.Ramps:
			onRamp = true;
			break;

		case Layer.Stumps:
			onStump = true;
			break;

		case Layer.Branches:
			collidingBranch = true;
			break;

		case Layer.Mushroom:
			collidingShroom = true;
			break;
		}
	}

	void OnCollisionExit(Collision collision)
	{
		switch (collision.gameObject.layer)
		{
		case Layer.Ramps:
			onRamp = false;
			break;

		case Layer.Stumps:
			onStump = false;
			break;

		case Layer.Branches:
			collidingBranch = false;
			break;

		case Layer.Mushroom:
			collidingShroom = false;
			break;
		}
	}

	void OnTriggerEnter(Collider other)
	{
		switch(other.gameObject.layer)
		{
		case Layer.Shrubs:
			inShrubs = true;
			break;

        case Layer.Hole:
            inHole = true;
            break;
		}
	}

	void OnTriggerExit(Collider other)
	{
		switch (other.gameObject.layer)
		{
		case Layer.Shrubs:
			inShrubs = false;
			break;

        case Layer.Hole:
            inHole = false;
            break;
		}
	}

	void OnCrashAnimationFinished() {
		crashing = false;
	}

	void OnFallAnimationFinished() {
		
	}

	public float GetRollInput()
	{
		float deltaRollAngle = 0.0f;

		if (!Game.that.ignoreInput)
        {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                deltaRollAngle += 90.0f * Time.deltaTime;
            
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                deltaRollAngle -= 90.0f * Time.deltaTime;
            
#elif UNITY_ANDROID || UNITY_IOS
            if(Accelerometer.value.x < -0.05f || Accelerometer.value.x > 0.05f)
                deltaRollAngle = -Accelerometer.value.x * 180.0f * Time.deltaTime;
#endif
		}

		return deltaRollAngle;
	}

    public bool RaycastTrunk(Vector3 pos, out RaycastHit hit)
    {
        Vector3 core = new Vector3(0.0f, pos.y, 0.0f);
        Vector3 up = (pos - core);
        Vector3 upNormalized = up.normalized;

        return Physics.Raycast(
            new Ray(core + upNormalized * 20.0f, -upNormalized),
            out hit, 20.0f, 1 << Layer.Stumps | 1 << Layer.Ramps);
    }
}

class IdleState : StateObject
{
    Chippy chippy;

    public IdleState(Chippy chippy) {
        this.chippy = chippy;
    }

    public override void Activate() {
        chippy.anim.Rewind("w_idle");
        chippy.anim.CrossFade("w_idle", 0.2f);
    }

    public override void Update()
    {
        if(Game.that.gameRunning || Game.that.tutorialRunning && !chippy.paused)
        {
            SetState(new RunState(chippy));
        }
    }

    public override void Deactivate(){}
}

class DinnerState : StateObject
{
    Chippy chippy;
    float dinnerTime;
    bool wasGobbled;

    public DinnerState(Chippy chippy) {
        this.chippy = chippy;
    }

    public override void Activate() {
        dinnerTime = Time.time + 0.8f;
        chippy.anim.CrossFade("w_walk", 0.2f);
    }

    public override void Update()
    {
        if(!wasGobbled && Time.time >= dinnerTime)
        {
            wasGobbled = true;
            chippy.anim.Rewind("w_fall");
            chippy.anim.CrossFade("w_fall", 0.2f);
        }
    }

    public override void Deactivate(){}
}

class RunState : StateObject
{
    Chippy chippy;

    public RunState(Chippy chippy) {
        this.chippy = chippy;
    }

    float crashThreshold = 17.0f;
    float walkSpeed = 8.0f;
    float runSpeed = 18.0f;
    float gravity = 50.0f;
    float targetSpeed = 0.0f;

    public override void Activate(){}

    bool crashing {
        get { return chippy.collidingBranch && chippy.speed > crashThreshold; }
    }

    public override void OnPointerDown(UnityEngine.EventSystems.PointerEventData data)
    {
        if(chippy.allowJump && !crashing && !Game.that.ignoreInput && !(Game.that.snake && Game.that.snake.isBiting)) {
            chippy.jumpCount++;
            SetState(new JumpState(chippy));
        }
    }

    public override void Update()
    {
        if(crashing)
        {
            SetState(new CrashState(chippy));
            return;
        }

        if(Game.that.snake && Game.that.snake.isBiting)
        {
            Game.that.KillPlayer("snake");
            SetState(new DinnerState(chippy));
            chippy.body.velocity = Vector3.zero;
            return;
        }

        if(chippy.allowJump && Input.GetKeyDown(KeyCode.Space))
        {
            chippy.jumpCount++;
            SetState(new JumpState(chippy));
            return;
        }

        if(chippy.inShrubs)
        {
            if (!chippy.bushRustleSource.isPlaying)
                chippy.bushRustleSource.Play();
        }
        else
        {
            if(chippy.bushRustleSource.isPlaying)
                chippy.bushRustleSource.Stop();
        }

        if (chippy.inShrubs || chippy.inHole || chippy.collidingBranch || chippy.collidingShroom)
        {
            targetSpeed = walkSpeed;

            if (!chippy.anim.IsPlaying("w_walk"))
            {
                chippy.anim.Rewind("w_walk");
                chippy.anim.CrossFade("w_walk", 0.2f);
            }
        }
        else
        {
            targetSpeed = runSpeed;

            if (!chippy.anim.IsPlaying("w_run"))
            {
                chippy.anim.Rewind("w_run");
                chippy.anim.CrossFade("w_run", 0.2f);
            }
        }

        chippy.speed = Mathf.Lerp(chippy.speed, targetSpeed, Time.deltaTime * 30.0f);

        float rollInput = chippy.GetRollInput();

        Vector3 pos = chippy.body.position;
        pos.y += (chippy.speed * Time.deltaTime);
        pos = Quaternion.AngleAxis(rollInput, Vector3.up) * pos;

        Vector3 core = new Vector3(0.0f, pos.y, 0.0f);
        Vector3 up = (pos - core).normalized;

        RaycastHit hit;
        if(!chippy.RaycastTrunk(pos, out hit))
            return;

        pos = Vector3.Lerp(pos, hit.point, 30.0f * Time.deltaTime);

        chippy.body.velocity = (pos - chippy.body.position).normalized * chippy.speed;

        var rot = Quaternion.LookRotation(Vector3.up, up);
        chippy.body.rotation = rot;
        chippy.transform.rotation = rot;
    }

    public override void Deactivate()
    {
        if(chippy.bushRustleSource.isPlaying)
            chippy.bushRustleSource.Stop();
    }
}

class JumpState : StateObject
{
    Chippy chippy;

    public JumpState(Chippy chippy) {
        this.chippy = chippy;
    }

    bool crashing {
        get { return chippy.collidingBranch && chippy.speed > FallThreshold; }
    }

    public const float FallThreshold = 17.0f;
    public const float MaxAirSpeed = 18.0f;
    public const float MaxJumpSpeed = 20.0f;
    public const float gravity = 50.0f;

    float takeoffSpeed = 0.0f;

    float targetSpeed = 0.0f;
    float jumpSpeed = 0.0f;
    float jumpHeight = 0.0f;

    public override void Activate()
    {
        SharedSounds.takeoff.Play();

        chippy.anim.Rewind("w_jump");
        chippy.anim.CrossFade("w_jump", 0.2f);

        jumpSpeed = MaxJumpSpeed;
        takeoffSpeed = chippy.speed;
    }

    public override void Update()
    {
        if(crashing)
        {
            if(chippy.tutorialMode)
            {
                GameUI.DoDamageAnimation();
                SetState(new CrashState(chippy));
            }
            else
            {
                SetState(new FallState(chippy));
            }
            
            return;
        }

        jumpSpeed -= gravity * Time.deltaTime;
        targetSpeed = Mathf.Sqrt((takeoffSpeed * takeoffSpeed) + (jumpSpeed * jumpSpeed));
        chippy.speed = Mathf.Lerp(chippy.speed, targetSpeed, Time.deltaTime * 70.0f);

        float rollInput = chippy.GetRollInput();

        Vector3 pos = chippy.body.position;
        pos.y += (chippy.speed * Time.deltaTime);
        pos = Quaternion.AngleAxis(rollInput, Vector3.up) * pos;

        Vector3 core = new Vector3(0.0f, pos.y, 0.0f);
        Vector3 up = (pos - core).normalized;

        pos += up * jumpSpeed * Time.deltaTime;

        // if on the way down
        if(jumpSpeed < 0.0f)
        {
            RaycastHit hit;
            if(!chippy.RaycastTrunk(pos, out hit))
                return;
            
            // if jump has finished
            if ((pos - core).sqrMagnitude <= (hit.point - core).sqrMagnitude)
            {
                chippy.body.velocity = (hit.point - chippy.body.position).normalized * chippy.speed;
                SetState(new RunState(chippy));
                SharedSounds.landing.Play();
                return;
            }
        }

        chippy.body.velocity = (pos - chippy.body.position).normalized * chippy.speed;

        var rot = Quaternion.LookRotation(Vector3.up, up);
        chippy.body.rotation = rot;
        chippy.transform.rotation = rot;
    }

    public override void Deactivate() {
        jumpSpeed = 0.0f;
    }
}

class CrashState : StateObject
{
    Chippy chippy;

    public CrashState(Chippy chippy) {
        this.chippy = chippy;
    }

    float reboundSpeed = -8.0f;
    float maxRunSpeed = 18.0f;
    float targetSpeed = 0.0f;

    public override void Activate()
    {
        chippy.anim.Rewind("w_crash");
        chippy.anim.CrossFade("w_crash", 0.2f);
        targetSpeed = reboundSpeed;
        chippy.crashing = true;
        SharedSounds.treeHit.Play();
    }

    public override void Update()
    {
        if(!chippy.crashing)
        {
            SetState(new RunState(chippy));
            return;
        }

        if(Game.that.snake && Game.that.snake.isBiting)
        {
            Game.that.KillPlayer("snake");
            SetState(new DinnerState(chippy));
            chippy.body.velocity = Vector3.zero;
            return;
        }

        chippy.speed = Mathf.Lerp(chippy.speed, targetSpeed, Time.deltaTime * 30.0f);

        float rollInput = chippy.GetRollInput();

        Vector3 pos = chippy.body.position;
        pos.y += (chippy.speed * Time.deltaTime);
        pos = Quaternion.AngleAxis(rollInput, Vector3.up) * pos;

        // find height above tree
        Vector3 core = new Vector3(0.0f, pos.y, 0.0f);
        Vector3 up = (pos - core).normalized;

        RaycastHit hit;
        if(!chippy.RaycastTrunk(pos, out hit))
            return;
        
        pos = Vector3.Lerp(pos, hit.point, 30.0f * Time.deltaTime);

        chippy.body.velocity = (pos - chippy.body.position).normalized * Mathf.Abs(chippy.speed);

        var rot = Quaternion.LookRotation(Vector3.up, up);
        chippy.body.rotation = rot;
        chippy.transform.rotation = rot;
    }

    public override void Deactivate(){}
}

class FallState : StateObject
{
    Chippy chippy;

    public FallState(Chippy chippy) {
        this.chippy = chippy;
    }

    float fallSpeed = 10.0f;

    public override void Activate()
    {
        chippy.anim.Rewind("w_fall");
        chippy.anim.CrossFade("w_fall", 0.2f);

        SharedSounds.treeHit.Play();
        Game.that.KillPlayer("fall");
        CameraController.that.WatchDeath();
        Snake.that.Hide();
    }

    public override void Update()
    {
        Vector3 pos = chippy.body.position;
        pos.y -= (fallSpeed * Time.deltaTime);

        // find height above tree
        Vector3 core = new Vector3(0.0f, pos.y, 0.0f);
        Vector3 up = (pos - core).normalized;

        pos += up * 2.0f * Time.deltaTime;

        chippy.body.velocity = (pos - chippy.body.position).normalized * fallSpeed;
        var rot = Quaternion.RotateTowards(chippy.body.rotation, Quaternion.LookRotation(up, Vector3.down), 90.0f * Time.deltaTime);
        chippy.body.rotation = rot;
        chippy.transform.rotation = rot;
    }

    public override void Deactivate(){}
}
