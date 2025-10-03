using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CastleHPScript : MonoBehaviour
{
    [SerializeField] private Vector3 _offset;
    public CastleHP CastleHP;
    public GameObject Castle;
    public Image ColorSlider;

    private RectTransform _thisRectTransform;
    private Slider _thisSlider;

    private void Start()
    {
        _thisRectTransform = GetComponent<RectTransform>();
        _thisSlider = GetComponent<Slider>();
    }

    void Update()
    {
        _thisRectTransform.position = Camera.main.WorldToScreenPoint(Castle.transform.position + _offset);
        _thisSlider.value = CastleHP.HP / CastleHP.MaxHP;
    }
}
