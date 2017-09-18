using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPoint : IFollowStrategy
{
    public BoyAI ai;

    public PlayerControl cat;

    public BoyController boy;

    public Vector3 posTarget;

    public FollowPoint(PlayerControl cat, BoyController boy)
    {
        this.cat = cat;
        this.boy = boy;
        ai = boy.GetComponent<BoyAI>();
    }

    public void Follow()
    {
        posTarget = GameManager.Instance._cameraController.transform.position;

        float disHor = Mathf.Abs(boy.transform.position.x - posTarget.x);
        if (disHor < ai.minDis)
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
        if (boy.transform.position.x <= posTarget.x)
        {
            return true;
        }
        return false;
    }

    public float RemainDistance()
    {
        float dis = Mathf.Abs(boy.transform.position.x - posTarget.x);
        return dis;
    }
}
