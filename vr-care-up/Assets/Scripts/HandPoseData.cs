using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands.Samples.VisualizerSample;

public class HandPoseData : MonoBehaviour
{
    public enum HandModelType {  Left, Right}
    public HandModelType handType;
    public Transform root;
    public Animator animator;
    public Transform[] fingerBones; 
    public Transform rootBone;
    private Vector3 baseRootBonePosition;
    private Quaternion baseRootBoneRotation;

    /*[SerializeField]
    GameObject debugJointPrefab;
    GameObject[] debugJoints;*/

    HandVisualizer.HandGameObjects m_HandGameObjects;

    private void Start()
    {
        baseRootBonePosition = rootBone.localPosition;
        baseRootBoneRotation = rootBone.localRotation;

        /*debugJoints = new GameObject[colliderBones.Length];
        for (int i = 0; i < debugJoints.Length; i++)
        {
            debugJoints[i] = Instantiate<GameObject>(debugJointPrefab, colliderBones[i]);
            debugJoints[i].transform.localScale = Vector3.one * 0.02f;
        }*/
    }

    public Vector3 GetBaseRootBonePosition()
    {
        return baseRootBonePosition;
    }

    public Quaternion GetBaseRootBoneRotation()
    {
        return baseRootBoneRotation;

    }
    
    public Vector3 GetParentOffset()
    {
        return transform.parent.parent.localPosition;
    }

    void Update()
    {
        
    }
}
