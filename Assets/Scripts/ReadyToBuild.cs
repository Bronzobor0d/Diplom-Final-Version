using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReadyToBuild : MonoBehaviour
{
    [SerializeField] private Tower _objectBuild;
    [SerializeField] private Money _money;
    [SerializeField] private Text _costTower;

    private Button _thisButton;

    #region MONO

    private void Awake()
    {
        _thisButton = GetComponent<Button>();
    }

    #endregion

    #region BODY

    void Update()
    {
        _costTower.text = _objectBuild.Cost.ToString();
        if (_money.Count < _objectBuild.Cost)
        {
            _thisButton.interactable = false;
        }
        else
        {
            _thisButton.interactable = true;
        }
    }

    #endregion
}
