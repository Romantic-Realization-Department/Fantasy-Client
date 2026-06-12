using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DungeonSelectButton : MonoBehaviour
{
    [field: SerializeField]
    public Button SelectButton { get; private set; }

    [field: SerializeField]
    public GameObject Filter { get; private set; }

    [SerializeField]
    private SceneNameRef _sceneNameRef;

    [SerializeField]
    private Vector2 _selectedSize = new(1200f, 400f);

    [SerializeField]
    private float _tweenDuration = 0.3f;

    private RectTransform _rectTransform;
    private Tweener _tweener;

    private Vector2 _originSize;

    private void Awake()
    {
        _rectTransform = transform as RectTransform;

        _originSize = _rectTransform.sizeDelta;
    }

    public void Select()
    {
        if (_rectTransform.sizeDelta == _selectedSize)
            return;

        Filter.SetActive(false);

        _tweener?.Kill();

        _tweener = _rectTransform.DOSizeDelta(_selectedSize, _tweenDuration);
    }

    public void Return()
    {
        if (_rectTransform.sizeDelta == _originSize)
            return;

        Filter.SetActive(true);

        _tweener?.Kill();

        _tweener = _rectTransform.DOSizeDelta(_originSize, _tweenDuration);
    }

    public void MovingScene()
    {
        SceneChanger.LoadScene(_sceneNameRef.SceneName, SceneChangeType.PageSwap);
    }

    private void OnValidate()
    {
        if (_sceneNameRef == null)
        {
            Debug.Log("SceneNameRef가 할당되지 않았습니다!!!", this);
        }

        if (SelectButton == null)
        {
            Debug.Log("SelectButton가 할당되지 않았습니다!!!", this);
        }

        if (Filter == null)
        {
            Debug.Log("Filter가 할당되지 않았습니다!!!", this);
        }
    }
}
