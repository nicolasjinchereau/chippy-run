using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Layer
{
    public const int Stumps = 8;
    public const int Branches = 9;
    public const int Player = 10;
    public const int Shrubs = 18;
    public const int Mushroom = 13;
    public const int Ramps = 14;
    public const int Snake = 16;
    public const int Hole = 17;
};

public class Game : MonoBehaviour
{
    public static Game that { get; private set; }

    public const float DeathLength = 2.5f;
    public const float StumpLength = 20.0f;
    public const float FirstStumpY = -StumpLength * 500;
    public const float MedallionRespawnDelay = 2.0f;
    public const float MedallionSpawnDist = 30.0f;
    public const int MaxNumStumps = 20;
    public const int MinBeaverStumpInterval = 2;
    public const int MaxBeaverStumpInterval = 10;

    public GameObject emptyStump;
    public List<GameObject> stumpPrefabs = new List<GameObject>();
    public GameObject beaverStumpPrefab;
    public GameObject playerPrefab;
    public GameObject snakePrefab;
    public MedallionPiece[] medallionPrefabs;
    public ObjectPool medallionPool;
    public ObjectPool eggPool;
    public CameraController mainCamera;
    public Camera snakeCamera;
    public Material cloudySkybox;
    public Material treeSkybox;
    public AudioSource drumSource;
    public LifeView lifeView;

    // prefabs for tutorial stuff
    public TutorialCamera tutorialCameraPrefab;
    public SplinePath tutorialCameraPathPrefab;
    public TutorialSnake tutorialSnakePrefab;
    public GameObject tutorialDangerSymbolPrefab;
    public GameObject terrainPrefab;
    public TiltDetector tiltDetectorPrefab;
    public SlowdownTutorial slowdownTutorial1Prefab;
    public SlowdownTutorial slowdownTutorial2Prefab;
    public SlowdownTutorial slowdownTutorial3Prefab;

    // instances for tutorial stuff
    TutorialCamera tutorialCamera;
    SplinePath tutorialCameraPath;
    TutorialSnake tutorialSnake;
    GameObject tutorialDangerSymbol;
    GameObject terrain;
    TiltDetector tiltDetector;
    SlowdownTutorial slowdownTutorial1;
    SlowdownTutorial slowdownTutorial2;
    SlowdownTutorial slowdownTutorial3;

    public Transform tutorialChippySpawn;
    public Transform tutorialCameraTarget01;

    [HideInInspector] public List<MedallionSpawnPoint> medallionSpawnPoints = new List<MedallionSpawnPoint>();
    [HideInInspector] public Chippy player;
    [HideInInspector] public Snake snake;
    [HideInInspector] public bool ignoreInput;
    [HideInInspector] public bool tutorialRunning = false;
    [HideInInspector] public bool gameRunning = false;
    [HideInInspector] public bool okPressed = false;
    [HideInInspector] public bool paused;

    List<TrunkPiece> stumps = new List<TrunkPiece>();
    List<TrunkPiece> stumpPool = new List<TrunkPiece>();
    List<TrunkPiece> beaverStumpPool = new List<TrunkPiece>();
    Vector3 stumpPoolPosition;
    float nextStumpY;
    MedallionPiece medallionPiece;
    float nextMedallionSpawn;
    bool medallionPieceRespawning;
    Vector3 playerSpawnOffset = new Vector3(0.0f, 10.0f, -8.0f);
    Vector3 playerSpawnPoint = Vector3.zero;
    int nextBeaverStump;

    float timePaused;
    bool showGameOver;
    bool playAgain;
    bool reviving = false;

    static bool continueGame = false;

    public delegate void PauseAction(bool didPause, float duration);
    public static event PauseAction OnPauseChanged;

    public void TogglePause()
    {
        if(ignoreInput)
            return;

        if(!paused)
        {
            paused = true;
            Time.timeScale = 0.0f;
            timePaused = Time.time;
            OnPauseChanged?.Invoke(true, 0);
        }
        else
        {
            paused = false;
            Time.timeScale = 1.0f;
            float duration = Time.time - timePaused;
            OnPauseChanged?.Invoke(false, duration);
        }
    }

    void SetScreenTimeoutEnabled(bool enable)
    {
        Screen.sleepTimeout = enable ?
            SleepTimeout.SystemSetting : SleepTimeout.NeverSleep;
    }

