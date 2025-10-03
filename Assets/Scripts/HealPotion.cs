using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealPotion : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private GameObject _potion;
    [SerializeField] private GameObject _explosionParticle;
    [SerializeField] private GameObject _bubblesParticle;
    [SerializeField] private float _speedUp;
    [SerializeField] private float _speedForward;

    public float HealCount;

    private List<Warrior> _hitWarriors = new List<Warrior>();
    private bool _isExplosion;

    #region MONO

    private void Start()
    {
        _rigidbody.AddForce(Vector3.up * _speedUp, ForceMode.Impulse);
        if (transform.rotation.y >= 0.9 && transform.rotation.y <= 1.1 || transform.rotation.y >= -1.1 && transform.rotation.y <= -0.9)
            _rigidbody.AddForce(Vector3.back * _speedForward, ForceMode.Impulse);
        else if (transform.rotation.y >= -0.1 && transform.rotation.y <= 0.1)
            _rigidbody.AddForce(Vector3.forward * _speedForward, ForceMode.Impulse);
        else if (transform.rotation.y >= -0.8 && transform.rotation.y <= -0.6)
            _rigidbody.AddForce(Vector3.left * _speedForward, ForceMode.Impulse);
        else if (transform.rotation.y >= 0.6 && transform.rotation.y <= 0.8)
            _rigidbody.AddForce(Vector3.right * _speedForward, ForceMode.Impulse);
    }

    #endregion

    #region BODY

    private void Update()
    {
        if (transform.position.y <= 0 && !_isExplosion)
        {
            _isExplosion = true;
            foreach (var warrior in _hitWarriors)
            {
                if (warrior.HP > 0 && warrior.HP < warrior.MaxHP && warrior.Hp != null)
                {
                    warrior.Hp.Heal(HealCount);
                }
            }
            _potion.SetActive(false);
            _bubblesParticle.SetActive(false);
            _explosionParticle.SetActive(true);
            Invoke("StartDestoy", 1f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Warrior warrior = other.GetComponent<Warrior>();
        if (warrior != null && warrior.HealCount <= 0)
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
