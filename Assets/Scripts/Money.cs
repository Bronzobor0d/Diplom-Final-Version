using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Money : MonoBehaviour
{
    [SerializeField] private Mine _mine;
    [SerializeField] private Text _countText;

    public int Count;
    public bool IsStart;

    private bool _isTick;
    private int _coinInTick;

    #region MONO

    public void Awake()
    {
        SetTickMoney();
    }

    #endregion

    #region BODY

    void Update()
    {
        _countText.text = Count.ToString();
        if (!_isTick && IsStart)
        {
            StartCoroutine(tick());
        }
    }

    #endregion

    #region CALLBACKS

    /// <summary>
    ///  ��������� ���������� ����� � �������.
    /// </summary>
    public void SetTickMoney()
    {
        _coinInTick = _mine.GetTickMoney();
    }

    /// <summary>
    ///  ���������� ���������� �����.
    /// </summary>
    public void CoinPlus(int plusCoin)
    {
        Count += plusCoin;
    }

    /// <summary>
    ///  ���������� ���������� �����.
    /// </summary>
    public void CoinMinus(int minusCoin)
    {
        Count -= minusCoin;
    }

    /// <summary>
    ///  ������������ ���������� ����� � ����������� �� ������ �����.
    /// </summary>
    IEnumerator tick()
    {
        _isTick = true;
        yield return new WaitForSeconds(1);
        CoinPlus(_coinInTick);
        _isTick = false;
    }

    #endregion
}
