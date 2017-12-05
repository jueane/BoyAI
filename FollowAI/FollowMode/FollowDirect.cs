using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowDirect : IFollowStrategy
{
    public BoyAI ai;

    //public PlayerControl cat;

    public BoyController boy;

    //目标位置
    public Vector3 targetPos;

    public FollowDirect(BoyController boy)
    {
        //this.cat = cat;
        this.boy = boy;
        ai = boy.GetComponent<BoyAI>();
    }

    public void Follow()
    {
        //移动
        boy.moveProc.SetMoveByAI(ai.speed);
    }

    public bool IsToRight()
    {
        if (boy.transform.position.x <= targetPos.x)
        {
            return true;
        }
        return false;
    }

    public float RemainDistance()
    {
        return Mathf.Abs(boy.transform.position.x - targetPos.x);
    }


    public bool IsArrived()
    {
        return Mathf.Abs(boy.transform.position.x - targetPos.x) < ai.minDis;
    }

    public void InitTargetPostion(Vector3 position)
    {
        targetPos = position;
    }
}
