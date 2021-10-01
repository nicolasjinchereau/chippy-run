using UnityEngine;
using System.Collections;

public class Snake : MonoBehaviour
{
    public static Snake that { get; private set; }

	public Animation anim;
    public float snakeMinOffset = -5.75f;
    public float snakeMaxOffset = -1.0f;
    public float snakeBiteOffset = -0.5f;

    public bool isBiting {
        get { return biting; }
    }

    bool biting;
    bool hiding;
    float advance;
    float biteAdvance;
    Coroutine biteRoutine;

    void Awake() {
        that = this;
    }

    private void OnDestroy() {
        that = null;
    }

    void Update()
    {
        var player = Game.that.player;

        if(Game.that.paused || !player || biting || hiding || !Game.that.gameRunning)
            return;
        
        float chippySpeed = Vector3.Project(Game.that.player.body.velocity, Vector3.up).magnitude;

        // walk speed = 8
        // run speed = 18
        if(chippySpeed < 12.0f)
        {
            float minAdvanceTime = 0.7f;
            float adv = 1.0f - Util.InverseLerp(chippySpeed, 4.0f, 12.0f);
            advance = Mathf.Min(advance + Time.deltaTime / minAdvanceTime * adv, 1.0f);
        }
        else
        {
            float receedTime = 3.0f;
            advance = Mathf.Max(advance - Time.deltaTime / receedTime, 0);
        }
		
        if(advance >= 0.99f)
        {
            biteRoutine = StartCoroutine(DoBite());
        }
        else
        {
            float snakeOffset = Mathf.Lerp(snakeMinOffset, snakeMaxOffset, advance);
            Vector3 pos = player.transform.position + Vector3.up * snakeOffset;
            Vector3 up = new Vector3(pos.x, 0, pos.z).normalized;

            transform.position = pos;
            transform.rotation = Quaternion.LookRotation(Vector3.up, up);   
        }
    }

    public void Hide()
    {
        if(biteRoutine != null)
            StopCoroutine(biteRoutine);
        
        anim.CrossFade("slither", 0.2f);

        StartCoroutine(DoHide());
    }

    IEnumerator DoHide()
    {
        hiding = true;

        float length = 0.5f;
        float start = Time.time;
        float end = start + length;

        Vector3 startPos = transform.position;

        while(Time.time <= end)
        {
            float t = (Time.time - start) / length;

            Vector3 pos = startPos - new Vector3(0, t * 10.0f, 0);
            Vector3 up = new Vector3(pos.x, 0, pos.z).normalized;

            transform.position = pos;
            transform.rotation = Quaternion.LookRotation(Vector3.up, up);  

            yield return null;
        }

        gameObject.SetActive(false);
    }

    IEnumerator DoBite()
    {
        biting = true;
        var player = Game.that.player;

        anim.CrossFade("chomp");

        float camFarDist = CameraController.that.camFarDist;
        float camCloseDist = CameraController.that.camCloseDist;

        float length = 1.0f;
        float start = Time.time;
        float end = start + length;

        while(Time.time <= end)
        {
            float t = (Time.time - start) / length;
            float camT = Util.InverseLerp(t, 0.0f, length * 0.6f);
            float chipT = Util.InverseLerp(t, length * 0.4f, length);

            CameraController.that.camDist = Mathf.Lerp(camFarDist, camCloseDist, camT);
            //float adv = Util.InverseLerp(t, 0.5f, 0.9f);

            float snakeOffset = Mathf.Lerp(snakeMaxOffset, snakeBiteOffset, chipT * chipT);

            Vector3 pos = player.transform.position + Vector3.up * snakeOffset;
            Vector3 up = new Vector3(pos.x, 0, pos.z).normalized;
            transform.position = pos;
            transform.rotation = Quaternion.LookRotation(Vector3.up, up);

            yield return null;
        }
    }
}
