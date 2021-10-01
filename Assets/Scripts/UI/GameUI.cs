using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using TMPro;

public class GameUI : MonoBehaviour, IPointerDownHandler
{
    public static GameUI that {
        get; private set;
    }

    public Camera uiCam;
    public Camera sceneCam;
    public Canvas canvas;
    public GameObject tutorialScreen;
    public GameObject hud;
    public GameObject pauseScreen;
    public GameObject scoreScreen;
    public GameObject activityIndicator;
    public OptionButton optionButton;
    public AlertButton alertButton;
    public Button doneButton;
    public Button startButton;
    public TMP_Text tutorialText01;
    public TMP_Text tutorialText02;
    public TMP_Text tutorialText03;
    public TMP_Text tutorialText04;
    public TMP_Text tutorialText05;
    public TMP_Text tutorialDoneText;
    public TMP_Text[] tutorialTextSlowdown;

    public RawImage overlay;
    public RawImage damageOverlay;
    public TMP_Text livesRemainingText;
    public TMP_Text infinityText;
    public TMP_Text eggCountText;
    public TMP_Text pointCountText;
    public TMP_Text medallionCountText;
    public Image medallionImage;
    public Image medallionGlow;
    public Image eggGlow;
    public SpeedBar speedBar;
    public TMP_Text popupText;
    public TMP_Text centeredPopupText;
    public Image medallion;
    public Sprite[] medallionTextures;
    public ObjectPool floatingPointsPool;
    public AudioSource drumSource;
    public RawImage tiltDevice;
    public RawImage tiltArrows;
    public GameObject tapDevice;
    
    bool paused = false;
    float eggCountPhase;
    float eggGlowPhase;
    float pointCountPhase;
    float medallionCountPhase;
    float medallionGlowPhase;

    public void OnPointerDown(PointerEventData eventData)
    {
        if(Game.that.player)
            Game.that.player.stateMachine.OnPointerDown(eventData);
    }

    void SpawnPoints(Vector3 pos, int pointsCount)
    {
        var points = floatingPointsPool.Spawn<FloatingPoints>(pos, Quaternion.identity);
        points.label.text = pointsCount.ToString();
        points.velocity = Vector3.up * 2.0f;
        points.start = Time.time;
        points.life = 1.0f;
        points.GameObject.transform.SetParent(transform, false);
        points.transform.localPosition = new Vector3(Mathf.Sin(Random.value * Mathf.PI * 2.0f) * 10.0f, 0, 0);
    }

    public static void CollectEgg(SnakeEgg egg)
    {
        that.StartCoroutine(that._collectEgg(egg));
    }

    IEnumerator _collectEgg(SnakeEgg egg)
    {
        SharedSounds.eggCollected.Play();
        SpawnPoints(egg.transform.position, Session.PointsPerEgg);

        var screenPos = uiCam.WorldToScreenPoint(eggGlow.transform.position);
        screenPos.z = Vector3.Distance(sceneCam.transform.position, egg.transform.position);
        var worldPos = sceneCam.ScreenToWorldPoint(screenPos);
        egg.transform.parent = sceneCam.transform;

        var startPos = egg.transform.localPosition;
        var finishPos = sceneCam.transform.InverseTransformPoint(worldPos);

        yield return Util.Blend(0.3f, t => {
            egg.transform.localPosition = Vector3.Lerp(startPos, finishPos, t);
        });

        egg.ReturnToPool();

        eggCountPhase = 1.0f;
        eggGlowPhase = 1.0f;
        pointCountPhase = 1.0f;

        Session.EggCollected();
    }

    public static void CollectMedallionPiece(MedallionPiece piece)
    {
        that.StartCoroutine(that._collectMedallionPiece(piece));
    }

