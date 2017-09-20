using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPointsManage : MonoBehaviour
{

    public Transform cat;
    public Transform boy;
    public BoyAI ai;

    public bool inPointArea;

    public Transform targetPos;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Collider[] cList = Physics.OverlapBox(boy.position, boy.GetComponent<BoxCollider>().bounds.extents, Quaternion.identity, LayerMask.GetMask("AIPoint"));
        //判断是否在FollowPoints中
        if (cList != null && cList.Length != 0)
        {
            inPointArea = true;
            ai.followPoint.posTarget = targetPos.position;
        }
        else
        {
            inPointArea = false;
        }


        //临时
        if (Input.GetKey(KeyCode.G))
        {
            Vector3 pos = GameManager.Instance.Player.transform.position = new Vector3(1059.28f, -52.05f, 5);
            GameManager.Instance.boy.transform.position = new Vector3(1073.65f, -52.31f, 5);
        }
    }
}



