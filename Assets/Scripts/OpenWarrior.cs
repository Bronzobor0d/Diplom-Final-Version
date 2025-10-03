using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpenWarrior : MonoBehaviour
{
    [SerializeField] private Text _warriorLevelText;
    [SerializeField] private List<Warrior> _warriors = new List<Warrior>();

    public int WarriorLevel;
    public HiringWarriors HiringWarriors;

    #region BODY

    void Update()
    {
        if (!HiringWarriors.IsOpen)
        {
            GetComponent<Button>().interactable = false;
            _warriorLevelText.text = "";
        }
        else
        {
            GetComponent<Button>().interactable = true;
            _warriorLevelText.text = WarriorLevel.ToString();
        }
    }

    #endregion

    #region CALLBACKS

    /// <summary>
    ///  Установка имени и параметров кнопкам панели улучшения воина.
    /// </summary>
    public void SetNameWarriorLevels(PanelControl upgradePanelControl)
    {
        int expCost = 0;
        for (int i = 1; i < 6; i++)
        {
            UpgradeWarriors btnUpgrade = upgradePanelControl.transform.GetChild(i).GetComponent<UpgradeWarriors>();
            btnUpgrade.OpenWarrior = this;
            btnUpgrade.Level = i;
            btnUpgrade.Warrior = _warriors[i - 1];
            if (i > WarriorLevel)
            {
                btnUpgrade.EXPCost = _warriors[i - 1].EXPCost + expCost;
                btnUpgrade.IsOpen = false;
                expCost += _warriors[i - 1].EXPCost;
            }
            else
                btnUpgrade.IsOpen = true;
            btnUpgrade.transform.GetChild(0).GetComponent<Text>().text = HiringWarriors.Warrior.Name;
        }
    }

    /// <summary>
    ///  Установка уровня воина после его улучшения.
    /// </summary>
    public void SetWarriorLevel(int level)
    {
        WarriorLevel = level;
    }

    #endregion
}
