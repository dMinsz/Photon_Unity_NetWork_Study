using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float moveSpeed;

    private Rigidbody rigid;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        rigid.velocity = transform.forward * moveSpeed;
        Destroy(gameObject, 3f);
    }

    public void ApplyLag(float lag) 
    {
        transform.position += rigid.velocity * lag;
    }

    public void Init(Vector3 position, Quaternion rotation, float lag)
    {
        transform.position = position;
        transform.rotation = rotation;

        rigid.velocity = transform.forward * moveSpeed;
        transform.position += rigid.velocity * lag;
    }
}
