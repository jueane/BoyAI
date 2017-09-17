using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCat : IFollowStrategy
{
    public BoyAI ai;

    public PlayerControl cat;

    public BoyController boy;

    public FollowCat(PlayerControl cat,BoyController boy)
    {
        ai = boy.ai;
        this.cat = cat;
        this.boy = boy;
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
            ai.posTarget = cat.transform.position;
            //移动

            float speedTemp = 1;

            if (ai.IsFollowingRight())
            {
                boy.moveProc.SetMoveByAI(speedTemp);
                ai.horSpeed = speedTemp;
            }
            else
            {
                boy.moveProc.SetMoveByAI(-speedTemp);
                ai.horSpeed = -speedTemp;
            }

        }
        
    }

    public void LookatCat()
    {
        
    }
}
