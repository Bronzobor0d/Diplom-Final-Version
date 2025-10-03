using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerTrigger : MonoBehaviour
{
    [SerializeField] private Tower _tower;

    private bool _warriorlock;
    private Warrior _curTarget;
    private Warrior _secondTarget;

    #region BODY

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Warrior") && !_warriorlock && !other.GetComponent<Warrior>().InvulnerabilityToTowers.Contains(_tower.Type))
        {
            _tower.Target = other.gameObject.transform;
            _curTarget = other.gameObject.GetComponent<Warrior>();
            _warriorlock = true;
        }
        else if (other.CompareTag("Warrior") && _warriorlock && !other.GetComponent<Warrior>().InvulnerabilityToTowers.Contains(_tower.Type)
            && _secondTarget == null && other.GetComponent<Warrior>() != _curTarget)
        {
            _secondTarget = other.gameObject.GetComponent<Warrior>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Warrior") && other.gameObject.GetComponent<Warrior>() == _curTarget && _secondTarget != null)
        {
            _tower.Target = _secondTarget.transform;
            _curTarget = _secondTarget;
            _secondTarget = null;
        }
        else if (other.CompareTag("Warrior") && other.gameObject.GetComponent<Warrior>() == _curTarget)
        {
            _warriorlock = false;
            _tower.Target = null;
            _curTarget = null;
        }
        if (other.CompareTag("Warrior") && other.gameObject.GetComponent<Warrior>() == _secondTarget)
        {
            _secondTarget = null;
        }
    }

    void Update()
    {
        if (_curTarget == null)
        {
            _warriorlock = false;
        }
        else if (_curTarget.HP <= 0 && _secondTarget != null)
        {
            _tower.Target = _secondTarget.transform;
            _curTarget = _secondTarget;
            _secondTarget = null;
        }
        else if (_curTarget.HP <= 0)
        {
            _warriorlock = false;
            _tower.Target = null;
            _curTarget = null;
        }
    }

    #endregion
}
