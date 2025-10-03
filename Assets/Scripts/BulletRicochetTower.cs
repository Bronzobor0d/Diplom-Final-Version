using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletRicochetTower : MonoBehaviour
{
    [SerializeField] private float _speed;

    public Transform Target;
    public Transform SecondTarget;
    public List<Transform> AlreadyDamageWarrior = new List<Transform>();
    public Tower Tower;
    public int CountRicochet;

    #region MONO

    private void Start()
    {
        AlreadyDamageWarrior.Add(Target);
        CountRicochet++;
    }

    #endregion

    #region BODY

    void Update()
    {
        if (Target && Vector3.Distance(Target.position, transform.position) < 0.005f)
        {
            CountRicochet--;
            AlreadyDamageWarrior.Add(SecondTarget);
            Warrior warrior = Target.GetComponent<Warrior>();
            if (warrior.HP > 0 && warrior.Hp != null)
                warrior.Hp.Damage(Tower.Damage);
            Target = SecondTarget;
            SecondTarget = null;
            if (CountRicochet == 0 || Target == null)
                Destroy(gameObject);
        }
        if (Target)
        {
            transform.LookAt(Target);
            transform.position = Vector3.MoveTowards(transform.position, Target.position, Time.deltaTime * _speed);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        Transform warrior = other.transform;
        if (other.CompareTag("Warrior") && !AlreadyDamageWarrior.Contains(warrior) && SecondTarget == null && CountRicochet > 0)
            SecondTarget = warrior;
    }

    private void OnTriggerExit(Collider other)
    {
        Transform warrior = other.transform;
        if (SecondTarget == warrior)
            SecondTarget = null;
    }

    #endregion
}