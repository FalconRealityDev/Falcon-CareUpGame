using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;


public class PickableObject : MonoBehaviour
{
    private float poseTransitionDuration = 0.2f;
    private float routineTime = float.PositiveInfinity;

    private Vector3 startPos;
    private Quaternion startRotation;


    bool isKinematic = false;
    private PlayerScript player;
    private Transform transformToFallow;

    public void Drop()
    {
        Debug.Log("@ " + name + ": Drop");
        transformToFallow = null;
        if (gameObject.GetComponent<Rigidbody>() != null)
            gameObject.GetComponent<Rigidbody>().isKinematic = isKinematic;
        MountDetector mountDetector = gameObject.GetComponentInChildren<MountDetector>();
        if (mountDetector != null)
        {
            Transform closestMount = mountDetector.FindClosestMount();
            if (closestMount != null)
                AttatchToMount(closestMount);
        }
}
    
    public bool PickUp(Transform handTransform, float transuitionDuration = 0.2f)
    {
        FallowTransform(handTransform, transuitionDuration);
        if (gameObject.GetComponent<Rigidbody>() != null)
            gameObject.GetComponent<Rigidbody>().isKinematic = true;
        return true;
    }

    public void FallowTransform(Transform trans, float transuitionDuration = 0.2f)
    {
        poseTransitionDuration = transuitionDuration;
        startPos = transform.position;
        startRotation = trans.rotation;
        routineTime = 0f;
        transformToFallow = trans;
    }

    private void Update()
    {
        float lerpValue = routineTime / poseTransitionDuration;
        if (transformToFallow != null)
        {
            Vector3 p = Vector3.Lerp(startPos, transformToFallow.position, lerpValue);
            Quaternion r = Quaternion.Lerp(startRotation, transformToFallow.rotation, lerpValue);

            transform.position = p;
            transform.rotation = r;

        }
        routineTime += Time.deltaTime;
    }

    void AttatchToMount(Transform mount)
    {
        transform.SetParent(mount);
        transform.position = mount.position;
        transform.rotation = mount.rotation;
        if (gameObject.GetComponent<Rigidbody>() != null)
            gameObject.GetComponent<Rigidbody>().isKinematic = true;
    }

    void Awake()
    {
        if (gameObject.GetComponent<Rigidbody>() != null)
        {
            isKinematic = gameObject.GetComponent<Rigidbody>().isKinematic;
            if (transform.parent != null && transform.parent.tag == "MountingPoint")
            {
                gameObject.GetComponent<Rigidbody>().isKinematic = true;
            }
        }
    }

    private void Start()
    {
        player = GameObject.FindObjectOfType<PlayerScript>();
    }
}
