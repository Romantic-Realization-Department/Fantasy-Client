using System.Collections.Generic;
using UnityEngine;

public enum GoodsType
{
    Gold,
    XP,
    SP,
    UpgradeScroll,
    Mithril,
    Level,
}

public class GoodsManager : MonoBehaviour
{
    private static GoodsManager _instance;

    /// <summary>
    /// 싱글톤 인스턴스
    /// </summary>
    public static GoodsManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<GoodsManager>();
                if (_instance == null)
                {
                    Debug.LogError("GoodsManager instance not found in the scene!");
                }
            }
            return _instance;
        }
    }

    [System.Serializable]
    private struct Goods
    {
        public GoodsType key;
        public SO_Goods value;
    }

    [SerializeField]
    private Goods[] goods; // 인스펙터에서 GoodsType과 SO_Goods를 매핑하여 설정할 수 있도록 배열로 선언

    private readonly Dictionary<GoodsType, SO_Goods> _goodsDictionary = new(); // GoodsType과 SO_Goods를 매핑하여 빠르게 접근할 수 있도록 딕셔너리로 변환

    private void Awake()
    {
        // 싱글톤 패턴 구현: 이미 인스턴스가 존재하면 자신을 파괴하고, 그렇지 않으면 인스턴스로 설정
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject); // 씬이 변경되어도 파괴되지 않도록 설정

        _instance = this;

        foreach (Goods good in goods)
            _goodsDictionary[good.key] = good.value;
    }

    /// <summary>
    /// 자원 클래스를 GoodsType을 통해 가져오는 메서드
    /// </summary>
    /// <param name="type">가져올 자원 타입</param>
    /// <returns>type을 통해 검색된 자원 클래스(없을 시 null반환)</returns>
    public SO_Goods GetGoods(GoodsType type)
    {
        if (_goodsDictionary.TryGetValue(type, out SO_Goods good))
            return good;
        Debug.LogError($"Goods of type {type} not found!");
        return null;
    }
}
