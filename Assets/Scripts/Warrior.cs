using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warrior : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Transform _pointSpawnPotion;
    [SerializeField] private HealPotion _healPotion;

    public WarriorHPScript Hp;
    public CastleHP EnemyCastleHP;
    public Money Money;
    public Experience EXP;
    public GameObject FireEffect;
    public GameObject FreezeEffect;
    public GameObject StunEffect;
    public Tower IceTower;
    public Tower QuakeTower;
    public Tower FireTower;

    private Animator _thisAnimator;
    private MoveToWayPoints _thisMoveToWayPoints;
    private PhotonView _photonView;

    [Header("Parametrs")]
    [SerializeField] private float _damage;
    [SerializeField] private float _attackDelay;
    [SerializeField] private float _healSelfCount;

    public float HP;
    public float MaxHP;
    public int Cost;
    public int EXPCost;
    public int EXPReward;
    public bool IsFreeze;
    public bool IsFire;
    public bool IsStun;
    public float TimeFire;
    public float TimeFreeze;
    public float TimeStun;
    public List<TowerType> InvulnerabilityToTowers;
    public float BurnDamage;
    public float FreezePower;
    public Vector3 HpOffset;
    public bool IsEnemy;
    public string Name;
    public int Index;
    public float HealCount;
    public bool isReadyHeal;

    private bool _isDeath;
    private bool _isAttack;
    private bool _isBurn;
    private bool _isFrozen;
    private bool _isStuning;
    private float _differenceSpeed;
    private float _speed;
    private bool _isHealSelf;
    private bool _isHeal;
    private bool _isMoveFinish;

    #region MONO

    private void Awake()
    {
        HP = MaxHP;
        _thisAnimator = gameObject.transform.GetChild(0).gameObject.GetComponent<Animator>();
        _thisMoveToWayPoints = gameObject.GetComponent<MoveToWayPoints>();
        _differenceSpeed = _thisAnimator.speed / _thisMoveToWayPoints.FirstLevelSpeed;
        _thisAnimator.SetBool("isMove", true);
        _photonView = GetComponent<PhotonView>();
    }

    #endregion

    #region BODY

    void Update()
    {
        if (HP <= 0 && !_isDeath && !_photonView.IsMine)
        {
            _isDeath = true;
            Money.CoinPlus(Cost / 2);
            _thisMoveToWayPoints.Waypoints = new List<Transform>();
            _thisAnimator.SetBool("isDead", true);
            StartCoroutine(Death());
        }
        else if (_isDeath)
        {
            _thisAnimator.speed = _thisMoveToWayPoints.FirstLevelSpeed * _differenceSpeed;
        }
        else
        {
            if (!_isBurn && IsFire && TimeFire > 0)
            {
                StartCoroutine(Burn());
            }
            if (!_isFrozen && IsFreeze && TimeFreeze > 0)
            {
                StartCoroutine(Freeze());
            }
            if (!_isStuning && IsStun && TimeStun > 0)
            {
                StartCoroutine(Stun());
            }
            if (!_isAttack && !_isStuning)
                _thisAnimator.speed = _thisMoveToWayPoints.Speed * _differenceSpeed;
            else if (!_isStuning)
                _thisAnimator.speed = _thisMoveToWayPoints.MaxSpeed * _differenceSpeed;
            else
                _thisAnimator.speed = _thisMoveToWayPoints.FirstLevelSpeed * _differenceSpeed;
            if (_healSelfCount > 0 && HP < MaxHP && !_isHealSelf)
            {
                StartCoroutine(HealSelf());
            }
            if (HealCount > 0 && !_isHeal && isReadyHeal && !_isStuning)
            {
                StartCoroutine(Heal());
            }
            if (HealCount > 0 && (!isReadyHeal || _isStuning))
            {
                _thisAnimator.SetBool("isHeal", false);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Castle"))
        {
            if (!_isAttack && !_isStuning && _isMoveFinish)
            {
                StartCoroutine(Attack());
            }
        }
    }

    #endregion

    #region CALLBACKS

    public void SetNotMove()
    {
        _isMoveFinish = true;
        _thisAnimator.SetBool("isMove", false);
    }

    IEnumerator Death()
    {
        _photonView.RPC("RPC_DestroyHPWarrior", RpcTarget.Others);
        yield return new WaitForSeconds(0.5f);
        _photonView.RPC("RPC_DestroyWarrior", RpcTarget.Others);
    }

    IEnumerator HealSelf()
    {
        _isHealSelf = true;
        yield return new WaitForSeconds(1f);
        Hp.Heal(_healSelfCount);
        _isHealSelf = false;
    }

    IEnumerator Heal()
    {
        _isHeal = true;
        yield return new WaitForSeconds(1f);
        HealPotion healPotion = Instantiate(_healPotion, _pointSpawnPotion.position, gameObject.transform.rotation);
        healPotion.HealCount = HealCount;
        _thisAnimator.SetBool("isHeal", true);
        _isHeal = false;
    }

    IEnumerator Attack()
    {
        _isAttack = true;
        StartCoroutine(AttackAnimation());
        yield return new WaitForSeconds(_attackDelay);
        if (HealCount > 0)
        {
            HealPotion healPotion = Instantiate(_healPotion, _pointSpawnPotion.position, gameObject.transform.rotation);
            healPotion.HealCount = HealCount;
        }
        _thisAnimator.SetBool("isAttack", false);
        if (!_isDeath && !_photonView.IsMine)
        {
            EnemyCastleHP.Damage(_damage);
        }
        _isAttack = false;
    }

    IEnumerator AttackAnimation()
    {
        yield return new WaitForSeconds(0.75f);
        _thisAnimator.SetBool("isAttack", true);
    }

    IEnumerator Burn()
    {
        _isBurn = true;
        if (!FireEffect.activeSelf)
            FireEffect.SetActive(true);
        yield return new WaitForSeconds(1f);
        TimeFire--;
        if (TimeFire == 0)
        {
            IsFire = false;
            FireEffect.SetActive(false);
        }
        if (HP > 0 && Hp != null)
            Hp.Damage(BurnDamage);
        _isBurn = false;
    }

    IEnumerator Freeze()
    {
        _isFrozen = true;
        if (!FreezeEffect.activeSelf)
            FreezeEffect.SetActive(true);
        _thisMoveToWayPoints.Speed *= 1 - FreezePower;
        yield return new WaitForSeconds(1f);
        _thisMoveToWayPoints.Speed = _thisMoveToWayPoints.MaxSpeed;
        TimeFreeze--;
        if (TimeFreeze == 0)
        {
            IsFreeze = false;
            FreezeEffect.SetActive(false);
        }
        _isFrozen = false;
    }

    IEnumerator Stun()
    {
        _isStuning = true;
        _thisAnimator.SetBool("isMove", false);
        if (!StunEffect.activeSelf)
            StunEffect.SetActive(true);
        if (_speed == 0)
            _speed = _thisMoveToWayPoints.MaxSpeed;
        _thisMoveToWayPoints.Speed = 0;
        _thisMoveToWayPoints.MaxSpeed = 0;
        yield return new WaitForSeconds(1f);
        TimeStun--;
        if (TimeStun <= 0)
        {
            IsStun = false;
            _thisMoveToWayPoints.Speed = _speed;
            _thisMoveToWayPoints.MaxSpeed = _speed;
            if (!_isMoveFinish)
                _thisAnimator.SetBool("isMove", true);
            StunEffect.SetActive(false);
        }
        _isStuning = false;
    }

    #endregion

    #region PUNCALLBACKS

    [PunRPC]
    private void RPC_DestroyWarrior()
    {
        if (gameObject != null)
            PhotonNetwork.Destroy(gameObject);
    }

    [PunRPC]
    private void RPC_DestroyHPWarrior()
    {
        _thisMoveToWayPoints.Waypoints = new List<Transform>();
        _thisAnimator.SetBool("isDead", true);
        EXP.ExpPlus(EXPReward);
        if (Hp != null)
            Destroy(Hp.gameObject);
    }

    #endregion
}
