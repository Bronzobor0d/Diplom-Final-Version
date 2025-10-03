using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallTower : MonoBehaviour
{
    [SerializeField] private GameObject _ball;
    [SerializeField] private GameObject _fireParticle;
    [SerializeField] private GameObject _explosionParticle;
    [SerializeField] private float _speed;
    [SerializeField] public float _damage;

    public Vector3 Target;
    public Tower Tower;

    private List<Warrior> _hitWarriors = new List<Warrior>();
    private bool _isExplosion;

    #region BODY

    void Update()
    {
        if (Target != transform.position)
            transform.position = Vector3.MoveTowards(transform.position, Target, Time.deltaTime * _speed);
        else if (!_isExplosion)
        {
            _isExplosion = true;
            foreach (var warrior in _hitWarriors)
            {
                if (warrior.HP > 0 && warrior.Hp != null)
                {
                    if (Tower.Type == TowerType.CannonTower)
                        warrior.Hp.Damage(Tower.Damage);
                    else
                        warrior.Hp.Damage(_damage);
                }
            }
            _ball.SetActive(false);
            _fireParticle.SetActive(false);
            _explosionParticle.SetActive(true);
            if (Tower.Type == TowerType.FireTower)
                Tower.Target = null;
            Invoke("StartDestoy", 0.5f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Warrior warrior = other.GetComponent<Warrior>();
        if (warrior != null)
        {
            _hitWarriors.Add(other.GetComponent<Warrior>());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Warrior warrior = other.GetComponent<Warrior>();
        if (warrior != null && _hitWarriors.Contains(warrior))
            _hitWarriors.Remove(warrior);
    }

    #endregion

    #region CALLBACKS

    private void StartDestoy()
    {
        Destroy(gameObject);
    }

    #endregion
}