    void Awake()
    {
        that = this;
        playerSpawnPoint = new Vector3(0, FirstStumpY, 0);
        playerSpawnPoint += playerSpawnOffset;

        if (continueGame)
        {
            continueGame = false;

            int lives = PlayerPrefs.GetInt(PrefKeys.LivesRemaining, 0);

            if (PrefKeys.IsImmortal)
            {
                Session.ContinueCurrent();
            }
            else if (PlayerPrefs.GetInt(PrefKeys.LivesRemaining, 0) > 0)
            {
                PlayerPrefs.SetInt(PrefKeys.LivesRemaining, --lives);
                PlayerPrefs.Save();

                Session.ContinueCurrent();
            }
            else
            {
                Debug.LogException(new System.Exception("level restarted with no lives remaining"));
                
                if (lives < 0) {
                    PlayerPrefs.SetInt(PrefKeys.LivesRemaining, 0);
                    PlayerPrefs.Save();
                }

                SceneManager.LoadScene("menu");
            }
        }
        else
        {
            Session.StartNew();
        }

        SetScreenTimeoutEnabled(false);
    }

    void OnDestroy() {
        SetScreenTimeoutEnabled(true);
    }

    void OnApplicationPause(bool pause) {
        SetScreenTimeoutEnabled(pause);
    }

    void OnApplicationQuit() {
        SetScreenTimeoutEnabled(true);
    }

    void Start()
    {
        foreach(var cam in Camera.allCameras)
            cam.eventMask = 0;
        
        GameUI.that.overlay.gameObject.SetActive(true);

        if (PlayerPrefs.GetInt(PrefKeys.TutorialComplete) != 1)
            StartCoroutine(RunTutorial());
        else
            StartCoroutine(BeginGame());
    }

    void Update()
    {
        if(Input.GetKeyUp(KeyCode.Escape) && !ignoreInput)
            TogglePause();

        if(Input.GetKeyUp(KeyCode.K) && !ignoreInput)
            KillPlayer(null);

        if(gameRunning)
        {
            if(!paused && player != null) {
                RespawnMedallionPiece();
                Session.UpdateDistanceTravelled(player.transform.position.y - playerSpawnPoint.y);
            }
        }
        else if(tutorialRunning)
        {

        }
    }

    void LateUpdate()
    {
        if(paused || player == null)
            return;

        if(gameRunning) {
            CycleTrunkPieces();
        }
        else if(tutorialRunning) {
            CycleTutorialTrunkPieces();
        }
    }

    public void KillPlayer(string how)
    {
        if(Session.KillPlayer()) {
            StartCoroutine(EndGame(how));
        }
    }

    public void EnableBranchCollision(bool enable)
    {
        foreach(var trunk in stumpPool)
            trunk.EnableBranchCollision(enable);
        
        foreach(var trunk in stumps)
            trunk.EnableBranchCollision(enable);
    }

    void CreateTree()
    {
        // create pool
        stumpPoolPosition = new Vector3(0, FirstStumpY - 200, 0);

        for(int i = 0; i < stumpPrefabs.Count; ++i)
        {
			var go = Instantiate(stumpPrefabs[i], stumpPoolPosition, Quaternion.identity);
            var tp = go.GetComponent<TrunkPiece>();
            tp.SpawnEggs(eggPool);
            stumpPool.Add(tp);
        }

        int beaverStumpCount = MaxNumStumps / MinBeaverStumpInterval + 1;
        for(int i = 0; i < beaverStumpCount; ++i)
        {
            var go = Instantiate(beaverStumpPrefab, stumpPoolPosition, Quaternion.identity);
            var tp = go.GetComponent<TrunkPiece>();
            tp.SpawnEggs(eggPool);
            beaverStumpPool.Add(tp);
        }

        // create tree
        nextStumpY = FirstStumpY - (StumpLength * 4);
        nextBeaverStump = Random.Range(MinBeaverStumpInterval, MaxBeaverStumpInterval + 1);

        for(int i = 0; i < MaxNumStumps; i++)
        {
            TrunkPiece stump = null;

            if(i == 4)
            {
                stump = Instantiate(emptyStump).GetComponent<TrunkPiece>();
            }
            else
            {
                if(i > 4 && --nextBeaverStump <= 0 && beaverStumpPool.Count > 0)
                {
                    int stumpIndex = beaverStumpPool.Count - 1;
                    stump = beaverStumpPool[stumpIndex];
                    beaverStumpPool.RemoveAt(stumpIndex);

                    nextBeaverStump = Random.Range(MinBeaverStumpInterval, MaxBeaverStumpInterval + 1);
                }
                else
                {
                    int stumpIndex = Random.Range(0, stumpPool.Count - 1);
                    stump = stumpPool[stumpIndex];
                    stumpPool.RemoveAt(stumpIndex);
                }
            }

            stump.transform.position = Vector3.up * nextStumpY;
            stumps.Add(stump);
            nextStumpY += StumpLength;
        }
    }

