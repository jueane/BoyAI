using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPoint : IFollowStrategy
{
    public BoyAI ai;

    public PlayerControl cat;

    public BoyController boy;

    public FollowPoint(PlayerControl cat, BoyController boy)
    {
        ai = boy.ai;
        this.cat = cat;
        this.boy = boy;
    }

    public void Follow()
    {
        ai.posTarget = GameManager.Instance._cameraController.transform.position;

        float disHor = Mathf.Abs(boy.transform.position.x - ai.posTarget.x);
        if (disHor < ai.minDis)
        {
            ai.arrived = true;
        }
        else
        {
        }
    }

    public void LookatCat()
    {

    }
}
