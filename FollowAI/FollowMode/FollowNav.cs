using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowNav : IFollowStrategy {

    public BoyAI ai;

    public PlayerControl cat;

    public BoyController boy;

    public FollowNav(PlayerControl cat, BoyController boy)
    {
        ai = boy.ai;
        this.cat = cat;
        this.boy = boy;
    }

    public void Follow()
    {
    }

    public void LookatCat()
    {

    }
}
