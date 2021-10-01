using UnityEngine;
using System.Collections;

public enum CameraFollowMode
{
    Regular,
    Death
}

public class CameraController : MonoBehaviour
{
    public static CameraController that { get; private set; }

	public float camHeight = 3.6f;
	public float camFarDist = 6.8f;
    public float camCloseDist = 4.0f;
    public float camDist = 0;

    float cameraSpeed = 7.0f;
    float cameraRotationSpeed = 10.0f;

	// used in death mode
	Vector3 camOffset = Vector3.zero;
	Vector3 deathCamOffset = Vector3.zero;
    CameraFollowMode mode = CameraFollowMode.Regular;

    void Awake()
    {
        that = this;
        camDist = camFarDist;
    }

    void Destroy()
    {
        that = null;
    }

	public void WatchDeath()
	{
        var player = Game.that.player;
        Vector3 core = new Vector3(0.0f, player.transform.position.y, 0.0f);
        Vector3 up = (player.transform.position - core).normalized;

        camOffset = (up * camHeight) + Vector3.down * camDist;
        deathCamOffset = up * camHeight * 2.0f + Vector3.up * 3.0f;
		mode = CameraFollowMode.Death;
	}

    void LateUpdate()
    {
        if(!Game.that.player)
            return;

        var player = Game.that.player;

        Vector3 core = new Vector3(0.0f, player.transform.position.y, 0.0f);
        Vector3 up = (player.transform.position - core).normalized;

		switch(mode)
		{
			case CameraFollowMode.Regular:
                camOffset = Vector3.up * camHeight + Vector3.forward * -camDist;
                transform.position = player.transform.position + (up * camHeight) + (Vector3.down * camDist);
                var lookDir = (player.transform.position + Vector3.up - transform.position).normalized;
                transform.rotation = Quaternion.LookRotation(lookDir, up);
				break;

			case CameraFollowMode.Death:
				camOffset = Vector3.Lerp(camOffset, deathCamOffset, 3.0f * Time.deltaTime);
				transform.position = player.transform.position + camOffset;
				transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(player.transform.position - transform.position), 180.0f * Time.deltaTime);
				break;
		}
    }
}
