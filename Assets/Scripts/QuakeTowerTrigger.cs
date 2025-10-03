using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuakeTowerTrigger : MonoBehaviour
{
    [SerializeField] private Animator _thisAnimator;
    [SerializeField] private float _firstLevelShootDelay;
    [SerializeField] private GameObject _stunParticles;
    [SerializeField] private Vector3 _offsetParticle;

    public List<Warrior> StunEnemies = new List<Warrior>();

    public Tower Tower;
    public int TimeStun;

    private bool _isShoot;
    private SendWarriors _sendWarriors;
    private PhotonView _photonView;

    #region MONO

    private void Start()
    {
        _thisAnimator.speed = _firstLevelShootDelay / Tower.ShootDelay;
        _photonView = GetComponent<PhotonView>();
        _sendWarriors = Tower.SendWarriors;
    }

    #endregion

    #region BODY

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Warrior"))
            return;
        Warrior otherWarrior = other.GetComponent<Warrior>();

        if (!otherWarrior.IsStun
            && !StunEnemies.Contains(otherWarrior)
            && Tower.IsPlaced
            && otherWarrior.HP > 0
            && !otherWarrior.InvulnerabilityToTowers.Contains(Tower.Type)
            && otherWarrior.QuakeTower == null)
        {
            _photonView.RPC("RPC_FindWarriorToStartStun", RpcTarget.Others, otherWarrior.Index);
            StartStunThisWarrior(otherWarrior);
            StunEnemies.Add(otherWarrior);
        }
        else if (!StunEnemies.Contains(otherWarrior)
            && Tower.IsPlaced
            && otherWarrior.HP > 0
            && !otherWarrior.InvulnerabilityToTowers.Contains(Tower.Type)
            && otherWarrior.QuakeTower != null)
        {
            if (Tower.Level > otherWarrior.QuakeTower.Level)
            {
                otherWarrior.QuakeTower.ZoneTrigger.GetComponent<QuakeTowerTrigger>().StunEnemies.Remove(otherWarrior);
                _photonView.RPC("RPC_FindWarriorToStartStun", RpcTarget.Others, otherWarrior.Index);
                StartStunThisWarrior(otherWarrior);
                StunEnemies.Add(otherWarrior);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Warrior") && Tower.IsPlaced && StunEnemies.Contains(other.GetComponent<Warrior>()))
        {
            Warrior warrior = other.GetComponent<Warrior>();
            _photonView.RPC("RPC_FindWarriorToStopStun", RpcTarget.Others, warrior.Index);
            StopStunThisWarrior(warrior);
            StunEnemies.Remove(other.GetComponent<Warrior>());
        }
    }

    private void Update()
    {
        if (_sendWarriors == null)
        {
            _sendWarriors = Tower.SendWarriors;
        }
        if (StunEnemies.Count > 0 && !_isShoot)
        {
            StartCoroutine(Stuning());
        }
    }

    #endregion

    #region CALLBACKS

    /// <summary>
    ///  Накладывание эффекта заморозки.
    /// </summary>
    public IEnumerator Stuning()
    {
        _isShoot = true;
        _thisAnimator.SetBool("isAttack", true);
        yield return new WaitForSeconds(Tower.ShootDelay);
        List<Warrior> nullEnemies = new List<Warrior>();
        foreach (var stunWarrior in StunEnemies)
        {
            if (stunWarrior != null)
            {
                _photonView.RPC("RPC_FindWarriorToStun", RpcTarget.Others, stunWarrior.Index);
                StunThisWarrior(stunWarrior);
            }
            else
            {
                nullEnemies.Add(stunWarrior);
            }
        }
        foreach (var nullWarrior in nullEnemies)
        {
            StunEnemies.Remove(nullWarrior);
        }
        StartCoroutine(WaitStun());
        if (StunEnemies.Count > 0)
        {
            _photonView.RPC("RPC_StartStunAnimation", RpcTarget.Others);
            StartCoroutine(StunAnimation());
        }
    }

    IEnumerator WaitStun()
    {
        _thisAnimator.SetBool("isAttack", false);
        yield return new WaitForSeconds(TimeStun);
        _isShoot = false;
    }

    private void StartStunThisWarrior(Warrior warrior)
    {
        warrior.QuakeTower = Tower;
    }

    private void StopStunThisWarrior(Warrior warrior)
    {
        warrior.QuakeTower = null;
    }

    private void StunThisWarrior(Warrior warrior)
    {
        warrior.IsStun = true;
        warrior.TimeStun = TimeStun;
        if (warrior.HP > 0)
            warrior.Hp.Damage(Tower.Damage);
    }

    IEnumerator StunAnimation()
    {
        GameObject stunParticle = Instantiate(_stunParticles, gameObject.transform.position + _offsetParticle, Quaternion.identity, Tower.transform);
        yield return new WaitForSeconds(1f);
        Destroy(stunParticle);
    }

    private Warrior FindWarrior(int index)
    {
        try
        {
            if (_sendWarriors == null)
                _sendWarriors = Tower.SendWarriors;
            return _sendWarriors.AlreadySendWarrior.Find(s => s.Index == index);
        }
        catch
        {
            if (_sendWarriors == null)
                Debug.LogError("У оглушалки _sendWarriors = null");
            else
                Debug.LogError("Оглушалка не смогла найти воина по другой причине");
            return null;
        }
    }

    #endregion

    #region PUNCALLBACKS

    [PunRPC]
    private void RPC_StartStunAnimation()
    {
        StartCoroutine(StunAnimation());
    }

    [PunRPC]
    private void RPC_FindWarriorToStun(int index)
    {
        Warrior warrior = FindWarrior(index);
        if (warrior != null)
            StunThisWarrior(warrior);
        else
            Debug.LogError("Найденный воин оглушалкой оказался null");
    }

    [PunRPC]
    private void RPC_FindWarriorToStartStun(int index)
    {
        Warrior warrior = FindWarrior(index);
        if (warrior != null)
            StartStunThisWarrior(warrior);
        else
            Debug.LogError("Найденный воин оглушалкой оказался null");
    }

    [PunRPC]
    private void RPC_FindWarriorToStopStun(int index)
    {
        Warrior warrior = FindWarrior(index);
        if (warrior != null)
            StopStunThisWarrior(warrior);
        else
            Debug.LogError("Найденный воин оглушалкой оказался null");
    }

    #endregion
}
