using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;
using UnityEngine;

public class CurserFollower : MonoBehaviour
{
    public GameObject objectManager;
    [NonSerialized] public Vector3 worldPosition;
    private Vector3 frameVelocity;
    private Vector3 previousPosition;
    [NonSerialized] public Vector3 velocity;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Plane plane = new Plane(Vector3.up, 0);

        float distance;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out distance))
        {
            worldPosition = ray.GetPoint(distance);
        }

        bool distortionDecision = objectManager.GetComponent<ObjectManager>().GetDistortionDecision();
        if (distortionDecision == false)
        {
            transform.position = worldPosition;
        }
        else
        {
            Vector3 directionVector = worldPosition - objectManager.GetComponent<ObjectManager>().GetCurrentStartLocation();
            Vector3 NormalizedRight = Vector3.Cross(-directionVector, Vector3.up).normalized;
            float tangetValue = (float)Math.Tan(Math.PI / 6);// CHange this 
            transform.position = directionVector.magnitude * tangetValue * NormalizedRight + worldPosition;
        }
        
        velocity= velocityCalculator();
        getVelocity();
        GetCurserPosition();
    }
    public Vector3 GetCurserPosition()
    {
        return worldPosition;
    }
    public Vector3 velocityCalculator()
    {
        Vector3 currFrameVelocity = (transform.position - previousPosition) / Time.deltaTime;
        frameVelocity = Vector3.Lerp(frameVelocity, currFrameVelocity, 0.1f);
        previousPosition = transform.position;
        return frameVelocity;
    }
    public bool setVisuomotorDistortion(bool boolean)
    {
        return boolean;
    }
    public Vector3 getVelocity()
    {
        return velocity;
    }
    
}
