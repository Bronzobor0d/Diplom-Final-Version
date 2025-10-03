using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CastleHP : MonoBehaviour
{
    [SerializeField] private BuildingTowers _buildingTowers;
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private Text _countHPText;
    [SerializeField] private CastleHP _enemyCastleHP;

    public float HP;
    public float MaxHP;

    private PhotonView _photonView;

    #region MONO

    private void Start()
    {
        _photonView = GetComponent<PhotonView>();
    }

    #endregion

    #region BODY

    void Update()
    {
        _countHPText.text = HP.ToString();

        if (HP <= 0)
        {
            _buildingTowers.GameOver();
            _gameOverPanel.SetActive(true);
        }
    }

    #endregion

    #region CALLBACKS

    /// <summary>
    ///  Вычетание из здоровья замка количества переданного урона.
    /// </summary>
    public void Damage(float DMGcount)
    {
        HP -= DMGcount;
        if (_photonView != null)
            _photonView.RPC("DamageEnemyCastle", RpcTarget.Others, DMGcount);
    }

    #endregion

    #region PUNCALLBACKS

    [PunRPC]
    private void DamageEnemyCastle(float DMGcount)
    {
        _enemyCastleHP.Damage(DMGcount);
    }

    #endregion
}
