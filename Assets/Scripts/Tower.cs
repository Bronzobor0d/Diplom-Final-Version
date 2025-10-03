using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private List<Renderer> _mainRenderers;
    [SerializeField] private Transform _shootElement;
    [SerializeField] private GameObject _bullet;
    [SerializeField] private GameObject _lookAtElement;
    [SerializeField] private GameObject _stand;
    [SerializeField] private Animator _lookAtElementAnimator;

    public Transform Target;
    public Tower NextLevelTower;
    public Tower NextLevelTowerPath1;
    public Tower NextLevelTowerPath2;
    public TowerPlaces TowerPlace;
    public GameObject ZoneTrigger;
    public GameObject ZoneVisibility;
    public SendWarriors SendWarriors;

    private PhotonView _photonView;

    [Header("Parametrs")]
    public float Damage;
    public int Cost;
    public float ShootDelay;
    public float ShootDelayAdditional;
    public bool IsPlaced = false;
    public Vector3 Offset;
    public TowerType Type;
    public string Name;
    public int Level;
    public int RemoveCost;
    public bool IsBig;
    public string Team;
    public bool TwoPath;
    public string NamePath1;
    public string NamePath2;
    public int SelectedPath;
    public int CountRicochet;

    private bool _isShoot;
    private float _delayShootAnimation;

    #region MONO

    private void Awake()
    {
        _photonView = GetComponent<PhotonView>();
        if (_lookAtElement != null)
        {
            if (Type == TowerType.TurretTower)
            {
                _delayShootAnimation = 0.0625f * ShootDelay;
                _lookAtElementAnimator.speed = 0.5f / ShootDelay;
            }
            else
                _delayShootAnimation = 6.88f;
        }
    }

    #endregion

    #region BODY

    void Update()
    {
        if (Target && _lookAtElement && _stand)
        {
            if (Type == TowerType.CannonTower)
                _lookAtElement.transform.LookAt(new Vector3(Target.position.x, 0, Target.position.z));
            else
                _lookAtElement.transform.LookAt(Target);
            _lookAtElement.transform.Rotate(0, 90, 0);
            float Angle = -Mathf.Atan2(Target.transform.position.z - _stand.transform.position.z, Target.transform.position.x - _stand.transform.position.x) / Mathf.PI * 180f;
            _stand.transform.eulerAngles = new Vector3(_stand.transform.eulerAngles.x, Angle, _stand.transform.eulerAngles.z);
        }
        if (Target && IsPlaced && !_isShoot)
        {
            StartCoroutine(Shoot());
        }
        if (!Target && IsPlaced && _lookAtElement != null)
        {
            _lookAtElementAnimator.SetBool("isShoot", false);
        }
    }

    #endregion

    #region CALLBACKS

    /// <summary>
    ///  Установка цвета башни, в зависимости от возможности установить её на выбранное место.
    /// </summary>
    public void SetTransparent(bool available)
    {
        if (available)
        {
            foreach(var renderer in _mainRenderers)
                renderer.material.color = Color.green;
        }
        else
        {
            foreach (var renderer in _mainRenderers)
                renderer.material.color = Color.red;
        }
    }

    /// <summary>
    ///  Установка стандартного цвета башни.
    /// </summary>
    public void SetNormal()
    {
        foreach (var renderer in _mainRenderers)
            renderer.material.color = Color.white;
    }

    /// <summary>
    ///  Выстрел башни с задержкой.
    /// </summary>
    IEnumerator Shoot()
    {
        _isShoot = true;
        if (_lookAtElement != null)
            StartCoroutine(ShootAnimation());
        if (Type == TowerType.FireTower)
            yield return new WaitForSeconds(ShootDelayAdditional);
        else
            yield return new WaitForSeconds(ShootDelay);
        if (Target && _bullet != null)
        {
            _photonView.RPC("RPC_FindTargetServer", RpcTarget.Others, Target.GetComponent<Warrior>().Index);
            CreateBullet(Target, true);
        }
        _isShoot = false;
    }

    private void CreateBullet(Transform target, bool isMine)
    {
        GameObject b;
        if (Type == TowerType.FireTower)
            b = Instantiate(_bullet, new Vector3(target.position.x, 2.5f, target.position.z), Quaternion.identity);
        else
            b = Instantiate(_bullet, _shootElement.position, Quaternion.identity);
        if (b.GetComponent<LightningTower>() != null)
        {
            LightningTower lightningTower = b.GetComponent<LightningTower>();
            lightningTower.Target = target;
            lightningTower.Tower = this;
            lightningTower.CountRicochet = CountRicochet;
            if (isMine)
                lightningTower.IsMine = true;
        }
        else if (b.GetComponent<BallTower>() != null)
        {
            BallTower ballTower = b.GetComponent<BallTower>();
            ballTower.Target = new Vector3(target.position.x, 0, target.position.z);
            ballTower.Tower = this;
        }
        else
        {
            BulletTower bulletTower = b.GetComponent<BulletTower>();
            if (bulletTower != null)
            {
                bulletTower.Target = target;
                bulletTower.Tower = this;
            }
            else
            {
                BulletRicochetTower bulletRicochetTower = b.GetComponent<BulletRicochetTower>();
                bulletRicochetTower.Target = target;
                bulletRicochetTower.Tower = this;
                bulletRicochetTower.CountRicochet = CountRicochet;
            }
        }
    }

    IEnumerator ShootAnimation()
    {
        yield return new WaitForSeconds(_delayShootAnimation);
        if (Target != null)
            _lookAtElementAnimator.SetBool("isShoot", true);
        if (Type == TowerType.CannonTower)
            StartCoroutine(EndShootAnimation());
    }

    IEnumerator EndShootAnimation()
    {
        yield return new WaitForSeconds(1f);
        _lookAtElementAnimator.SetBool("isShoot", false);
    }

    public void CreateRicoshetSparkl(Warrior warrior, Warrior warriorOld)
    {
        _photonView.RPC("RPC_SpawnRicoshetSparkl", RpcTarget.Others, warrior.Index, warriorOld.Index);
    }

    #endregion

    #region PUNCALLBACKS

    [PunRPC]
    private void RPC_FindTargetServer(int indexTarget)
    {
        Warrior warrior = null;
        try
        {
            warrior = SendWarriors.AlreadySendWarrior.Find(s => s.Index == indexTarget);
        }
        catch
        {
            if (SendWarriors == null)
                Debug.LogError("У какой-то башни SendWarriors = null");
            else
                Debug.LogError("У башни другие проблемы");
        }
        if (warrior != null)
            CreateBullet(warrior.transform, false);
        else
            Debug.LogError("Найденный башней воин оказался null");
    }

    [PunRPC]
    private void RPC_SpawnRicoshetSparkl(int indexTarget, int indexTargetOld)
    {
        Warrior warriorNew = null;
        Warrior warriorOld = null;
        try
        {
            warriorNew = SendWarriors.AlreadySendWarrior.Find(s => s.Index == indexTarget);
            warriorOld = SendWarriors.AlreadySendWarrior.Find(s => s.Index == indexTargetOld);
        }
        catch
        {
            if (SendWarriors == null)
                Debug.LogError("У какой-то башни SendWarriors = null");
            else
                Debug.LogError("У башни другие проблемы");
        }
        LightningTower lightningTower = Instantiate(_bullet, warriorOld.transform.position, Quaternion.identity).GetComponent<LightningTower>();
        lightningTower.gameObject.transform.localScale = new Vector3(lightningTower.gameObject.transform.localScale.x, lightningTower.gameObject.transform.localScale.y, 0);
        try
        {
            lightningTower.Target = warriorNew.transform;
        }
        catch
        {
            Debug.LogError("Воин уже умер, но молния только в него целится");
        }
        lightningTower.Tower = this;
        lightningTower.ParentSparkle = lightningTower;
        lightningTower.CountRicochet = CountRicochet;
    }

    #endregion
}
