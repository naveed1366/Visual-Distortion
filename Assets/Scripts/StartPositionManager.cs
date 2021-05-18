using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartPositionManager : MonoBehaviour
{
    //public bool startCollided;
    //private bool entryFlag;
    Collider startCollider;
    // Start is called before the first frame update
    void Start()
    {
        startCollider = GetComponent<Collider>();
        //startCollided = false;
        //entryFlag = false;
    }

    // Update is called once per frame
    void Update()
    {
        startCollider.enabled = false;
        //GetCollisionFlag();
    }
    //public void OnTriggerEnter(Collider other)

    //{
    //    startCollided=SetCollisionFlag(entryFlag);
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
    //    return startCollided;
    //}
}