    IEnumerator _collectMedallionPiece(MedallionPiece piece)
    {
        SharedSounds.medallionPieceCollected.Play();

        bool lastPiece = Session.MedallionPieceCount == 4;
        int pointsAwarded = lastPiece ? Session.PointsPerWholeMedallion : Session.PointsPerMedallionPiece;
        SpawnPoints(piece.transform.position, pointsAwarded);

        var screenPos = uiCam.WorldToScreenPoint(medallionGlow.transform.position);
        screenPos.z = Vector3.Distance(sceneCam.transform.position, piece.transform.position);
        var worldPos = sceneCam.ScreenToWorldPoint(screenPos);
        piece.transform.parent = sceneCam.transform;

        var startPos = piece.transform.localPosition;
        var finishPos = sceneCam.transform.InverseTransformPoint(worldPos);
        var startRot = piece.transform.rotation;
        var finishRot = sceneCam.transform.rotation;

        yield return Util.Blend(0.3f, t => {
            piece.transform.localPosition = Vector3.Lerp(startPos, finishPos, t);
            piece.transform.rotation = Quaternion.Slerp(startRot, finishRot, 1.0f - Mathf.Pow(1 - t, 8));
        });

        piece.ReturnToPool();

        if(Session.MedallionPieceCount == 4)
        {
            Session.MedallionPieceCollected(false);
            yield return StartCoroutine(DoMedallionAnimation());
        }
        else
        {
            Session.MedallionPieceCollected();
            medallionGlowPhase = 1.0f;
        }
    }

    public IEnumerator DoMedallionAnimation()
    {
        var startPos = medallionImage.transform.position;
        var finishPos = medallionCountText.transform.position;

        medallion.gameObject.SetActive(true);
        medallion.transform.position = startPos;
        medallion.transform.localScale = Vector3.one;
        medallion.color = Color.white;

        yield return Util.Blend(0.7f, t =>
        {
            float t1 = t * t - t * Mathf.Sin(Mathf.PI * t);
            float t2 = t * t - t * Mathf.Sin(Mathf.PI * Mathf.Pow(t, 0.05f)) * 5.0f;

            medallion.transform.position = startPos + (finishPos - startPos) * t2;
            medallion.transform.localScale = Vector3.one * (1.0f - t1 * 0.8f);
            medallion.color = new Color(1.0f, 1.0f, 1.0f, (1.0f - t1 * 0.8f));
        });

        medallion.gameObject.SetActive(false);

        SharedSounds.medallionCollected.Play();
        Session.FullMedallionCollected();
        medallionCountPhase = 1.0f;
    }

    public Image scoreEggImage;
    public Image scoreEggGlow;
    public Image scoreMedallionGlow;
    public Image scoreMedallion00;
    public Image scoreMedallion01;
    public Image scoreMedallion02;
    public Image scoreMedallion03;
    public Image scoreMedallion04;
    public Image scoreMedallion05;
    public Image scoreMedallionFull;

    public TMP_Text scoreEggCount;
    public TMP_Text scoreMedallionCount;
    public TMP_Text scoreText;
    public TMP_Text scoreTitle;

    public Transform scoreTitleFinalPosition;
    public Transform scoreTextFinalPosition;
    public TMP_Text scoreNameMessage;
    public TMP_InputField nameInput;
    
    static readonly Color OrangeColor = Util.HexRGBA(0xFFDB00FF);
    static readonly Color DarkOrangeColor = Util.HexRGBA(0xFF7F00FF);
    static readonly Color RedColor = Util.HexRGBA(0xFF0000FF);

