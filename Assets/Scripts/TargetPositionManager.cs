using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TargetPositionManager : MonoBehaviour
{
    //public bool targetCollided;
    //private bool entryFlag;
    Collider targetCollider;
    // Start is called before the first frame update
    void Start()
    {
        targetCollider = GetComponent<Collider>();
        //targetCollided = false;
        //entryFlag = false;
    }

    // Update is called once per frame
    void Update()
    {
        targetCollider.enabled = false;
        //GetCollisionFlag();
    }
    //public void OnTriggerEnter(Collider other)

    //{
    //    targetCollided= SetCollisionFlag(entryFlag);
    //}
    //private bool SetCollisionFlag(bool flag)
    //{
    //    flag = true;
    //    return flag;
    //}
    //public bool ResetCollisionFlag(bool flag)
    //{
    //    flag = false;
    //    return flag;
    //}
    //public bool GetCollisionFlag()
    //{
    //    return targetCollided;
    //}
}
