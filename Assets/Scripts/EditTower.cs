using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditTower : MonoBehaviour
{
    [SerializeField] private Text _removeCoinPlusText;

    public Tower TowerObj;
    public Vector3 Offset;
    public Money Money;
    public Text LevelTowerText;
    public BuildingTowers BuildingTowers;

    private RectTransform _thisRectTransform;
    private Camera _mainCamera;
    private bool _monoUpdate;

    #region MONO

    private void Awake()
    {
        _thisRectTransform = GetComponent<RectTransform>();
        _mainCamera = Camera.main;
    }

    #endregion

    #region BODY

    void Update()
    {
        if (TowerObj == null)
        {
            Destroy(gameObject);
        }
        else
        {
            _thisRectTransform.position = _mainCamera.WorldToScreenPoint(TowerObj.transform.position + Offset);
        }
        if (!_monoUpdate)
        {
            LevelTowerText.text = TowerObj.Level.ToString();
            _removeCoinPlusText.text = "+" + (TowerObj.RemoveCost).ToString();
            _monoUpdate = true;
        }
    }

    #endregion
}