    public IEnumerator DoScoreAnimation()
    {
        scoreEggCount.text = Session.EggCount.ToString();
        scoreEggCount.transform.localScale = new Vector3(1, 0, 1);
        scoreEggCount.color = Session.EggCount > 0 ? DarkOrangeColor : RedColor;
        scoreEggGlow.color = Session.EggCount > 0 ? OrangeColor : RedColor;
        SetOpacity(scoreEggGlow, 0);
        SetOpacity(scoreEggImage, 0);

        scoreMedallionCount.text = Session.MedallionCount.ToString();
        scoreMedallionCount.transform.localScale = new Vector3(1, 0, 1);
        scoreMedallionCount.color = Session.MedallionCount > 0 ? DarkOrangeColor : RedColor;
        scoreMedallionGlow.color = Session.MedallionCount > 0 ? OrangeColor : RedColor;
        SetOpacity(scoreMedallionGlow, 0);
        SetOpacity(scoreMedallion00, 0);

        scoreText.text = "0";
        scoreText.transform.localScale = new Vector3(1, 0, 1);

        doneButton.transform.localScale = Vector3.zero;

        // FADE IN TO SCORE SCREEN
        yield return StartCoroutine(Util.Blend(1.0f, t => {
            SetOpacity(overlay, (1.0f - t) * (1.0f - t));
        }));

        overlay.gameObject.SetActive(false);

        // ANIMATE COLLECTED EGGS
        yield return StartCoroutine(Util.Blend(0.5f, t => {
            SetOpacity(scoreEggImage, Curve.InCube(t));
        }));

        yield return new WaitForSeconds(0.25f);

        SharedSounds.pop.Play();
        StartCoroutine(Util.Blend(0.5f, t => {
            SetOpacity(scoreEggGlow, 1.0f - t);
        }));

        yield return StartCoroutine(Util.Blend(0.5f, t => {
            scoreEggCount.transform.localScale = new Vector3(1, Curve.InElastic(t), 1);
        }));

        // ANIMATE COLLECTED MEDALLIONS
        yield return StartCoroutine(Util.Blend(0.5f, t => {
            SetOpacity(scoreMedallion00, Curve.InCube(t));
        }));

        yield return new WaitForSeconds(0.25f);

        if(Session.MedallionCount > 0)
        {
            Vector3 end01 = scoreMedallion01.transform.position;
            Vector3 end02 = scoreMedallion02.transform.position;
            Vector3 end03 = scoreMedallion03.transform.position;
            Vector3 end04 = scoreMedallion04.transform.position;

            float offset = 2.0f;
            Vector3 start01 = end01 + new Vector3( offset,  offset, 0);
            Vector3 start02 = end02 + new Vector3( offset, -offset, 0);
            Vector3 start03 = end03 + new Vector3(-offset, -offset, 0);
            Vector3 start04 = end04 + new Vector3(-offset,  offset, 0);

            SharedSounds.clank01.PlayDelayed(0.1f);
            scoreMedallion01.gameObject.SetActive(true);
            StartCoroutine(Util.Blend(0.15f, t => {
                scoreMedallion01.transform.position = Vector3.Lerp(start01, end01, Curve.OutQuad(t));
                scoreMedallion01.transform.localScale = Vector3.Lerp(Vector3.one * 2.0f, Vector3.one, Curve.OutQuad(t));
                SetOpacity(scoreMedallion01, Curve.InCube(t));
            }));
            yield return new WaitForSeconds(0.125f);

            SharedSounds.clank02.PlayDelayed(0.1f);
            scoreMedallion03.gameObject.SetActive(true);
            StartCoroutine(Util.Blend(0.15f, t => {
                scoreMedallion03.transform.position = Vector3.Lerp(start03, end03, Curve.OutQuad(t));
                scoreMedallion03.transform.localScale = Vector3.Lerp(Vector3.one * 2.0f, Vector3.one, Curve.OutQuad(t));
                SetOpacity(scoreMedallion03, Curve.InCube(t));
            }));
            yield return new WaitForSeconds(0.125f);

            SharedSounds.clank03.PlayDelayed(0.1f);
            scoreMedallion04.gameObject.SetActive(true);
            StartCoroutine(Util.Blend(0.15f, t => {
                scoreMedallion04.transform.position = Vector3.Lerp(start04, end04, Curve.OutQuad(t));
                scoreMedallion04.transform.localScale = Vector3.Lerp(Vector3.one * 2.0f, Vector3.one, Curve.OutQuad(t));
                SetOpacity(scoreMedallion04, Curve.InCube(t));
            }));
            yield return new WaitForSeconds(0.125f);

            SharedSounds.clank04.PlayDelayed(0.1f);
            scoreMedallion02.gameObject.SetActive(true);
            StartCoroutine(Util.Blend(0.15f, t => {
                scoreMedallion02.transform.position = Vector3.Lerp(start02, end02, Curve.OutQuad(t));
                scoreMedallion02.transform.localScale = Vector3.Lerp(Vector3.one * 2.0f, Vector3.one, Curve.OutQuad(t));
                SetOpacity(scoreMedallion02, Curve.InCube(t));
            }));
            yield return new WaitForSeconds(0.35f);

            SharedSounds.medallionPieceCollected.PlayDelayed(0.3f);

            scoreMedallion05.gameObject.SetActive(true);
            yield return StartCoroutine(Util.Blend(0.4f, t => {
                scoreMedallion05.transform.localScale = Vector3.Lerp(Vector3.one * 6.0f, Vector3.one, Curve.OutCube(t));
                SetOpacity(scoreMedallion05, t);
            }));

            SharedSounds.medallionCollected.Play(0.4f);
            scoreMedallion00.gameObject.SetActive(false);
            scoreMedallion01.gameObject.SetActive(false);
            scoreMedallion02.gameObject.SetActive(false);
            scoreMedallion03.gameObject.SetActive(false);
            scoreMedallion04.gameObject.SetActive(false);
            scoreMedallion05.gameObject.SetActive(false);
            scoreMedallionFull.gameObject.SetActive(true);
        }

        SharedSounds.pop.Play();
        StartCoroutine(Util.Blend(0.5f, t => {
            SetOpacity(scoreMedallionGlow, 1.0f - t);
        }));

        yield return StartCoroutine(Util.Blend(0.5f, t => {
            scoreMedallionCount.transform.localScale = new Vector3(1, Curve.InElastic(t), 1);
        }));

        yield return new WaitForSeconds(0.5f);

        // ANIMATE TOTAL SCORE
        StartCoroutine(Util.Blend(0.5f, t => {
            t = Curve.InElastic(t);
            scoreText.transform.localScale = new Vector3(t, t, t);
        }));

        float countLength = Mathf.Lerp(0.5f, 5.0f, Session.Points / 100000.0f);
        yield return StartCoroutine(Util.Blend(countLength, t => {
            scoreText.text = Mathf.RoundToInt(Session.Points * t).ToString();
        }));

        yield return new WaitForSeconds(0.3f);

        // FADE OUT OLD ELEMENTS
        StartCoroutine(Util.Blend(0.5f, t => {
            SetOpacity(scoreEggImage, Curve.InvInCube(t));
            SetOpacity(scoreEggCount, Curve.InvInCube(t));
            SetOpacity(scoreMedallion00, Curve.InvInCube(t));
            SetOpacity(scoreMedallionFull, Curve.InvInCube(t));
            SetOpacity(scoreMedallionCount, Curve.InvInCube(t));
        }));

        var titleStartPos = scoreTitle.transform.position;
        var titleFinishPos = scoreTitleFinalPosition.position;

        var scoreStartPos = scoreText.transform.position;
        var scoreFinishPos = scoreTextFinalPosition.position;

        yield return StartCoroutine(Util.Blend(1.0f, t => {
            scoreTitle.transform.position = Vector3.Lerp(titleStartPos, titleFinishPos, Curve.SmoothStepSteep(t));
            scoreText.transform.position = Vector3.Lerp(scoreStartPos, scoreFinishPos, Curve.SmoothStepSteep(t));
        }));

        yield return new WaitForSeconds(0.2f);

        scoreEggImage.gameObject.SetActive(false);
        scoreEggCount.gameObject.SetActive(false);
        scoreMedallion00.gameObject.SetActive(false);
        scoreMedallionFull.gameObject.SetActive(false);
        scoreMedallionCount.gameObject.SetActive(false);

        scoreNameMessage.gameObject.SetActive(true);
        nameInput.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        // SCALE IN DONE BUTTON
        SharedSounds.boing.Play();
        yield return StartCoroutine(Util.Blend(0.4f, t => {
            t = Curve.InElastic(t);
            doneButton.transform.localScale = new Vector3(t, t, t);
        }));
    }