    void CreateTutorialTree()
    {
        // create pool
        stumpPoolPosition = new Vector3(0, FirstStumpY - 200, 0);

        for(int i = 0; i < MaxNumStumps + 2; ++i)
        {
            var go = Instantiate(stumpPrefabs[i % 14], stumpPoolPosition, Quaternion.identity);
            var tp = go.GetComponent<TrunkPiece>();
            stumpPool.Add(tp);
        }

        // create tree
        nextStumpY = FirstStumpY - (StumpLength * 4);

        for(int i = 0; i < MaxNumStumps; i++)
        {
            TrunkPiece stump = null;

            if(i < 4) {
                stump = Instantiate(emptyStump).GetComponent<TrunkPiece>();
            }
            else if(i == 4) {
                stump = stumpPool[1];
                stumpPool.RemoveAt(1);
            }
            else {
                int stumpIndex = Random.Range(0, stumpPool.Count - 1);
                stump = stumpPool[stumpIndex];
                stumpPool.RemoveAt(stumpIndex);
            }

            stump.transform.position = Vector3.up * nextStumpY;
            stumps.Add(stump);
            nextStumpY += StumpLength;
        }
    }

    void DestroyTree()
    {
        stumpPoolPosition = Vector3.zero;
        nextStumpY = 0;

        foreach(var stump in stumpPool)
            Destroy(stump.gameObject);

        foreach(var stump in beaverStumpPool)
            Destroy(stump.gameObject);

        foreach(var stump in stumps)
            Destroy(stump.gameObject);
        
        stumpPool.Clear();
        beaverStumpPool.Clear();
        stumps.Clear();
    }

    void CycleTutorialTrunkPieces()
    {
        if(!player)
            return;

        int removeCount = 0;
        for( ; removeCount < stumps.Count; ++removeCount)
        {
            if(stumps[removeCount].transform.position.y >= player.transform.position.y - (StumpLength * 7))
                break;
        }

        if(removeCount > 0)
        {
            for(int i = 0; i < removeCount; ++i)
            {
                var stump = stumps[i];
                stump.transform.position = stumpPoolPosition;
                stumpPool.Add(stump);
            }

            stumps.RemoveRange(0, removeCount);

            for(int i = 0; i < removeCount; ++i)
            {
                int stumpIndex = Random.Range(0, stumpPool.Count - 1);
                TrunkPiece stump = stumpPool[stumpIndex];
                stumpPool.RemoveAt(stumpIndex);

                stumps.Add(stump);
                stump.transform.position = Vector3.up * nextStumpY;
                stump.FadeIn(3.0f);

                nextStumpY += StumpLength;
            }
        }
    }

    void CycleTrunkPieces()
    {
        if(!player)
            return;

        int removeCount = 0;
        for( ; removeCount < stumps.Count; ++removeCount)
        {
            if(stumps[removeCount].transform.position.y >= player.transform.position.y - (StumpLength * 7))
                break;
        }

        if(removeCount > 0)
        {
            for(int i = 0; i < removeCount; ++i)
            {
                var stump = stumps[i];
                stump.transform.position = stumpPoolPosition;

                if(stump.isBeaverStump)
                    beaverStumpPool.Add(stump);
                else
                    stumpPool.Add(stump);
            }

            stumps.RemoveRange(0, removeCount);

            for(int i = 0; i < removeCount; ++i)
            {
                TrunkPiece stump = null;

                if(--nextBeaverStump <= 0 && beaverStumpPool.Count > 0)
                {
                    int stumpIndex = beaverStumpPool.Count - 1;
                    stump = beaverStumpPool[stumpIndex];
                    beaverStumpPool.RemoveAt(stumpIndex);

                    nextBeaverStump = Random.Range(MinBeaverStumpInterval, MaxBeaverStumpInterval + 1);
                }
                else
                {
                    int stumpIndex = Random.Range(0, stumpPool.Count - 1);
                    stump = stumpPool[stumpIndex];
                    stumpPool.RemoveAt(stumpIndex);
                }

                stumps.Add(stump);
                stump.transform.position = Vector3.up * nextStumpY;
                stump.SpawnEggs(eggPool);
                stump.FadeIn(3.0f);

                nextStumpY += StumpLength;
            }
        }
    }

    void SpawnPlayer()
    {
		Vector3 core = new Vector3(0, playerSpawnPoint.y, 0);
		Vector3 upNormalized = (playerSpawnPoint - core).normalized;

		RaycastHit hit;
        if (Physics.Raycast(new Ray(core + upNormalized * 20.0f, -upNormalized), out hit, 20.0f, 1 << Layer.Stumps | 1 << Layer.Ramps))
            playerSpawnPoint = hit.point;

        var spawnDir = Quaternion.FromToRotation(Vector3.forward, Vector3.up);
        player = Instantiate(playerPrefab, playerSpawnPoint, spawnDir).GetComponent<Chippy>();
    }

    void SpawnSnake()
    {
        snake = Instantiate(snakePrefab).GetComponent<Snake>();
    }

