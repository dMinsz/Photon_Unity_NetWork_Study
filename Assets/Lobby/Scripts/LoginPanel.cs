using Photon.Pun;
using TMPro;
using UnityEngine;

public class LoginPanel : MonoBehaviour
{
    [SerializeField] TMP_InputField idInputField;

    private void OnEnable()
    {
        idInputField.text = string.Format("Player {0}", Random.Range(1000, 10000));
    }

    public void Login()
    {
        if (idInputField.text == null || idInputField.text == "")
        {
            StatePanel.Instance.AddMessage("Player name is null");
            return;
        }

        PhotonNetwork.LocalPlayer.NickName = idInputField.text;
        PhotonNetwork.ConnectUsingSettings();

    }
}
