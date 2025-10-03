using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeTower : MonoBehaviour
{
    [SerializeField] private EditTower _parent;
    [SerializeField] private Text _cost;
    [SerializeField] private Transform _remove;

    private Button _thisButton;
    private bool _fullLevel;

    #region MONO

    private void Awake()
    {
        _thisButton = GetComponent<Button>();
    }

    #endregion

    #region BODY

    void Update()
    {
        if (_parent.TowerObj.NextLevelTower == null && !_fullLevel)
        {
            gameObject.SetActive(false);
            _remove.localPosition = new Vector3(7.629395e-06f, _remove.localPosition.y, _remove.localPosition.z);
            _fullLevel = true;
        }
        else if (!_fullLevel)
        {
            if (_parent.Money.Count < _parent.TowerObj.NextLevelTower.Cost)
            {
                _thisButton.interactable = false;
            }
            else
            {
                _thisButton.interactable = true;
            }
            _cost.text = _parent.TowerObj.NextLevelTower.Cost.ToString();
        }
    }

    #endregion
}
