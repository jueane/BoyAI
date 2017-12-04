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

    // Update is called once per frame
    void Update()
    {
        Collider[] cList = Physics.OverlapBox(boy.position, boy.GetComponent<BoxCollider>().bounds.extents, Quaternion.identity, LayerMask.GetMask(LayerName.AIPoint));
        //判断是否在FollowPoints中
        if (cList != null && cList.Length != 0)
        {
            inPointArea = true;
            //ai.followPoint.posTarget = targetPos.position;
        }
        else
        {
            inPointArea = false;
        }
    }
}



