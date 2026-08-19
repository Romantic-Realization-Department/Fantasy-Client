using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FadeIn : SceneChangeAction
{
    [SerializeField]
    private float _fadeDuration = 0.5f;

    private Image _image;

    protected override void Awake()
    {
        _image = GetComponent<Image>();

        gameObject.SetActive(false);
        Color color = _image.color;
        color.a = 0;
        _image.color = color;
    }

    public override Tween BeforeChange()
    {
        gameObject.SetActive(true);
        return _image.DOFade(1, _fadeDuration);
    }

    public override Tween AfterChange()
    {
        return _image.DOFade(0, _fadeDuration).OnComplete(() => gameObject.SetActive(false));
    }
}
