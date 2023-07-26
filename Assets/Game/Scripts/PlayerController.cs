using Photon.Pun.Demo.Asteroids;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviourPun, IPunObservable
{
    [SerializeField] List<Color> playerColor;
    [SerializeField] float movePower;
    [SerializeField] float rotateSpeed;
    [SerializeField] float maxSpeed;
    [SerializeField] Bullet bulletPrefab;
    [SerializeField] float fireCoolTime;

    private PlayerInput input;
    private Rigidbody rigid;
    private Vector2 inputDir;

    private int bulletCount;
    private float lastFireTime = float.MinValue;


    private void Awake()
    {
        input = GetComponent<PlayerInput>();
        rigid = GetComponent<Rigidbody>();

        SetPlayerColor();

        if (!photonView.IsMine)
            Destroy(input);
    }

    private void Update()
    {
        Accelate(inputDir.y);
        Rotate(inputDir.x);
        CheckExitScreen();
    }

    private void OnMove(InputValue value)
    {
        inputDir = value.Get<Vector2>();
    }

    private void OnFire(InputValue value)
    {
        if (value.isPressed)
            Fire();
    }

    private void Accelate(float input)
    {
        rigid.AddForce(input * movePower * transform.forward, ForceMode.Force);
        if (rigid.velocity.magnitude > maxSpeed)
        {
            rigid.velocity = rigid.velocity.normalized * maxSpeed;
        }
    }

    private void Rotate(float input)
    {
        transform.Rotate(Vector3.up, input * rotateSpeed * Time.deltaTime);
    }

    //private void Fire()
    //{
    //    photonView.RPC("CreateBullet", RpcTarget.All,transform.position,transform.rotation);
    //    bulletCount++;
    //}

    private void Fire()
    {
        photonView.RPC("RequestCreateBullet", RpcTarget.MasterClient, transform.position, transform.rotation);
    }
    [PunRPC]
    public void CreateBullet(Vector3 pos, Quaternion rotation , PhotonMessageInfo info)
    {        
        float lag = (float)(PhotonNetwork.Time - info.SentServerTime);// data delay compensation

        Bullet bullet = Instantiate(bulletPrefab, pos, rotation);
        bullet.ApplyLag(lag);

    }


    [PunRPC]
    public void RequestCreateBullet(Vector3 position, Quaternion rotation, PhotonMessageInfo info)
    {
        if (Time.time < lastFireTime + fireCoolTime)
            return;

        lastFireTime = Time.time;
        photonView.RPC("ResultCreateBullet", RpcTarget.All, transform.position, transform.rotation, info.SentServerTime);
    }

    [PunRPC]
    public void ResultCreateBullet(Vector3 position, Quaternion rotation, double sentServerTime)
    {
        float lag = (float)(PhotonNetwork.Time - sentServerTime);

        Bullet bullet = Instantiate(bulletPrefab);
        bullet.Init(position, rotation, lag);
        bulletCount++;
    }


    private void CheckExitScreen()
    {
        if (Camera.main == null)
            return;

        if (Mathf.Abs(rigid.position.x) > (Camera.main.orthographicSize * Camera.main.aspect))
        {
            rigid.position = new Vector3(-Mathf.Sign(rigid.position.x) * Camera.main.orthographicSize * Camera.main.aspect, 0, rigid.position.z);
            rigid.position -= rigid.position.normalized * 0.1f; // offset a little bit to avoid looping back & forth between the 2 edges 
        }

        if (Mathf.Abs(rigid.position.z) > Camera.main.orthographicSize)
        {
            rigid.position = new Vector3(rigid.position.x, rigid.position.y, -Mathf.Sign(rigid.position.z) * Camera.main.orthographicSize);
            rigid.position -= rigid.position.normalized * 0.1f; // offset a little bit to avoid looping back & forth between the 2 edges 
        }
    }

    private void SetPlayerColor()
    {
        int playerNumber = photonView.Owner.GetPlayerNumber();

        if (playerColor == null || playerColor.Count <= playerNumber)
            return;

        Renderer render = GetComponent<Renderer>();
        render.material.color = playerColor[playerNumber];
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    { // 변수동기화
        if (stream.IsWriting)
            stream.SendNext(bulletCount);
        else // stream.IsReading
            bulletCount = (int)stream.ReceiveNext();
    }
}