    void RespawnMedallionPiece()
    {
        if(medallionPiece)
        {
            if(medallionPiece.transform.position.y < player.transform.position.y - 10.0f)
            {
                medallionPiece.ReturnToPool();
                medallionPiece = null;
            }
            else if(!medallionPiece.gameObject.activeSelf)
            {
                medallionPiece = null;
            }
            else
            {
                return;
            }
        }
        
        if(!medallionPieceRespawning)
        {
            nextMedallionSpawn = Time.time + MedallionRespawnDelay;
            medallionPieceRespawning = true;
        }

        if(Time.time >= nextMedallionSpawn)
        {
            Transform idealSpawnPoint = null;
            float pointDist = 0;
            float smallestDiff = float.MaxValue;

            foreach(var spawnPoint in medallionSpawnPoints)
            {
                float distAhead = spawnPoint.transform.position.y - player.transform.position.y;
                float diff = Mathf.Abs(MedallionSpawnDist - distAhead);

                if(distAhead > 0 && diff < smallestDiff)
                {
                    idealSpawnPoint = spawnPoint.transform;
                    smallestDiff = diff;
                    pointDist = distAhead;
                }
            }

            if(idealSpawnPoint != null)
            {
                medallionPiece = medallionPool.Spawn<MedallionPiece>(
                    idealSpawnPoint.position,
                    idealSpawnPoint.rotation,
                    Session.MedallionPieceCount);

                medallionPieceRespawning = false;
            }
        }
    }

    public void OnPauseTogglePressed()
    {
        SharedSounds.button.Play();
        TogglePause();
    }

    public void OnRevivePressed()
    {
        reviving = true;
        
        SharedSounds.button.Play();
        GameUI.that.alertButton.Hide();

        StartCoroutine(ContinueGame());
    }

    IEnumerator ContinueGame()
    {
        yield return new WaitForSeconds(1.0f);

        if (!PrefKeys.IsImmortal)
        {
            lifeView.Show();
            yield return new WaitForSeconds(1.0f);

            lifeView.SubtractLives(1);
            yield return new WaitForSeconds(1.0f);

            lifeView.Hide();
            yield return new WaitForSeconds(1.0f);
        }

        continueGame = true;
        SceneManager.LoadSceneAsync("game");
    }

    public void OnExitPressed()
    {
        SharedSounds.button.Play();
        TogglePause();
        SceneManager.LoadScene("menu");
    }

    public void OnTutorialOKPressed()
    {
        SharedSounds.button.Play();
        okPressed = true;
    }

    public void OnHelpPressed()
    {
        SharedSounds.button.Play();
        StopAllCoroutines();
        GameUI.that.optionButton.Hide();
        ignoreInput = true;
        GameUI.that.overlay.gameObject.SetActive(true);
        PlayerPrefs.SetInt(PrefKeys.TutorialComplete, 0);
        SceneManager.LoadSceneAsync("game");
    }

    public void OnStartPressed()
    {
        SharedSounds.button.Play();
        okPressed = true;
    }

    public void OnScoreDonePressed()
    {
        SharedSounds.button.Play();

        string username = GameUI.that.nameInput.text;

        if(!string.IsNullOrEmpty(username))
        {
            MenuUI.mostRecentScoreID = ScoreService.PutScore(username, Session.Points);

            GameUI.that.scoreTitle.gameObject.SetActive(false);
            GameUI.that.scoreEggImage.gameObject.SetActive(false);
            GameUI.that.scoreEggCount.gameObject.SetActive(false);
            GameUI.that.scoreMedallion00.gameObject.SetActive(false);
            GameUI.that.scoreMedallionFull.gameObject.SetActive(false);
            GameUI.that.scoreMedallionCount.gameObject.SetActive(false);
            GameUI.that.scoreText.gameObject.SetActive(false);
            GameUI.that.scoreNameMessage.gameObject.SetActive(false);
            GameUI.that.nameInput.gameObject.SetActive(false);
            GameUI.that.doneButton.gameObject.SetActive(false);
            GameUI.that.activityIndicator.SetActive(true);

            playAgain = false;
            showGameOver = false;
        }
        else
        {
            showGameOver = false;
        }
    }

    public void OnScoreQuitPressed()
    {
        SharedSounds.button.Play();
        playAgain = false;
        showGameOver = false;
    }

    public void OnScorePlayPressed()
    {
        SharedSounds.button.Play();
        playAgain = true;
        showGameOver = false;
    }

    IEnumerator NotifySnakeDanger()
    {
        tutorialDangerSymbol.SetActive(true);
        SharedSounds.boing.Play();

        GameUI.that.tutorialText01.gameObject.SetActive(true);
        GameUI.that.tutorialText01.transform.localScale = new Vector3(0, 1, 1);

        yield return Util.Blend(0.333f, t => {
            float s = Curve.InElastic(Util.InverseLerp(t, 0.0f, 0.6f)) * 3;
            tutorialDangerSymbol.transform.localScale = Vector3.one * s;

            float fastT = 1.0f - (1.0f - t) * (1.0f - t);
            tutorialSnake.meshRenderer.material.SetColor("_ExtrusionColor", new Color(1, 0, 0, fastT));
            GameUI.that.tutorialText01.transform.localScale = new Vector3(Curve.InElastic(Util.InverseLerp(t, 0.4f, 1.0f)), 1, 1);
        });
    }

