using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class RoomSettings : MonoBehaviourPunCallbacks
{
    [SerializeField] private string _map;
    private bool _isLeave;

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == _map)
        {
            if (PhotonNetwork.CurrentRoom != null)
            {
                if (PhotonNetwork.CurrentRoom.PlayerCount < 2 && !_isLeave)
                {
                    _isLeave = true;
                    PhotonNetwork.LeaveRoom();
                }
            }
        }
    }

    public void Leave()
    {
        PhotonNetwork.LocalPlayer.NickName = "";
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("MenuScene");
    }
}
