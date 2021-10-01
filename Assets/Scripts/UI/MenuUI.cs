using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    public GameObject scoreItemPrefab;
    public GameObject main;
    public GameObject scores;
    public GameObject help;
    public GameObject loading;
    public RectTransform scoreContent;
    public RectTransform scoreScrollView;
    public ScrollRect scoreScrollRect;

    public static int mostRecentScoreID = -1;
    Coroutine showScoreRoutine = null;

    void Awake()
    {
        if(!PlayerPrefs.HasKey(PrefKeys.LivesRemaining))
            PlayerPrefs.SetInt(PrefKeys.LivesRemaining, 3);
    }

    void Start()
    {
        //scoreID = ScoreService.PutScore("Nick", 1000);

        Music.that.PlayTrack(AudioTrack.NatureAmbience);

        if(mostRecentScoreID != -1)
            OnScoresPressed();
    }

    public void OnPlayPressed()
    {
        SharedSounds.button.Play();
        Music.that.Stop();
        main.SetActive(false);
        loading.SetActive(true);
        SceneManager.LoadSceneAsync("game");
    }

    public void OnScoresPressed()
    {
        SharedSounds.button.Play();
        main.SetActive(false);
        scores.SetActive(true);
        ShowAllScores();
    }
    
    void ShowAllScores()
    {
        while (scoreContent.childCount > 0)
        {
            var c = scoreContent.GetChild(0);
            c.SetParent(null);
            Destroy(c.gameObject);
        }

        var highScores = ScoreService.GetScores();
        highScores.Sort((a, b) => b.score.CompareTo(a.score));

        ScoreItem scoreItem = null;

        for (int i = 0; i < highScores.Count; ++i)
        {
            var score = highScores[i];

            var item = Instantiate(scoreItemPrefab).GetComponent<ScoreItem>();
            item.id = score.id;
            item.place.text = (i + 1).ToString();
            item.username.text = score.name;
            item.score.text = score.score.ToString();
            item.transform.SetParent(scoreContent, false);
            item.menuUI = this;

            if (score.id == mostRecentScoreID) {
                scoreItem = item;
            }
        }

        mostRecentScoreID = -1;

        if (scoreItem != null)
        {
            scoreItem.background.enabled = true;
            showScoreRoutine = StartCoroutine(ScrollToScore(scoreItem));
        }
    }

    IEnumerator ScrollToScore(ScoreItem scoreItem)
    {
        yield return null;
        yield return null;

        var scoreItemTransform = scoreItem.transform as RectTransform;

        var rc = Util.RectRelativeTo(scoreItemTransform, scoreScrollView);
        if (rc.yMin < scoreScrollView.rect.yMin)
        {
            float diff = scoreScrollView.rect.yMin - rc.yMin + scoreScrollView.rect.height / 2;
            float n = 1.0f - diff / (scoreContent.rect.height - scoreScrollView.rect.height);
            n = Mathf.Clamp01(n);

            yield return StartCoroutine(Util.Blend(1.0f, t => {
                scoreScrollRect.verticalNormalizedPosition = Mathf.Lerp(1, n, Curve.InCube(t));
            }));
        }

        showScoreRoutine = null;
    }

    public void OnHelpPressed()
    {
        SharedSounds.button.Play();
        main.SetActive(false);
        help.SetActive(true);
    }

    public void OnExitPressed()
    {
        SharedSounds.button.Play();
        Application.Quit();
    }

    public void OnTitlePressed()
    {
        SharedSounds.button.Play();
        main.SetActive(false);
    }

    public void OnBackPressed()
    {
        SharedSounds.button.Play();
        main.SetActive(true);
        scores.SetActive(false);
        help.SetActive(false);

        if(showScoreRoutine != null)
        {
            StopCoroutine(showScoreRoutine);
            showScoreRoutine = null;
        }
    }
}
