using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooFarFollow : MonoBehaviour
{
    //检测间隔
    public float interval = 1f;
    private float timeElapse = 0;

    //触发跟随的最小距离
    public float minDis = 20;

    public Transform cat;

    public Transform boy;

    public BoyAI ai;

    // Use this for initialization
    void Start()
    {
        cat = GameManager.Instance.Player.transform;
        boy = transform;
        ai = GetComponent<BoyAI>();
    }

    // Update is called once per frame
    void Update()
    {
        timeElapse += Time.deltaTime;
        if (timeElapse < interval)
        {
            return;
        }
        timeElapse = 0;

        Detect();
    }

    //检测一次是否需要[过远跟随]
    void Detect()
    {
        if (Vector2.Distance(boy.position, cat.position) > minDis)
        {
            ai.GoToCat();
        }                
    }

}