    Coroutine damageAnimation;

    public static void DoDamageAnimation()
    {
        if(that.damageAnimation != null)
            that.StopCoroutine(that.damageAnimation);
        
        that.damageAnimation = that.StartCoroutine(that.doDamageAnimation());
    }

    IEnumerator doDamageAnimation()
    {
        damageOverlay.gameObject.SetActive(true);

        yield return Util.Blend(2.0f, t => {
            var c = Color.red;
            c.a = (1.0f - t) * 0.9f;
            damageOverlay.color = c;
        });

        damageOverlay.gameObject.SetActive(false);
        damageAnimation = null;
    }

    void Awake()
    {
        that = this;
        nameInput.characterLimit = ScoreService.MaxNameLength;
        
        if (PrefKeys.IsImmortal)
        {
            livesRemainingText.gameObject.SetActive(false);
            infinityText.gameObject.SetActive(true);
        }
        else
        {
            livesRemainingText.text = PlayerPrefs.GetInt(PrefKeys.LivesRemaining).ToString();
        }
    }

    void OnEnable() {
        Game.OnPauseChanged += OnPause;
    }

    void OnDisable() {
        Game.OnPauseChanged -= OnPause;
    }

    void OnPause(bool didPause, float duration) {
        paused = didPause;
        hud.SetActive(!didPause);
        pauseScreen.SetActive(didPause);
        optionButton.gameObject.SetActive(!didPause);
    }