    IEnumerator HideSnakeNotification()
    {
        yield return Util.Blend(0.1f, t => {
            GameUI.that.tutorialText01.transform.localScale = new Vector3(1.0f - t, 1, 1);
        });

        GameUI.that.tutorialText01.gameObject.SetActive(false);
        tutorialDangerSymbol.SetActive(false);
    }

    float AttentionBounce(float t)
    {
        var twoPI = Mathf.PI * 2;
        var oscillations = 7;
        var scaleEase = (1.0f - t) * (1.0f - t);
        var timeEase = (1.0f - t) + (1.0f - t * t);
        var scale = 0.2f;
        return (-Mathf.Cos(timeEase * t * twoPI * oscillations) * 0.5f + 0.5f) * scaleEase * scale;
    }

    IEnumerator WaitForOK()
    {
        okPressed = false;

        GameUI.that.optionButton.Show("OK", OnTutorialOKPressed);

        yield return new WaitUntil(() => okPressed);

        GameUI.that.optionButton.Hide();
    }

    IEnumerator RunTutorial()
    {
        ignoreInput = true;
        okPressed = false;
        
        Music.that.PlayTrack(AudioTrack.NatureAmbience);
        drumSource.Play();
        terrain = Instantiate(terrainPrefab);
        tutorialCamera = Instantiate(tutorialCameraPrefab.gameObject).GetComponent<TutorialCamera>();
        tutorialCameraPath = Instantiate(tutorialCameraPathPrefab.gameObject).GetComponent<SplinePath>();
        tutorialSnake = Instantiate(tutorialSnakePrefab.gameObject).GetComponent<TutorialSnake>();
        tutorialDangerSymbol = Instantiate(tutorialDangerSymbolPrefab);

        RenderSettings.skybox = cloudySkybox;

        tutorialDangerSymbol.SetActive(false);
        mainCamera.gameObject.SetActive(false);
        tutorialCamera.gameObject.SetActive(true);

        var spline = tutorialCameraPath.spline;

        var sp0 = spline.points[0];
        tutorialCamera.transform.position = sp0.pos;
        tutorialCamera.transform.LookAt(new Vector3(0, sp0.pos.y, 0));

        GameUI.that.tutorialScreen.SetActive(true);
        CreateTutorialTree();
        SpawnPlayer();
        player.allowJump = false;
        player.tutorialMode = true;

        var chippyStartSpawnPos = player.transform.position;
        var chippyStartSpawnRot = player.transform.rotation;
        player.transform.position = tutorialChippySpawn.position;
        player.transform.rotation = tutorialChippySpawn.rotation;

        // fade in from black
        StartCoroutine(Util.Blend(
            1.0f, t => GameUI.that.overlay.color = new Color(0, 0, 0, 1.0f - t),
            () => GameUI.that.overlay.gameObject.SetActive(false)
        ));

        // do camera pan to show snake-danger and chippy
        tutorialSnake.PlayAnimation();

        // pan camera for 3 seconds then stop
        yield return StartCoroutine(Util.Blend(3.0f, 0.0f, 3.0f / 9.0f, t => {
            t = Curve.SmoothStep(t);

            var sp = spline.SampleFrac(t);
            tutorialCamera.transform.position = sp.pos;

            var tree = new Vector3(0, sp.pos.y, 0);
            var target = new Vector3(tutorialCameraTarget01.position.x, sp.pos.y, tutorialCameraTarget01.position.z);

            float lookT = Util.InverseLerp(t, 0.5f, 1.0f);
            lookT = lookT * lookT;
            var lookAt = Vector3.Lerp(tree, target, lookT);
            tutorialCamera.transform.LookAt(lookAt);
        }));

        // wait for ok
        yield return StartCoroutine(NotifySnakeDanger());
        yield return StartCoroutine(WaitForOK());
        yield return StartCoroutine(HideSnakeNotification());

        yield return StartCoroutine(Util.Blend(6.0f, 3.0f / 9.0f, 1.0f, t => {
            t = Curve.SmoothStep(t);

            var sp = spline.SampleFrac(t);
            tutorialCamera.transform.position = sp.pos;

            var tree = new Vector3(0, sp.pos.y, 0);
            var target = new Vector3(tutorialCameraTarget01.position.x, sp.pos.y, tutorialCameraTarget01.position.z);

            float lookT = Util.InverseLerp(t, 0.5f, 1.0f);
            lookT = lookT * lookT;
            var lookAt = Vector3.Lerp(tree, target, lookT);
            tutorialCamera.transform.LookAt(lookAt);
        }));

        yield return new WaitForSeconds(2.0f);

        // fade to follow camera
        GameUI.that.overlay.gameObject.SetActive(true);
        yield return Util.Blend(1.0f, t => GameUI.that.overlay.color = new Color(0, 0, 0, t));

        {
            Destroy(terrain);
            terrain = null;

            Destroy(tutorialSnake.gameObject);
            tutorialSnake = null;

            Destroy(tutorialDangerSymbol);
            tutorialDangerSymbol = null;

            RenderSettings.skybox = treeSkybox;
            tutorialCamera.gameObject.SetActive(false);
            mainCamera.gameObject.SetActive(true);

            // align chippy again so he's ready to run
            player.transform.position = chippyStartSpawnPos;
            player.transform.rotation = chippyStartSpawnRot;
        }

        yield return Util.Blend(1.0f, t => GameUI.that.overlay.color = new Color(0, 0, 0, 1.0f - t));
        GameUI.that.overlay.gameObject.SetActive(false);
        // ------------------

        GameUI.that.optionButton.Show("Skip", OnTutorialSkipPressed);

        // wait one second while viewing chippy
        yield return new WaitForSeconds(1);

        tutorialRunning = true; // causes chippy to start running

        // wait half a second then show tilt graphic
        yield return new WaitForSeconds(0.5f);

        GameUI.that.tiltDevice.gameObject.SetActive(true);
        GameUI.that.tutorialText02.gameObject.SetActive(true);
        GameUI.that.tutorialText02.transform.localScale = new Vector3(0, 1, 1);
        tiltDetector = Instantiate(tiltDetectorPrefab.gameObject).GetComponent<TiltDetector>();

        yield return Util.Blend(0.25f, t => {
            t = Curve.InElastic(t);
            GameUI.that.tiltDevice.transform.localScale = new Vector3(t, 1, 1);
            GameUI.that.tutorialText02.transform.localScale = new Vector3(t, 1, 1);
        });

        bool rotateCCW = true;
        float angle = 0;

        // wait for player to tilt the device
        ignoreInput = false;
        while(!tiltDetector.triggered)
        {
            if(rotateCCW)
            {
                if(angle < 20.0f) {
                    angle += Time.deltaTime * 40.0f;
                }
                else {
                    rotateCCW = false;
                    GameUI.that.tiltArrows.transform.localScale = new Vector3(-1, 1, 1);
                }
            }
            else
            {
                if(angle > -20.0f) {
                    angle -= Time.deltaTime * 40.0f;
                }
                else {
                    rotateCCW = true;
                    GameUI.that.tiltArrows.transform.localScale = new Vector3(1, 1, 1);
                }
            }

            GameUI.that.tiltDevice.transform.localEulerAngles = new Vector3(0, 0, angle);

            yield return null;
        }

        yield return Util.Blend(0.25f, t => {
            GameUI.that.tiltDevice.transform.localScale = new Vector3(1.0f - t, 1, 1);
            GameUI.that.tutorialText02.transform.localScale = new Vector3(1.0f - t, 1, 1);
        });

        GameUI.that.tiltDevice.gameObject.SetActive(false);
        GameUI.that.tutorialText02.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        GameUI.that.tutorialText03.gameObject.SetActive(true);

        yield return Util.Blend(0.25f, t => {
            GameUI.that.tutorialText03.transform.localScale = new Vector3(Curve.InElastic(t), 1, 1);
        });

        // show tutorialText03 for 3 seconds
        yield return new WaitForSeconds(3);

        yield return Util.Blend(0.25f, t => {
            GameUI.that.tutorialText03.transform.localScale = new Vector3(1.0f - t, 1, 1);
        });

        GameUI.that.tutorialText03.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        GameUI.that.tutorialText04.gameObject.SetActive(true);

        yield return Util.Blend(0.25f, t => {
            GameUI.that.tutorialText04.transform.localScale = new Vector3(Curve.InElastic(t), 1, 1);
        });

        GameUI.that.tapDevice.SetActive(true);

        // wait for player to jump 3 times
        player.allowJump = true;

        string fmt = "Tap the screen to\nmake Chippy jump!\n[<color=#{0}>{1}</color>/3]";
        int currentJumpCount = 0;
        float jumpHighlight = 0.0f;

        while(player.jumpCount < 3)
        {
            if(currentJumpCount != player.jumpCount) {
                currentJumpCount = player.jumpCount;
                jumpHighlight = 1.0f;
            }
            
            Color hintNumberColor = Color.Lerp(new Color(1.0f, 0.5f, 0.0f, 1.0f), Color.white, jumpHighlight);
            var num = ColorUtility.ToHtmlStringRGBA(hintNumberColor);

            jumpHighlight = Mathf.Max(jumpHighlight - Time.deltaTime, 0);

            GameUI.that.tutorialText04.text = string.Format(fmt, num, player.jumpCount);
            yield return null;
        }

        var finalNumColor = ColorUtility.ToHtmlStringRGBA(new Color(1.0f, 0.5f, 0.0f, 1.0f));
        GameUI.that.tutorialText04.text = string.Format(fmt, finalNumColor, player.jumpCount);

        yield return new WaitForSeconds(1.0f);

        yield return Util.Blend(0.25f, t => {
            GameUI.that.tutorialText04.transform.localScale = new Vector3(1.0f - t, 1, 1);
            GameUI.that.tapDevice.transform.localScale = new Vector3(1.0f - t, 1, 1);
        });

        GameUI.that.tapDevice.SetActive(false);
        GameUI.that.tutorialText04.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        GameUI.that.tutorialText05.gameObject.SetActive(true);

        yield return Util.Blend(0.25f, t => {
            GameUI.that.tutorialText05.transform.localScale = new Vector3(Curve.InElastic(t), 1, 1);
        });

        // wait for player to read the message
        yield return new WaitForSeconds(3);

        yield return Util.Blend(0.25f, t => {
            GameUI.that.tutorialText05.transform.localScale = new Vector3(1.0f - t, 1, 1);
        });

        GameUI.that.tutorialText05.gameObject.SetActive(false);

        // fade to black and swap to camera pan view
        GameUI.that.overlay.gameObject.SetActive(true);
        yield return Util.Blend(1.0f, t => GameUI.that.overlay.color = new Color(0, 0, 0, t));

        {
            // enable camera pan stuff
            DestroyTree();
            Destroy(player.gameObject);
            player = null;

            mainCamera.gameObject.SetActive(false);
        }

        slowdownTutorial1 = Instantiate(slowdownTutorial1Prefab.gameObject).GetComponent<SlowdownTutorial>();
        yield return slowdownTutorial1.PlayTutorial();

        Destroy(slowdownTutorial1.gameObject);
        slowdownTutorial1 = null;

        slowdownTutorial2 = Instantiate(slowdownTutorial2Prefab.gameObject).GetComponent<SlowdownTutorial>();
        yield return slowdownTutorial2.PlayTutorial();

        Destroy(slowdownTutorial2.gameObject);
        slowdownTutorial2 = null;

        slowdownTutorial3 = Instantiate(slowdownTutorial3Prefab.gameObject).GetComponent<SlowdownTutorial>();
        yield return slowdownTutorial3.PlayTutorial();

        Destroy(slowdownTutorial3.gameObject);
        slowdownTutorial3 = null;

        mainCamera.gameObject.SetActive(true);

        yield return new WaitForSeconds(1);

        PlayerPrefs.SetInt(PrefKeys.TutorialComplete, 1);

        tutorialRunning = false;
        GameUI.that.overlay.gameObject.SetActive(true);

        drumSource.Stop();
        DestroyTree();
        mainCamera.gameObject.SetActive(true);

        if(slowdownTutorial1) {
            Destroy(slowdownTutorial1.gameObject);
            slowdownTutorial1 = null;
        }

        if(slowdownTutorial2) {
            Destroy(slowdownTutorial2.gameObject);
            slowdownTutorial2 = null;
        }

        if(slowdownTutorial3) {
            Destroy(slowdownTutorial3.gameObject);
            slowdownTutorial3 = null;
        }

        if(tutorialSnake) {
            Destroy(tutorialSnake.gameObject);
            tutorialSnake = null;
        }

        if(tutorialCamera) {
            Destroy(tutorialCamera.gameObject);
            tutorialCamera = null;
        }

        if(tutorialCameraPath) {
            Destroy(tutorialCameraPath.gameObject);
            tutorialCameraPath = null;
        }

        if(tutorialDangerSymbol) {
            Destroy(tutorialDangerSymbol);
            tutorialDangerSymbol = null;
        }

        if(terrain != null) {
            Destroy(terrain);
            terrain = null;
        }

        if(tiltDetector != null) {
            Destroy(tiltDetector.gameObject);
            tiltDetector = null;
        }

        if(player) {
            Destroy(player.gameObject);
            player = null;
        }

        // enable ready buttons and wait for clicks

        okPressed = false;

        GameUI.that.optionButton.Hide(true);
        GameUI.that.startButton.gameObject.SetActive(true);
        GameUI.that.tutorialDoneText.gameObject.SetActive(true);
        
        yield return StartCoroutine(Util.Blend(1.0f, t => GameUI.that.overlay.color = new Color(0, 0, 0, 1.0f - t)));
        GameUI.that.overlay.gameObject.SetActive(false);

        yield return new WaitUntil(() => okPressed);

        GameUI.that.overlay.gameObject.SetActive(true);
        yield return StartCoroutine(Util.Blend(1.0f, t => GameUI.that.overlay.color = new Color(0, 0, 0, t)));

        GameUI.that.startButton.gameObject.SetActive(false);
        GameUI.that.tutorialDoneText.gameObject.SetActive(false);
        GameUI.that.tutorialScreen.SetActive(false);

        yield return new WaitForSeconds(1);

        StartCoroutine(BeginGame());
    }

