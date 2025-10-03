using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Mine : MonoBehaviour
{
    [SerializeField] private int _levelTwoCost;
    [SerializeField] private int _levelThreeCost;
    [SerializeField] private int _levelFourCost;
    [SerializeField] private int _levelFiveCost;
    [SerializeField] private int _levelSixCost;
    [SerializeField] private int _tickMoneyOne;
    [SerializeField] private int _tickMoneyTwo;
    [SerializeField] private int _tickMoneyThree;
    [SerializeField] private int _tickMoneyFour;
    [SerializeField] private int _tickMoneyFive;
    [SerializeField] private int _tickMoneySix;
    [SerializeField] private Experience _exp;
    [SerializeField] private Money _money;
    [SerializeField] private Text _costText;
    [SerializeField] private Text _text;
    [SerializeField] private Color _fullLevelColor;

    private Button _thisButton;
    private bool _isFullLevel;
    private int _level = 1;

    #region MONO

    private void Awake()
    {
        _thisButton = GetComponent<Button>();
    }

    #endregion

    #region BODY

    void Update()
    {
        if (_level == 6 && !_isFullLevel)
        {
            GetComponent<Image>().color = _fullLevelColor;
            _thisButton.interactable = false;
            _text.color = Color.black;
            _costText.text = "";
            _isFullLevel = true;
        }
        else if (!_isFullLevel)
        {
            if (_exp.Count < GetUpgradeMineCost())
            {
                _thisButton.interactable = false;
            }
            else
            {
                _thisButton.interactable = true;
            }
            _costText.text = GetUpgradeMineCost().ToString();
        }
    }

    #endregion

    #region CALLBACKS

    /// <summary>
    ///  Улучшение шахты.
    /// </summary>
    public void UpgradeMine()
    {
        _exp.Count -= GetUpgradeMineCost();
        _level++;
        _money.SetTickMoney();
    }

    /// <summary>
    ///  Получение стоимости улучшения шахты.
    /// </summary>
    private int GetUpgradeMineCost()
    {
        switch (_level)
        {
            case 1:
                return _levelTwoCost;
            case 2:
                return _levelThreeCost;
            case 3:
                return _levelFourCost;
            case 4:
                return _levelFiveCost;
            case 5:
                return _levelSixCost;
        }
        return 0;
    }

    /// <summary>
    ///  Получение количества монет в секунду.
    /// </summary>
    public int GetTickMoney()
    {
        switch (_level)
        {
            case 1:
                return _tickMoneyOne;
            case 2:
                return _tickMoneyTwo;
            case 3:
                return _tickMoneyThree;
            case 4:
                return _tickMoneyFour;
            case 5:
                return _tickMoneyFive;
            case 6:
                return _tickMoneySix;
        }
        return 0;
    }

    #endregion
}
