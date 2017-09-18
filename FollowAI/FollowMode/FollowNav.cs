using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowNav : IFollowStrategy
{
    public NavManage nav;

    public BoyAI ai;

    public PlayerControl cat;

    public BoyController boy;

    public FollowNav(PlayerControl cat, BoyController boy, NavManage nav)
    {
        this.cat = cat;
        this.boy = boy;
        ai = boy.GetComponent<BoyAI>();
        this.nav = nav;
    }

    public void Follow()
    {
        float dis = RemainTotalDistance();
        if (dis < ai.minDis)
        {
            ai.arrived = true;
        }
        else
        {
            //移动
            if (Mathf.Abs(nav.pathList[1].x - nav.pathList[0].x) < 0.1f)
            {
                boy.moveProc.SetMoveByAI(0);
            }
            else
            {
                boy.moveProc.SetMoveByAI(ai.speed);
            }
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
        float dis = 0;
        dis = Mathf.Abs(nav.pathList[1].x - nav.pathList[0].x);
        return dis;
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
}