    public void OnTutorialSkipPressed()
    {
        StopAllCoroutines();

        SharedSounds.button.Play();
        StopAllCoroutines();
        ignoreInput = true;
        GameUI.that.overlay.gameObject.SetActive(true);

        PlayerPrefs.SetInt(PrefKeys.TutorialComplete, 1);
        PlayerPrefs.Save();

        SceneManager.LoadSceneAsync("game");
    }

    IEnumerator BeginGame()
    {
        ignoreInput = true;

        RenderSettings.skybox = treeSkybox;

        CreateTree();
        SpawnPlayer();
        SpawnSnake();

        Music.that.PlayTrack(AudioTrack.GameMusic);

        yield return StartCoroutine(Util.Blend(1.0f, t => {
            GameUI.that.overlay.color = new Color(0, 0, 0, 1.0f - t);
        }));

        GameUI.that.hud.SetActive(true);
        GameUI.that.overlay.gameObject.SetActive(false);

        GameUI.that.optionButton.Show("Help!", OnHelpPressed);

        var pt = GameUI.that.popupText;
        pt.color = new Color(1.0f, 0.5f, 0.0f, 1.0f);
        pt.text = "";
        pt.gameObject.SetActive(true);

        float countDownTime = 4;
        float countDownFinishTime = Time.time + countDownTime;

        while(Time.time <= countDownFinishTime)
        {
            float timeLeft = countDownFinishTime - Time.time;
            if (timeLeft > 1.0f)
                pt.text = Mathf.Clamp((int)timeLeft, 0, 3).ToString();
            else
                pt.text = "GO!";

            yield return null;
        }

        pt.gameObject.SetActive(false);
        pt.text = "";

        GameUI.that.optionButton.Show("Pause", OnPauseTogglePressed);

        ignoreInput = false;
        gameRunning = true;
    }

