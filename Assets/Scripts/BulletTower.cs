using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletTower : MonoBehaviour
{
    [SerializeField] private float _speed;

    public Transform Target;
    public Tower Tower;

    private Vector3 _newTarget;

    #region BODY

    void Update()
    {
        if (Target)
        {
            transform.LookAt(Target);
            transform.position = Vector3.MoveTowards(transform.position, Target.position, Time.deltaTime * _speed);
            _newTarget = new Vector3(Target.position.x, 0, Target.position.z);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, _newTarget, Time.deltaTime * _speed);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.transform == Target)
        {
            Warrior warrior = Target.GetComponent<Warrior>();
            if (warrior.HP > 0 && warrior.Hp != null)
                warrior.Hp.Damage(Tower.Damage);
            Destroy(gameObject);
        }
        else if (other.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }

    #endregion
}
