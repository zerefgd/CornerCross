using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region  STARTING_VARIABLES

    public static GameManager Instance { get; private set; }

    [SerializeField]
    private TMP_Text _scoreText, _endScoreText,_highScoreText;

    private int score;

    [SerializeField]
    private Animator _scoreAnimator;

    [SerializeField]
    private AnimationClip _scoreClip;
 
    [SerializeField]
    private GameObject _endPanel;

    [SerializeField]
    private Image _soundImage;

    [SerializeField]
    private Sprite _activeSoundSprite, _inactiveSoundSprite;

    [SerializeField]
    private int _totalScoreTargets;

    [HideInInspector]
    public int currentTargetIndex;

    #endregion

    #region MONOBEHAVIOUR METHODS

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        AudioManager.Instance.AddButtonSound();

        StartCoroutine(IStartGame());
    }
    #endregion

    #region UI_METHODS
    public void GoToMainMenu()
    {
        SceneManager.LoadScene(Constants.DATA.MAIN_MENU_SCENE);
    }

    public void ReloadGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ToggleSound()
    {
        bool sound = (PlayerPrefs.HasKey(Constants.DATA.SETTINGS_SOUND) ? PlayerPrefs.GetInt(Constants.DATA.SETTINGS_SOUND)
            : 1) == 1;
        sound = !sound;
        PlayerPrefs.SetInt(Constants.DATA.SETTINGS_SOUND, sound ? 1 : 0);
        _soundImage.sprite = sound ? _activeSoundSprite : _inactiveSoundSprite;
        AudioManager.Instance.ToggleSound();
    }

    public static event Action<int> UpdateScoreColor;
    public void UpdateScore()
    {
        score++;
        _scoreText.text = score.ToString();
        _scoreAnimator.Play(_scoreClip.name, -1, 0f);

        //Fire UpdateScoreColorEvent
        int temp = UnityEngine.Random.Range(0, _totalScoreTargets);
        while (temp == currentTargetIndex)
        {
            temp = UnityEngine.Random.Range(0, _totalScoreTargets);
        }
        currentTargetIndex = temp;
        UpdateScoreColor?.Invoke(currentTargetIndex);
    }

    #endregion

    #region GAME_START_END ANIMATIONS

    public void EndGame()
    {
        StartCoroutine(GameOver());
    }

    [SerializeField] private Animator _highScoreAnimator;
    [SerializeField] private AnimationClip _highScoreClip;

    private IEnumerator GameOver()
    {
        hasGameEnded = true;
        _scoreText.gameObject.SetActive(false);

        yield return MoveCamera (new Vector3(_cameraStartPos.x,-_cameraStartPos.y,_cameraStartPos.z));

        _endPanel.SetActive(true);
        _endScoreText.text = score.ToString();

        bool sound = (PlayerPrefs.HasKey(Constants.DATA.SETTINGS_SOUND) ?
          PlayerPrefs.GetInt(Constants.DATA.SETTINGS_SOUND) : 1) == 1;
        _soundImage.sprite = sound ? _activeSoundSprite : _inactiveSoundSprite;

        int highScore = PlayerPrefs.HasKey(Constants.DATA.HIGH_SCORE) ? PlayerPrefs.GetInt(Constants.DATA.HIGH_SCORE) : 0;
        if (score > highScore)
        {
            _highScoreText.text = "NEW BEST";

            //Play HighScore Animation
            _highScoreAnimator.Play(_highScoreClip.name,-1,0f);

            highScore = score;
            PlayerPrefs.SetInt(Constants.DATA.HIGH_SCORE, highScore);
        }
        else
        {
            _highScoreText.text = "BEST " + highScore.ToString();
        }
    }


    [SerializeField]
    private Vector3 _cameraStartPos, _cameraEndPos;
    [SerializeField]
    private float _timeToMoveCamera;

    private bool hasGameEnded;
    public UnityAction GameStarted, GameEnded;

    private IEnumerator IStartGame()
    {
        hasGameEnded = false;
        Camera.main.transform.position = _cameraStartPos;
        _scoreText.gameObject.SetActive(false);
        yield return MoveCamera(_cameraEndPos);

        //Start Event Conditons
        currentTargetIndex = 0;

        int temp = UnityEngine.Random.Range(0, _totalScoreTargets);
        while (temp == currentTargetIndex)
        {
            temp = UnityEngine.Random.Range(0, _totalScoreTargets);
        }
        currentTargetIndex = temp;

        UpdateScoreColor?.Invoke(currentTargetIndex);

        //Score
        _scoreText.gameObject.SetActive(true);
        score = 0;
        _scoreText.text = score.ToString();
        _scoreAnimator.Play(_scoreClip.name, -1, 0f);

        StartCoroutine(ISpawnObstacles());
        GameStarted?.Invoke();
    }

    private IEnumerator MoveCamera(Vector3 cameraPos)
    {
        Transform cameraTransform = Camera.main.transform;
        float timeElapsed = 0f;
        Vector3 startPos = cameraTransform.position;
        Vector3 offset = cameraPos - startPos;
        float speed = 1 / _timeToMoveCamera;
        while(timeElapsed < 1f)
        {
            timeElapsed += speed * Time.deltaTime;
            cameraTransform.position = startPos + timeElapsed * offset;
            yield return null;
        }
        cameraTransform.position = cameraPos;
    }

    #endregion

    #region OBSTACLE_SPAWINING
    [SerializeField]
    private GameObject _obstaclePrefab;

    [SerializeField]
    private List<Vector3> _obstacleSpawnPos;

    [SerializeField]
    private float _obstacleSpawnTime;

    private IEnumerator ISpawnObstacles()
    {
        var timeInterval = new WaitForSeconds(_obstacleSpawnTime);
        Vector3 spawnPos = _obstacleSpawnPos[UnityEngine.Random.Range(0,_obstacleSpawnPos.Count)];
        while(!hasGameEnded)
        {
            Instantiate(_obstaclePrefab, spawnPos, Quaternion.identity);
            spawnPos = _obstacleSpawnPos[UnityEngine.Random.Range(0, _obstacleSpawnPos.Count)];
            yield return timeInterval;
        }
    }
    #endregion
}
