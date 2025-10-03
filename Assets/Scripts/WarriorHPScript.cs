using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Internal;
using UnityEngine.UI;

public class WarriorHPScript : MonoBehaviour
{
    public Vector3 Offset;
    public Warrior Warrior;
    public Image ColorSlider;

    private Slider _slider;
    private RectTransform _rectTransform;

    #region MONO

    private void Awake()
    {
        _slider = GetComponent<Slider>();
        _rectTransform = GetComponent<RectTransform>();
    }

    #endregion

    #region BODY

    private void Update()
    {
        if (Warrior == null)
            return;
        if (Warrior.HP <= 0)
        {
            Destroy(gameObject);
        }
        _rectTransform.position = Camera.main.WorldToScreenPoint(Warrior.transform.GetChild(0).transform.position + Offset);
        _slider.value = Warrior.HP / Warrior.MaxHP;
    }

    #endregion

    #region CALLBACKS

    /// <summary>
    ///  Вычетает из здоровья моба количество передающегося урона
    /// </summary>
    public void Damage(float DMGCount)
    {
        Warrior.HP -= DMGCount;
    }

    public void Heal(float HealCount)
    {
        if (Warrior.HP + HealCount > Warrior.MaxHP)
            Warrior.HP = Warrior.MaxHP;
        else
            Warrior.HP += HealCount;
    }

    #endregion
}
