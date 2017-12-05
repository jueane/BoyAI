using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFollowStrategy
{
    //跟随
    void Follow();

    //跟随方向【当前应该面朝的方向】【考虑需要向上跳的情况，要面朝台子】
    bool IsToRight();

    //到终点的剩余距离
    float RemainDistance();

    //指定目标位置
    void InitTargetPostion(Vector3 position);

    //是否到达
    bool IsArrived();
}
