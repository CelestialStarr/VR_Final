using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GestureGameManager : MonoBehaviour
{
    public static GestureGameManager Instance;

    [Header("UI References")]
    public GameObject canvasRoot;    // 包含 Slider 和 Text 的根物体
    public Slider timerSlider;
    public Text progressText;

    [Header("Gesture Mapping")]
    public List<GestureObjectMap> gestureLibrary;

    [Header("Visual Settings")]
    [Tooltip("手势图标默认的半透明黑色")]
    public Color defaultGestureColor = new Color(0, 0, 0, 0.5f);
    public Color successColor = Color.green;
    public Color failureColor = Color.red;

    [Header("Game Settings")]
    public float timeLimitPerGesture = 3.0f; 
    public float feedbackDelay = 0.4f; 

    [System.Serializable]
    public struct GestureObjectMap
    {
        public string gestureName;       // 必须与识别器传递的字符串一致
        public GameObject gestureObject; // 挂载了 Image 的手势图标物体
    }

    private string[] availableGestureNames = { "ThumbsUp", "ThumbsDown", "Shaka", "Fist", "OK", "Grab", "Yeah", "Gun", "Spider" };
    private List<string> _requiredSequence = new List<string>();
    private int _currentIndex = 0;
    private bool _isValidating = false;
    private bool _isTransitioning = false;
    private float _remainingTime;
    private StealableObject _currentObject;
    private Image _activeImage;

    void Awake()
    {
        Instance = this;
        // 游戏启动时确保 UI 是隐藏的
        if (canvasRoot) canvasRoot.SetActive(false);
    }

    void Update()
    {
        // 只有在验证中且非动画过渡状态才计时
        if (!_isValidating || _isTransitioning) return;

        _remainingTime -= Time.deltaTime;
        if (timerSlider) timerSlider.value = _remainingTime;

        if (_remainingTime <= 0 && !_isTransitioning)
        {
            _isTransitioning = true;
            _isValidating = false;
            StartCoroutine(HandleFeedback(false));
        }

    }

    // 由 StealableObject 拿起时触发
    public void StartValidation(StealableObject obj, int difficulty)
    {
        // 清理上一次可能的残留状态
        StopAllCoroutines();
        _isTransitioning = false;

        _currentObject = obj;
        _currentIndex = 0;

        // 拿起物品，激活 UI
        if (canvasRoot) canvasRoot.SetActive(true);

        // 初始化：关闭所有图标显示
        foreach (var map in gestureLibrary)
        {
            if (map.gestureObject) map.gestureObject.SetActive(false);
        }

        GenerateRandomSequence(difficulty);
        _isValidating = true;
        ShowCurrentGesture();
    }

    private void GenerateRandomSequence(int count)
    {
        _requiredSequence.Clear();
        string lastGesture = "";

        for (int i = 0; i < count; i++)
        {
            string nextGesture;
            do
            {
                nextGesture = gestureLibrary[Random.Range(0, gestureLibrary.Count)].gestureName;
            } while (nextGesture == lastGesture); // 确保不与上一个手势重复

            _requiredSequence.Add(nextGesture);
            lastGesture = nextGesture;
        }
    }


    // 手势识别器触发此方法
    public void OnGestureDetected(string gestureName)
    {
        if (!_isValidating || _isTransitioning) return;

        if (_requiredSequence[_currentIndex] == gestureName)
            StartCoroutine(HandleCorrect());
        else
            StartCoroutine(HandleFeedback(false));
    }

    private IEnumerator HandleCorrect()
    {
        _isTransitioning = true;

        if (_activeImage) _activeImage.color = successColor;

        yield return new WaitForSeconds(feedbackDelay);

        _currentIndex++;
        if (_currentIndex >= _requiredSequence.Count)
        {
            CompleteChallenge(true);
        }
        else
        {
            _isTransitioning = false;
            ShowCurrentGesture();
        }
    }

    private IEnumerator HandleFeedback(bool success)
    {
        _isTransitioning = true;
        _isValidating = false; // 停止计时

        if (!success && _activeImage) _activeImage.color = failureColor;

        yield return new WaitForSeconds(feedbackDelay);
        CompleteChallenge(success);
    }

    private void ShowCurrentGesture()
    {
        // 重置当前手势的独立倒计时
        _remainingTime = timeLimitPerGesture;
        if (timerSlider)
        {
            timerSlider.maxValue = timeLimitPerGesture;
            timerSlider.value = timeLimitPerGesture;
        }

        // 隐藏上一个图标
        if (_activeImage) _activeImage.gameObject.SetActive(false);

        string targetName = _requiredSequence[_currentIndex];
        var targetMap = gestureLibrary.Find(x => x.gestureName == targetName);

        if (targetMap.gestureObject != null)
        {
            targetMap.gestureObject.SetActive(true);
            _activeImage = targetMap.gestureObject.GetComponent<Image>();

            // 应用你设定的半透明黑色
            if (_activeImage) _activeImage.color = defaultGestureColor;
        }

        if (progressText) progressText.text = $"{_currentIndex + 1} / {_requiredSequence.Count}";
    }

    public void CompleteChallenge(bool success)
    {
        _isValidating = false;
        _isTransitioning = false;

        // 完成挑战（无论胜负），关闭 UI
        if (canvasRoot) canvasRoot.SetActive(false);

        if (success) _currentObject.HandleSuccess();
        else _currentObject.HandleFailure();
    }
}