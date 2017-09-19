using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCat : IFollowStrategy
{
    public BoyAI ai;

    public PlayerControl cat;

    public BoyController boy;

    public float speed;

    public FollowCat(PlayerControl cat, BoyController boy)
    {
        this.cat = cat;
        this.boy = boy;
        ai = boy.GetComponent<BoyAI>();
    }

    public void Follow()
    {
        float dis = Mathf.Abs(boy.transform.position.x - cat.transform.position.x);
        if (dis < ai.minDis)
        {
            ai.arrived = true;
        }
        else
        {
            //移动
            boy.moveProc.SetMoveByAI(ai.speed);
        }

    }

    public bool IsToRight()
    {
        if (boy.transform.position.x <= cat.transform.position.x)
        {
            return true;
        }
        return false;
    }

    public float RemainDistance()
    {
        float dis = Mathf.Abs(boy.transform.position.x - cat.transform.position.x);
        return dis;
    }
}
