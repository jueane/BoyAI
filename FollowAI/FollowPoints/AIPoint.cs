using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPoint : MonoBehaviour
{
    public Transform cat;
    public BoyController boy;
    public BoyAI ai;

    public TriggerMode trigger;
    public ActionMode action;

    public float runSpeed = 1;

    public float delay = 0;

    public Transform targetPos;

    void OnTriggerEnter(Collider c)
    {
        bool condition1 = trigger == TriggerMode.cat && TagName.Player.Equals(c.tag);
        bool condition2 = trigger == TriggerMode.boy && TagName.Player2.Equals(c.tag);
        if (condition1 || condition2)
        {
            StartCoroutine(DoAction());
        }

    }

    IEnumerator DoAction()
    {
        if (delay > 0)
        {
            Stop();
        }
        print(Time.frameCount);
        yield return new WaitForSeconds(delay);
        print(Time.frameCount);
        switch (action)
        {
            case ActionMode.run:
                Run();
                break;
            case ActionMode.stop:
                Stop();
                break;
            case ActionMode.end:
                End();
                break;
        }
    }

    void Run()
    {
        //print("速度为：" + runSpeed);
        //boy.moveProc.SetMoveByAI(runSpeed);
        ai.GotoPoint(targetPos.position);
    }

    void Stop()
    {
        ai.StayThere();
        //boy.moveProc.SetMoveByAI(0);
    }

    void End()
    {
        ai.mode = FollowMode.FollowTarget;
    }

}


public enum TriggerMode
{
    cat,
    boy
}

public enum ActionMode
{
    run,
    stop,
    //将来需要的话，可以把结束模式改为：结束为followCat、结束改followNav两种方式
    end
}