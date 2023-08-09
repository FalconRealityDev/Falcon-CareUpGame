using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    const float OBJ_TRANS_DURATION = 0.2f;
    public AnimHandsTransform animHandsTransform;
    private Animator animHandsAnimator;
    public string currentWTGName = "";
    private HandPoseControl leftHandPoseControl;
    private HandPoseControl rightHandPoseControl;
    private Transform mainCameraTransform;
    public Animation fadeAnimation;

    public GameObject LongRangeLeftRay;
    public GameObject LongRangeRightRay;

    public GameObject ShortRangeLeftRay;
    public GameObject ShortRangeRightRay;

    private HandPresence leftHandPresence;
    private HandPresence rightHandPresence;
    private GameUIVR gameUIVR;

    public HandPresence GetHandWithThisObject(GameObject obj)
    {
        if (GetObjectInHand(true) == obj)
            return leftHandPresence;
        if (GetObjectInHand(false) == obj)
            return rightHandPresence;
        return null;
    }

    public bool Away()
    {
        return currentWTGName == "";
    }

    public void DropFromHand(bool leftHand)
    {
        if (leftHand && leftHandPresence != null)
            leftHandPresence.DropObjectFromHand();
        else if (!leftHand && rightHandPresence != null)
            rightHandPresence.DropObjectFromHand();

    }

    public GameObject GetObjectInHand(bool isLeft)
    {
        if (isLeft && leftHandPresence != null)
            return leftHandPresence.GetObjectInHand();
        if (!isLeft && rightHandPresence != null)
            return rightHandPresence.GetObjectInHand();
        return null;
    }

    public void AddHandPresence(bool isLeft, HandPresence hp)
    {
        if (isLeft)
            leftHandPresence = hp;
        else
            rightHandPresence = hp;
    }

    public ActionTrigger.TriggerHandAction GetCurrentHandPose(bool isLeftHand)
    {
        if (isLeftHand && leftHandPresence != null)
            return leftHandPresence.GetCurrentHandPose();
        if (!isLeftHand && rightHandPresence != null)
            return rightHandPresence.GetCurrentHandPose();
        return ActionTrigger.TriggerHandAction.None;
    }

    public void ForcePickUpObject(PickableObject pObject, bool isLeftHand)
    {
        if (pObject == null)
            return;
        HandPresence currentHand = rightHandPresence;
        if (isLeftHand)
            currentHand = leftHandPresence;

        currentHand.PickUpObject(pObject, true);
        if (leftHandPoseControl.handPoseMode == HandPoseControl.HandPoseMode.CopyAnimIn)
        {
            ObjectsInHandsFallowAnimation(true);
        }
    }

    public void SwitchHands()
    {
        GameObject leftObject = GetObjectInHand(true);
        GameObject rightObject = GetObjectInHand(false);
        string mess = "@switch hands:";
        if (leftObject != null)
        {
            mess += leftObject.name + " ";
            leftHandPresence.DropObjectFromHand(true, true);
            ForcePickUpObject(leftObject.GetComponent<PickableObject>(), false);
        }
        if (rightObject != null)
        {
            mess += rightObject.name;

            rightHandPresence.DropObjectFromHand(true, true);
            ForcePickUpObject(rightObject.GetComponent<PickableObject>(), true);
        }
        Debug.Log(mess);

}


    public void ForceDeleteObjectFromHand(bool isLeftHand)
    {
        GameObject objInHand = null;
        HandPresence currentHand = rightHandPresence;
        if (isLeftHand && leftHandPresence != null)
        {
            objInHand = leftHandPresence.GetObjectInHand();
            currentHand = rightHandPresence;
        }
        else if (!isLeftHand && rightHandPresence != null)
        {
            objInHand = rightHandPresence.GetObjectInHand();
            currentHand = rightHandPresence;
        }
        if (objInHand == null)
            return;
        currentHand.DropObjectFromHand(true);
        Destroy(objInHand);

    }

    public void EnableRaycastControllers(bool toEnable)
    {
        LongRangeLeftRay.SetActive(toEnable);
        LongRangeRightRay.SetActive(true);//toEnable);
        ShortRangeLeftRay.SetActive(toEnable);
        ShortRangeRightRay.SetActive(toEnable);
    }


    public void SetAnimationSpeed(float speed)
    {
        animHandsAnimator.speed = speed;
    }

    public bool TriggerAction(string triggerName, GameObject cinematicTarget = null, bool mirrorAnimation = false)
    {
        if (leftHandPoseControl == null || rightHandPoseControl == null)
            return false;
        if (IsInCopyAnimationState())
            return false;
        if (leftHandPoseControl.handPoseMode != HandPoseControl.HandPoseMode.Default ||
            rightHandPoseControl.handPoseMode != HandPoseControl.HandPoseMode.Default )
            return false;

        if (cinematicTarget != null)
        {
            // fadeAnimation.Play();
            Vector3 zVec = new Vector3(1f, 0f, 1f);
            animHandsTransform.fallowVRCamera = false;
            animHandsTransform.transform.position = cinematicTarget.transform.position;
            animHandsTransform.transform.rotation = cinematicTarget.transform.rotation;
            Vector3 cinPos = cinematicTarget.transform.position;
            Vector3 camPos = mainCameraTransform.position;
            Vector3 pos = transform.position;
            Vector3 newPos = cinPos - (camPos - pos);
            newPos.y = transform.position.y;
            transform.position = newPos;

            Vector3 cinForward = cinematicTarget.transform.forward;
            Vector3 camForward = Vector3.Scale(mainCameraTransform.forward, zVec).normalized;
            float rotAngle = Vector3.Angle(cinForward, camForward);
            // transform.RotateAround(transform.position, Vector3.up, rotAngle);
        }
        leftHandPoseControl.SetupCopyAnimationData(mirrorAnimation);
        rightHandPoseControl.SetupCopyAnimationData(mirrorAnimation);
        // EnableRaycastControllers(false);
        ObjectsInHandsFallowAnimation();
        animHandsAnimator = animHandsTransform.animator;
        animHandsAnimator.SetTrigger(triggerName);
        if (gameUIVR == null)
            gameUIVR = GameObject.FindObjectOfType<GameUIVR>();
        gameUIVR.UpdateHelpHighlight();
        return true;
    }

    private void ObjectsInHandsFallowAnimation(bool noTransition = false)
    {
        float transuitionDuration = OBJ_TRANS_DURATION;
        if (noTransition)
            transuitionDuration = 0.001f;

        GameObject objectInLeft = GetObjectInHand(true);
        if (objectInLeft != null)
        {
            Transform leftToolTransform = animHandsTransform.GetToolHoldBoneTransform(true);
            if (leftToolTransform != null)
                objectInLeft.GetComponent<PickableObject>().FallowTransform(leftToolTransform, transuitionDuration);
        }
        GameObject objectInRight = GetObjectInHand(false);
        if (objectInRight != null)
        {
            Transform rightToolTransform = animHandsTransform.GetToolHoldBoneTransform(false);
            if (rightToolTransform != null)
                objectInRight.GetComponent<PickableObject>().FallowTransform(rightToolTransform, transuitionDuration);
        }
    }


    public bool IsInCopyAnimationState()
    {
        if (leftHandPoseControl != null)
        {
            if (leftHandPoseControl.handPoseMode == HandPoseControl.HandPoseMode.CopyAnimIn ||
                leftHandPoseControl.handPoseMode == HandPoseControl.HandPoseMode.CopyAnimOut)
                return true;
        }
        if (rightHandPoseControl != null)
        {
            if (rightHandPoseControl.handPoseMode == HandPoseControl.HandPoseMode.CopyAnimIn ||
                rightHandPoseControl.handPoseMode == HandPoseControl.HandPoseMode.CopyAnimOut)
                return true;
        }
        return false;
    }

    public void ExitCopyAnimationState()
    {
    if (leftHandPoseControl == null || rightHandPoseControl == null)
            return;
        
        GameObject objectInLeft = GetObjectInHand(true);
        if (objectInLeft != null)
            objectInLeft.GetComponent<PickableObject>().FallowTransform(
                leftHandPresence.GetHandPoseControl().objectHolder, 
                OBJ_TRANS_DURATION);
        GameObject objectInRight = GetObjectInHand(false);
        if (objectInRight != null)
            objectInRight.GetComponent<PickableObject>().FallowTransform(
                rightHandPresence.GetHandPoseControl().objectHolder, 
                OBJ_TRANS_DURATION);

        if (objectInLeft != null && objectInLeft.GetComponent<GrabHandPose>() != null &&
            objectInLeft.GetComponent<GrabHandPose>().leftHandPose != null)
            leftHandPoseControl.ExitCopyAnimationState(objectInLeft.GetComponent<GrabHandPose>().leftHandPose, 0.5f);
        else
            leftHandPoseControl.ExitCopyAnimationState();
        
        if (objectInRight != null && objectInRight.GetComponent<GrabHandPose>() != null &&
            objectInRight.GetComponent<GrabHandPose>().righHandPose != null)
            rightHandPoseControl.ExitCopyAnimationState(objectInRight.GetComponent<GrabHandPose>().righHandPose, 0.5f);     
        else
            rightHandPoseControl.ExitCopyAnimationState();     
        animHandsTransform.fallowVRCamera = true;
        // EnableRaycastControllers(true);
    }

    public void AddHandPoseControl(HandPoseControl control, bool isLeftHand)
    {
        if (!isLeftHand)
            rightHandPoseControl = control;
        else
            leftHandPoseControl = control;
    }

    private void Start()
    {
        mainCameraTransform = transform.Find("Camera Offset/Main Camera");
        animHandsAnimator = animHandsTransform.transform.GetComponentInChildren<Animator>();
    }
    
    private void Update()
    {
        EnableRaycastControllers(!IsInCopyAnimationState());
    }

    public void UpdateWalkToGroup(string WTGName)
    {
        currentWTGName = WTGName;
        if (gameUIVR != null)
            gameUIVR.UpdateHelpWitDelay(1f);
        ActionManager.UpdateRequirements();
    }

}
