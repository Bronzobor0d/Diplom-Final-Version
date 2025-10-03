using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlchemistZoneTrigger : MonoBehaviour
{
    [SerializeField] private Warrior _warrior;

    private List<Warrior> _healWarriors = new List<Warrior>();

    private void Update()
    {
        List<Warrior> nullEnemies = new List<Warrior>();
        foreach (var healWarrior in _healWarriors)
        {
            if (healWarrior == null)
                nullEnemies.Add(healWarrior);
        }
        foreach (var nullWarrior in nullEnemies)
        {
            _healWarriors.Remove(nullWarrior);
        }
        if (!_warrior.isReadyHeal && _healWarriors.Count > 0)
        {
            _warrior.isReadyHeal = true;
        }
        else if (_healWarriors.Count <= 0)
        {
            _warrior.isReadyHeal = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Warrior warrior = other.GetComponent<Warrior>();
        if (warrior != null && warrior.HealCount <= 0)
            _healWarriors.Add(other.GetComponent<Warrior>());
    }

    private void OnTriggerExit(Collider other)
    {
        Warrior warrior = other.GetComponent<Warrior>();
        if (warrior != null && _healWarriors.Contains(warrior))
            _healWarriors.Remove(warrior);
    }
}
