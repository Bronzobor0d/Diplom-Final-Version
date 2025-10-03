using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceTowerTrigger : MonoBehaviour
{
    [SerializeField] private float _freezePower;
    [SerializeField] private GameObject _freezePatricle;
    [SerializeField] private Vector3 _offsetParticle;
    [SerializeField] private GameObject _crystal;

    public List<Warrior> FreezesWarrior = new List<Warrior>();
    public Tower Tower;

    private bool _isShoot;
    private SendWarriors _sendWarriors;
    private PhotonView _photonView;

    #region MONO

    private void Start()
    {
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

        if (!otherWarrior.IsFreeze
            && !FreezesWarrior.Contains(otherWarrior) 
            && Tower.IsPlaced 
            && otherWarrior.HP > 0
            && !otherWarrior.InvulnerabilityToTowers.Contains(Tower.Type))
        {
            _photonView.RPC("RPC_FindWarriorToStartFreeze", RpcTarget.Others, otherWarrior.Index);
            StartFreezeThisWarrior(otherWarrior);
            FreezesWarrior.Add(otherWarrior);
        }
        else if (!FreezesWarrior.Contains(otherWarrior)
            && Tower.IsPlaced
            && otherWarrior.HP > 0
            && !otherWarrior.InvulnerabilityToTowers.Contains(Tower.Type))
        {
            if (Tower.NextLevelTower == null && otherWarrior.IceTower.NextLevelTower != null)
            {
                if (otherWarrior.IceTower != null)
                    otherWarrior.IceTower.ZoneTrigger.GetComponent<IceTowerTrigger>().FreezesWarrior.Remove(otherWarrior);
                _photonView.RPC("RPC_FindWarriorToStartFreeze", RpcTarget.Others, otherWarrior.Index);
                StartFreezeThisWarrior(otherWarrior);
                FreezesWarrior.Add(otherWarrior);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Warrior") && Tower.IsPlaced && FreezesWarrior.Contains(other.GetComponent<Warrior>()))
        {
            FreezesWarrior.Remove(other.GetComponent<Warrior>());
        }
    }

    private void Update()
    {
        if (_sendWarriors == null)
        {
            _sendWarriors = Tower.SendWarriors;
        }
        if (FreezesWarrior.Count > 0 && !_isShoot)
        {
            StartCoroutine(Freeze());
        }
    }

    #endregion

    #region CALLBACKS

    /// <summary>
    ///  Накладывание эффекта заморозки.
    /// </summary>
    public IEnumerator Freeze()
    {
        _isShoot = true;
        yield return new WaitForSeconds(Tower.ShootDelay);
        List<Warrior> nullEnemies = new List<Warrior>();
        foreach (var freezeWarrior in FreezesWarrior)
        {
            if (freezeWarrior != null)
            {
                _photonView.RPC("RPC_FindWarriorToFreeze", RpcTarget.Others, freezeWarrior.Index);
                FreezeThisWarrior(freezeWarrior);
            }
            else
            {
                nullEnemies.Add(freezeWarrior);
            }
        }
        foreach (var nullWarrior in nullEnemies)
        {
            FreezesWarrior.Remove(nullWarrior);
        }
        if (FreezesWarrior.Count > 0)
        {
            _photonView.RPC("RPC_StartFreezeAnimation", RpcTarget.Others);
            StartCoroutine(FreezeAnimation());
        }
        _isShoot = false;
    }

    private void StartFreezeThisWarrior(Warrior warrior)
    {
        warrior.IsFreeze = true;
        warrior.TimeFreeze = 2;
        warrior.FreezePower = _freezePower;
        warrior.IceTower = Tower;
    }

    private void FreezeThisWarrior(Warrior warrior)
    {
        warrior.IsFreeze = true;
        warrior.TimeFreeze = 2;
        if (Tower.NextLevelTower == null && warrior.HP > 0 && warrior.Hp != null)
            warrior.Hp.Damage(Tower.Damage);
    }

    IEnumerator FreezeAnimation()
    {
        GameObject freezeParticle;
        if (Tower.Level == 4)
            freezeParticle = Instantiate(_freezePatricle, _crystal.transform.position + _offsetParticle, Quaternion.identity, _crystal.transform);
        else
            freezeParticle = Instantiate(_freezePatricle, gameObject.transform.position + _offsetParticle, Quaternion.identity, Tower.transform);
        yield return new WaitForSeconds(1f);
        Destroy(freezeParticle);
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
                Debug.LogError("У ледяной башни _sendWarriors = null");
            else
                Debug.LogError("Ледяная башня не смогла найти воина по другой причине");
            return null;
        }
    }

    #endregion

    #region PUNCALLBACKS

    [PunRPC]
    private void RPC_StartFreezeAnimation()
    {
        StartCoroutine(FreezeAnimation());
    }

    [PunRPC]
    private void RPC_FindWarriorToFreeze(int index)
    {
        Warrior warrior = FindWarrior(index);
        if (warrior != null)
            FreezeThisWarrior(warrior);
        else
            Debug.LogError("Найденный воин ледяной башней оказался null");
    }

    [PunRPC]
    private void RPC_FindWarriorToStartFreeze(int index)
    {
        Warrior warrior = FindWarrior(index);
        if (warrior != null)
            StartFreezeThisWarrior(warrior);
    }

    #endregion
}
