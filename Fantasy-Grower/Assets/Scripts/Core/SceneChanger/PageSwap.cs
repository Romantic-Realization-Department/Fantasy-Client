using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class PageSwap : SceneChangeAction
{
    [SerializeField]
    private Image _tipImage;

    [SerializeField]
    private TMP_Text _tipText;

    [System.Serializable]
    public struct Tip
    {
        public Sprite Image;

        [TextArea]
        public string Text;
    }

    [SerializeField]
    private Tip[] _tips;

    [SerializeField]
    private float _fadeDuration = 0.3f;

    [SerializeField]
    private float _intervalDuration = 1.2f;

    private Graphic _myPanel;

    protected override void Awake()
    {
        gameObject.SetActive(false);

        _myPanel = GetComponent<Graphic>();

        Color color = _myPanel.color;
        color.a = 0;
        _myPanel.color = color;
    }

    public override Tween BeforeChange()
    {
        gameObject.SetActive(true);

        Tip tip = _tips[Random.Range(0, _tips.Length)];

        _tipImage.sprite = tip.Image;
        _tipText.text = tip.Text;

        return _myPanel.DOFade(1, _fadeDuration);
    }

    public override Tween AfterChange()
    {
        Sequence sequence = DOTween.Sequence();

        sequence.AppendInterval(_intervalDuration);

        return sequence.Append(
            _myPanel.DOFade(0, _fadeDuration).OnComplete(() => gameObject.SetActive(false))
        );
    }
}