    bool initializedPos = false;
    float lastPlayerY = 0;
    float playerSpeed = 0;

    void Update()
    {
        if(paused)
            return;

        if(Input.GetKeyUp(KeyCode.M))
            StartCoroutine(DoMedallionAnimation());
        
        eggCountText.text = Session.EggCount.ToString();
        pointCountText.text = Session.Points.ToString();
        medallionImage.sprite = medallionTextures[Session.MedallionPieceCount];
        medallionCountText.text = Session.MedallionCount.ToString();

        UpdateGlow(eggCountText, ref eggCountPhase);
        UpdateGlow(pointCountText, ref pointCountPhase);
        UpdateGlow(medallionCountText, ref medallionCountPhase);
        UpdateGlow(medallionGlow, ref medallionGlowPhase);
        UpdateGlow(eggGlow, ref eggGlowPhase);

        if (Game.that.player && Game.that.gameRunning)
        {
            var playerY = Game.that.player.transform.position.y;

            if (!initializedPos) {
                lastPlayerY = playerY;
                initializedPos = true;
            }

            float currentSpeed = 0;

            if (!Mathf.Approximately(Time.deltaTime, 0))
            {
                currentSpeed = (playerY - lastPlayerY) / Time.deltaTime;
            }

            lastPlayerY = playerY;

            playerSpeed = playerSpeed + (currentSpeed - playerSpeed) * Time.deltaTime * 5;

            float maxSpeed = 18.0f;
            speedBar.fillAmount = playerSpeed / (maxSpeed - 2.0f);
        }
        else
        {
            speedBar.fillAmount = 0;
        }
    }

    static void UpdateGlow(Text text, ref float phase)
    {
        var normal = Util.HexRGBA(0xD59D12FF);
        var glow = Util.HexRGBA(0xFFFFFFFF);
        text.color = Color.Lerp(normal, glow, phase * phase);
        phase = Mathf.Max(phase - Time.deltaTime, 0);
    }

    static void UpdateGlow(TMP_Text text, ref float phase)
    {
        var normal = Util.HexRGBA(0xD59D12FF);
        var glow = Util.HexRGBA(0xFFFFFFFF);
        text.color = Color.Lerp(normal, glow, phase * phase);
        phase = Mathf.Max(phase - Time.deltaTime, 0);
    }

    static void UpdateGlow(Image image, ref float phase)
    {
        var normal = Util.HexRGBA(0xFFDB0000);
        var glow = Util.HexRGBA(0xFFDB00FF);
        image.color = Color.Lerp(normal, glow, phase * phase);
        phase = Mathf.Max(phase - Time.deltaTime, 0);
    }

    static void SetOpacity(MaskableGraphic image, float opacity)
    {
        var c = image.color;
        c.a = opacity;
        image.color = c;
    }
}
