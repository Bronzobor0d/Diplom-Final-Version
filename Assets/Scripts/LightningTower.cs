using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningTower : MonoBehaviour
{
    [SerializeField] private LightningTower _sparkle;

    public Transform Target;
    public Tower Tower;
    public int CountRicochet;
    public LightningTower ParentSparkle;
    public List<Transform> AlreadyDamageWarrior;
    public bool IsMine;

    private bool _isRicochetTime;
    private bool _warriorLock;

    #region MONO

    private void Start()
    {
        StartCoroutine(RicochetTime());
        if (AlreadyDamageWarrior == null)
            AlreadyDamageWarrior = new List<Transform>();
        AlreadyDamageWarrior.Add(Target);
        transform.LookAt(Target);
        if (ParentSparkle == null)
            transform.localScale += new Vector3(0, 0, Vector3.Distance(transform.position, Target.position) - 1);
        else
            transform.localScale += new Vector3(0, 0, Vector3.Distance(transform.position, Target.position));
        StartCoroutine(Sparkle());
    }

    #endregion

    #region BODY

    private void OnTriggerEnter(Collider other)
    {
        if (IsMine)
        {
            if (other.CompareTag("Warrior") && !other.GetComponent<Warrior>().InvulnerabilityToTowers.Contains(Tower.Type) && !_warriorLock && CountRicochet > 0 && !_isRicochetTime && !AlreadyDamageWarrior.Contains(other.transform))
            {
                Tower.CreateRicoshetSparkl(other.GetComponent<Warrior>(), Target.GetComponent<Warrior>());
                LightningTower lightningTower = Instantiate(_sparkle, Target.position, Quaternion.identity);
                lightningTower.AlreadyDamageWarrior = AlreadyDamageWarrior;
                lightningTower.gameObject.transform.localScale = new Vector3(lightningTower.gameObject.transform.localScale.x, lightningTower.gameObject.transform.localScale.y, 0);
                CountRicochet--;
                lightningTower.Target = other.transform;
                lightningTower.CountRicochet = CountRicochet;
                lightningTower.IsMine = true;
                lightningTower.ParentSparkle = this;
                _warriorLock = true;
            }
        }
    }

    #endregion

    #region CALLBACKS

    private IEnumerator Sparkle()
    {
        if (Target.GetComponent<Warrior>().HP > 0 && Target.GetComponent<Warrior>().Hp != null)
            Target.GetComponent<Warrior>().Hp.Damage(Tower.Damage);
        yield return new WaitForSeconds(1);
        Destroy(gameObject);
    }

    private IEnumerator RicochetTime()
    {
        yield return new WaitForSeconds(0.15f);
        _isRicochetTime = true;
    }

    #endregion
}