    IEnumerator EndGame(string how)
    {
        ignoreInput = true;
        gameRunning = false;

        Session.EndCurrent();

        // hide UI
        GameUI.that.optionButton.Hide();
        GameUI.that.hud.SetActive(false);
        
        SharedSounds.death.Play();

        float musicStartVol = Music.that.source.volume;
        StartCoroutine(Util.Blend(0.5f, t => { Music.that.source.volume = musicStartVol * (1.0f - t); }));
        
        yield return new WaitForSeconds(1.0f);

        Music.that.Stop();

        GameUI.that.overlay.gameObject.SetActive(true);
        GameUI.that.overlay.color = new Color(0, 0, 0, 0);

        StartCoroutine(Util.Blend(2.5f, t => { GameUI.that.overlay.color = new Color(0, 0, 0, t); }));

        yield return new WaitForSeconds(1.0f);

        if (PrefKeys.IsImmortal || PlayerPrefs.GetInt(PrefKeys.LivesRemaining, 0) > 0)
        {
            GameUI.that.alertButton.Show("Keep\nRunning?", OnRevivePressed, 2);

            yield return new WaitForSeconds(2.0f);

            if (GameUI.that.alertButton.button.interactable)
                GameUI.that.alertButton.Show("Tap\nHere!", OnRevivePressed, 3);

            GameUI.that.overlay.color = new Color(0, 0, 0, 1.0f);

            yield return new WaitForSeconds(2.0f);

            GameUI.that.alertButton.Hide();
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            yield return new WaitForSeconds(1.5f);
        }

        while (reviving)
            yield return null;

        var pt = GameUI.that.centeredPopupText;
        pt.color = new Color(1.0f, 0.5f, 0.0f, 1.0f);
        pt.text = "Game Over!";
        pt.gameObject.SetActive(true);

        DestroyTree();
		Destroy(player.gameObject);

        yield return new WaitForSeconds(1.0f);

        yield return StartCoroutine(Util.Blend(1.0f, t => { pt.color = new Color(1.0f, 0.5f, 0.0f, 1.0f - t); }));

        pt.gameObject.SetActive(false);
        pt.text = "";

        {
            showGameOver = true;

            Music.that.PlayTrack(AudioTrack.NatureAmbience);
            GameUI.that.drumSource.Play();

            // fades in from black
            GameUI.that.scoreScreen.SetActive(true);
            GameUI.that.StartCoroutine(GameUI.that.DoScoreAnimation());

            while(showGameOver)
                yield return null;
            
            // fade to black
            GameUI.that.overlay.gameObject.SetActive(true);
            yield return StartCoroutine(Util.Blend(0.5f, t => { GameUI.that.overlay.color = new Color(0, 0, 0, t); }));

            GameUI.that.scoreScreen.SetActive(false);
        }

        ignoreInput = false;

        if(playAgain)
            SceneManager.LoadScene("game");
        else
            SceneManager.LoadScene("menu");
    }
}
