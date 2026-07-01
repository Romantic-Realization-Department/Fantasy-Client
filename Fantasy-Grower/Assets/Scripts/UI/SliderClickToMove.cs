using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SliderClickToMove : MonoBehaviour, IPointerDownHandler
{
    private Slider _slider;
    private RectTransform _sliderRect;

    void Awake()
    {
        _slider = GetComponent<Slider>();
        _sliderRect = _slider.GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 클릭한 화면 좌표를 Slider의 Local UI 좌표로 변환
        if (
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _sliderRect,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint
            )
        )
        {
            // Slider의 가로/세로 방향(Direction)에 따라 비율 계산
            float handleRatio;
            if (
                _slider.direction == Slider.Direction.LeftToRight
                || _slider.direction == Slider.Direction.RightToLeft
            )
            {
                // 가로 Slider: 클릭 위치의 X 좌표 기준 비율 계산
                // localPoint.x는 중심점이 기준이므로 Left(-width/2)부터 Right(width/2)까지의 값을 0~1로 정규화
                float minX = _sliderRect.rect.xMin;
                float maxX = _sliderRect.rect.xMax;
                handleRatio = Mathf.InverseLerp(minX, maxX, localPoint.x);

                if (_slider.direction == Slider.Direction.RightToLeft)
                    handleRatio = 1f - handleRatio;
            }
            else
            {
                // 세로 Slider: 클릭 위치의 Y 좌표 기준 비율 계산
                float minY = _sliderRect.rect.yMin;
                float maxY = _sliderRect.rect.yMax;
                handleRatio = Mathf.InverseLerp(minY, maxY, localPoint.y);

                if (_slider.direction == Slider.Direction.TopToBottom)
                    handleRatio = 1f - handleRatio;
            }

            // 계산된 비율을 Slider의 전체 범위(MinValue ~ MaxValue)에 맞춰 값 적용
            float newValue = Mathf.Lerp(_slider.minValue, _slider.maxValue, handleRatio);

            // 정수형(Whole Numbers) Slider인 경우 반올림 처리
            if (_slider.wholeNumbers)
            {
                newValue = Mathf.Round(newValue);
            }

            _slider.value = newValue;
        }
    }
}
