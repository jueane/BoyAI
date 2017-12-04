using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowNav : IFollowStrategy
{
    public NavManage nav;

    public BoyAI ai;

    //public PlayerControl cat;

    public BoyController boy;

    //目标位置
    public Vector3 targetPos;

    public FollowNav(BoyController boy, NavManage nav)
    {
        //this.cat = cat;
        this.boy = boy;
        ai = boy.GetComponent<BoyAI>();
        this.nav = nav;
    }

    public void Follow()
    {
        //移动
        if (Mathf.Abs(nav.pathList[1].x - nav.pathList[0].x) < 0.3f)
        {
            boy.moveProc.SetMoveByAI(0);
        }
        else
        {
            boy.moveProc.SetMoveByAI(ai.speed);
        }
    }

    public bool IsToRight()
    {
        if (boy.transform.position.x <= nav.pathList[1].x)
        {
            return true;
        }
        return false;
    }

    public float RemainDistance()
    {
        return Mathf.Abs(nav.pathList[1].x - nav.pathList[0].x);
    }

    //所有节点加起来的剩余距离
    float RemainTotalDistance()
    {
        float dis = 0;
        for (int i = 0; i < nav.pathList.Count - 1; i++)
        {
            dis += Mathf.Abs(nav.pathList[i + 1].x - nav.pathList[i].x);
        }
        return dis;
    }

    public bool IsArrived()
    {
        return RemainTotalDistance() < ai.minDis;
    }

    public void AdjustFacing()
    {
        //throw new System.NotImplementedException();
    }

    public void InitTargetPostion(Vector3 position)
    {
        targetPos = position;
    }
}
