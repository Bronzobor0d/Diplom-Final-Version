using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireTowerTrigger : MonoBehaviour
{
    [SerializeField] private GameObject _fireParticle;
    [SerializeField] private Vector3 _offsetFire;

    public List<Warrior> BurnedEnemies = new List<Warrior>();
    public Tower Tower;

    private bool _isShoot = false;
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
        Warrior otherWarrior;
        if (other.CompareTag("Warrior"))
            otherWarrior = other.GetComponent<Warrior>();
        else
            return;
        if (Tower.IsPlaced
            && otherWarrior.HP > 0
            && !BurnedEnemies.Contains(otherWarrior)
            && !otherWarrior.InvulnerabilityToTowers.Contains(Tower.Type))
        {
            if (otherWarrior.FireTower == null || Tower.Level > otherWarrior.FireTower.Level)
            {
                if (otherWarrior.FireTower != null)
                    otherWarrior.FireTower.ZoneTrigger.GetComponent<FireTowerTrigger>().BurnedEnemies.Remove(otherWarrior);
                _photonView.RPC("RPC_FindWarriorToStartBurn", RpcTarget.Others, otherWarrior.Index);
                StartBurnThisWarrior(otherWarrior);
                BurnedEnemies.Add(otherWarrior);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Warrior") && Tower.IsPlaced && BurnedEnemies.Contains(other.GetComponent<Warrior>()))
        {
            Warrior warrior = BurnedEnemies.Find(s => s.Index == other.GetComponent<Warrior>().Index);
            warrior.FireTower = null;
            BurnedEnemies.Remove(warrior);
        }
    }

    private void Update()
    {
        if (_sendWarriors == null)
            _sendWarriors = Tower.SendWarriors;
        if (BurnedEnemies.Count > 0 && Tower.Target == null)
        {
            bool check = false;
            foreach (var enemy in BurnedEnemies)
            {
                if (!enemy.InvulnerabilityToTowers.Contains(TowerType.Fireball))
                {
                    check = true;
                    break;
                }
            }
            if (check)
            {
                while (true)
                {
                    Warrior warrior = BurnedEnemies[Random.Range(0, BurnedEnemies.Count)];
                    if (warrior != null && !warrior.InvulnerabilityToTowers.Contains(TowerType.Fireball))
                    {
                        Tower.Target = warrior.transform;
                        break;
                    }
                    else if (warrior == null)
                        BurnedEnemies.Remove(warrior);
                    else if (BurnedEnemies.Count == 0)
                        break;
                }
            }
        }
        if (BurnedEnemies.Count > 0 && !_isShoot)
            StartCoroutine(Fire());
    }

    #endregion

    #region CALLBACKS

    /// <summary>
    ///  Накладывание эффекта поджога.
    /// </summary>

    IEnumerator Fire()
    {
        _isShoot = true;
        yield return new WaitForSeconds(Tower.ShootDelay);
        List<Warrior> nullEnemies = new List<Warrior>();
        foreach(var burnedWarrior in BurnedEnemies)
        {
            if (burnedWarrior != null)
            {
                _photonView.RPC("RPC_FindWarriorToBurn", RpcTarget.Others, burnedWarrior.Index);
                BurnThisWarrior(burnedWarrior);
            }
            else
            {
                nullEnemies.Add(burnedWarrior);
            }
        }
        foreach(var nullWarrior in nullEnemies)
        {
            BurnedEnemies.Remove(nullWarrior);
        }
        if (BurnedEnemies.Count > 0)
            _photonView.RPC("RPC_StartFireAnimation", RpcTarget.All);
        _isShoot = false;
    }

    private void StartBurnThisWarrior(Warrior warrior)
    {
        warrior.BurnDamage = Tower.Damage;
        warrior.FireTower = Tower;
    }

    private void BurnThisWarrior(Warrior warrior)
    {
        warrior.IsFire = true;
        warrior.TimeFire = 5;
    }

    IEnumerator FireAnimation()
    {
        GameObject fireParticle = Instantiate(_fireParticle, gameObject.transform.position + _offsetFire, Quaternion.identity, Tower.transform);
        yield return new WaitForSeconds(1f);
        Destroy(fireParticle);
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
                Debug.LogError("У огненной башни _sendWarrirors = null");
            else
                Debug.LogError("Огненная башня не смогла найти воина по другой причине");
            return null;
        }
    }

    #endregion

    #region PUNCALLBACKS

    [PunRPC]
    private void RPC_StartFireAnimation()
    {
        StartCoroutine(FireAnimation());
    }

    [PunRPC]
    private void RPC_FindWarriorToStartBurn(int index)
    {
        Warrior warrior = FindWarrior(index);
        if (warrior != null)
            StartBurnThisWarrior(warrior);
    }

    [PunRPC]
    private void RPC_FindWarriorToBurn(int index)
    {
        Warrior warrior = FindWarrior(index);
        if (warrior != null)
            BurnThisWarrior(warrior);
        else
            Debug.LogError("Найденный воин огненной башней оказался null");
    }

    #endregion
}
