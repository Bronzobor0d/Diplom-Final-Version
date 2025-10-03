using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HiringWarriors : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private SendWarriors _sendWarriors;
    [SerializeField] private Experience _exp;
    [SerializeField] private Color _colorEXP;
    [SerializeField] private Color _colorText;
    [SerializeField] private Text _costWarrior;

    public bool IsOpen;
    public int CountWarrior;
    public Text CountEnemeyText;
    public Warrior Warrior;

    private Button _thisButton;

    #region MONO

    private void Awake()
    {
        if (!IsOpen)
        {
            CountEnemeyText.color = _colorEXP;
        }
        CountEnemeyText.text = Warrior.EXPCost.ToString();
        _thisButton = GetComponent<Button>();
    }

    #endregion

    #region BODY

    void Update()
    {
        if (IsOpen)
        {
            CountEnemeyText.text = CountWarrior.ToString();
            _thisButton.interactable = true;
            _costWarrior.text = Warrior.Cost.ToString();
        }
        else
        {
            if (_exp.Count < Warrior.EXPCost)
                _thisButton.interactable = false;
            else
                _thisButton.interactable = true;
        }
    }

    #endregion

    #region CALLBACKS

    /// <summary>
    ///  Увеличение или уменьшение количества мобов для отправки противнику.
    /// </summary>
    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (!IsOpen && _exp.Count >= Warrior.EXPCost)
        {
            _exp.Count -= Warrior.EXPCost;
            CountEnemeyText.color = _colorText;
            IsOpen = true;
        }
        else if (IsOpen)
        {
            //Use this to tell when the user left-clicks on the Button
            if (pointerEventData.button == PointerEventData.InputButton.Left && Input.GetKey(KeyCode.LeftShift)
                && _sendWarriors.CountAllWarrior < 12 && _thisButton.IsInteractable())
            {
                for (int i = 0; i < 12 - _sendWarriors.CountAllWarrior; i++)
                {
                    CountWarrior++;
                    _sendWarriors.CostArmy += Warrior.Cost;
                    _sendWarriors.Warriors.Add(Warrior.name + CountWarrior, Warrior);
                }
                _sendWarriors.CountAllWarrior = 12;
            }
            //Use this to tell when the user right-clicks on the Button
            else if (pointerEventData.button == PointerEventData.InputButton.Right 
                && Input.GetKey(KeyCode.LeftShift) && CountWarrior > 0 && _thisButton.IsInteractable())
            {
                int pasteCountWarrior = CountWarrior;
                for (int i = 0; i < pasteCountWarrior; i++)
                {
                    _sendWarriors.Warriors.Remove(Warrior.name + CountWarrior);
                    CountWarrior--;
                    _sendWarriors.CostArmy -= Warrior.Cost;
                }
                _sendWarriors.CountAllWarrior -= pasteCountWarrior;
            }
            else if (pointerEventData.button == PointerEventData.InputButton.Left 
                && _sendWarriors.CountAllWarrior < 12 && _thisButton.IsInteractable())
            {
                CountWarrior++;
                _sendWarriors.CostArmy += Warrior.Cost;
                _sendWarriors.Warriors.Add(Warrior.name + CountWarrior, Warrior);
                _sendWarriors.CountAllWarrior++;
            }
            else if (pointerEventData.button == PointerEventData.InputButton.Right 
                && CountWarrior > 0 && _thisButton.IsInteractable())
            {
                _sendWarriors.Warriors.Remove(Warrior.name + CountWarrior);
                CountWarrior--;
                _sendWarriors.CostArmy -= Warrior.Cost;
                _sendWarriors.CountAllWarrior--;
            }
        }
    }

    /// <summary>
    ///  Удаляет всех воинов предыдущего уровня.
    /// </summary>
    public int RemoveOldHiringWarriors()
    {
        int pasteCountWarrior = CountWarrior;
        for (int i = 0; i < pasteCountWarrior; i++)
        {
            _sendWarriors.Warriors.Remove(Warrior.name + CountWarrior);
            CountWarrior--;
            _sendWarriors.CostArmy -= Warrior.Cost;
        }
        _sendWarriors.CountAllWarrior -= pasteCountWarrior;
        return pasteCountWarrior;
    }

    /// <summary>
    ///  Добавляет столько же воинов текущего уровня.
    /// </summary>
    public void AddNewHiringWarriors(int pasteCountWarrior)
    {
        for (int i = 0; i < pasteCountWarrior; i++)
        {
            CountWarrior++;
            _sendWarriors.CostArmy += Warrior.Cost;
            _sendWarriors.Warriors.Add(Warrior.name + CountWarrior, Warrior);
        }
        _sendWarriors.CountAllWarrior += pasteCountWarrior;
    }

    #endregion
}
