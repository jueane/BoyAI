using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavManage : MonoBehaviour
{
    public Transform cat;
    public Transform boy;

    public bool inNavmesh;

    public MeshBoard mb;

    public Vector3 a;
    public Vector3 b;

    public List<Vector3> pathList;

    public Vector2 direction;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Collider[] cList = Physics.OverlapBox(boy.position, boy.GetComponent<BoxCollider>().bounds.extents, Quaternion.identity, LayerMask.GetMask("NavMesh"));
        //判断是否在navmesh中
        if (cList != null && cList.Length != 0)
        {
            inNavmesh = true;
            mb = cList[0].transform.parent.GetComponentInChildren<MeshBoard>();
        }
        else
        {
            inNavmesh = false;
            mb = null;
        }

        if (inNavmesh)
        {
            //寻路
            a = boy.transform.position + Vector3.up * 0.5f;
            b = cat.transform.position + Vector3.up * 0.3f;
            mb.Init();
            pathList = mb.FindPath(a, b);

            //计算方向：上下左右
            direction = pathList[1] - pathList[0];
        }

        //临时
        if (Input.GetKey(KeyCode.T))
        {
            Vector3 pos = GameManager.Instance.Player.transform.position = new Vector3(1544.39f, -30.73f, 5);
            GameManager.Instance.boy.transform.position = pos;
        }
    }
}
