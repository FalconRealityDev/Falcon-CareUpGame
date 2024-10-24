﻿using System.Collections.Generic;
using UnityEngine;
using CareUpAvatar;

public class CharacterСarousel : MonoBehaviour
{
    public int CurrentCharacter = 1;
    public CharacterPanelManager panelManager;
    public List<GameObject> platforms;

    private float turnAngle = 0;
    private int behindMarker = 3;
    private int nextTurnDir = 0;
    private int turnDir = 0;
    private int targetTurnPosition = -1;
    private float defaultTurnSpeed = 90f;

    static int current;

    private PlayerPrefsManager pref;

    public List<PlayerAvatar> avatars = new List<PlayerAvatar>();
    public List<GameObject> checkMarks = new List<GameObject>();

    public void UpdateSelected(PlayerAvatarData aData)
    {
        int currentMarker = GetCurrentMarker();
        avatars[currentMarker].avatarData = aData;
        avatars[currentMarker].UpdateCharacter();
        //GameObject.FindObjectOfType<LoadCharacterScene>().LoadCharacter(avatars[GetCurrentMarker()]);
    }

    public void Initialize()
    {
        int current = CurrentCharacter - 1;
        foreach (PlayerAvatar avatar in avatars)
        {
            PlayerAvatarData playerAvatarData = GetAvatarData(current);
            if (playerAvatarData != null)
            {
                avatar.avatarData = playerAvatarData;
                avatar.UpdateCharacter();
                bool purchased = PlayerPrefsManager.storeManager.CharacterItems[current].purchased;
                checkMarks[current].SetActive(purchased);
                //panelManager.SetStoreInfo(behindMarker, current);
            }
            avatar.SetAnimationAction(Actions.Idle, true);
            current++;
        }
        panelManager.SetStoreInfo(CurrentCharacter);

        if (pref != null)
        {
            ImmediateTurnToPos(PlayerPrefsManager.storeManager.GetPositionFromIndex(CharacterInfo.index));
        }
    }

    public void Scroll(int dir)
    {
        int nextChar = CurrentCharacter + dir;
        if (nextChar >= 0 && nextChar < PlayerPrefsManager.storeManager.CharacterItems.Count)
            nextTurnDir = dir;
        targetTurnPosition = -1;

        // enabled = true;
    }

    public int GetCurrentMarker()
    {
        int currentMarker = behindMarker - 2;
        if (currentMarker < 0)
            currentMarker = 4 + currentMarker;
        return currentMarker;
    }

    private void Start()
    {
        pref = GameObject.FindObjectOfType<PlayerPrefsManager>();
        // checkMarks.Clear();
        // foreach (GameObject platform in platforms)
        // {
        //     checkMarks.Add(platform.transform.Find("checkMark").gameObject);
        // }
        // foreach (GameObject platform in platforms)
        // {
        //     avatars.Add(platform.transform.Find("PlayerAvatar").GetComponent<PlayerAvatar>());
        // }

        Invoke("Initialize", 0.1f);
    }

//    void OnGUI()
//    {
//#if UNITY_EDITOR || DEVELOPMENT_BUILD
//        GUIStyle style = new GUIStyle();
//        style.normal.textColor = new Color(1f, 0f, 0f);
//        style.fontSize = 30;
//        string ss = targetTurnPosition.ToString() + " " + CurrentCharacter.ToString();
//        GUI.Label(new Rect(0, 0, 100, 100), ss, style);

//#endif
//    }

    public void SetAnimation()
    {
        avatars[GetCurrentMarker()].SetAnimationAction(Actions.Dance);
        checkMarks[GetCurrentMarker()].SetActive(PlayerPrefsManager.storeManager.CharacterItems[CurrentCharacter].purchased);
    }

    private PlayerAvatarData GetAvatarData(int index)
    {
        if (index >= 0 && index < (PlayerPrefsManager.storeManager.CharacterItems.Count))
        {
            //panelManager.SetStoreInfo(behindMarker, index);
            return PlayerPrefsManager.storeManager.CharacterItems[index].playerAvatar;
        }
        return null;
    }

    public void TurnToPosition(int value)
    {
        targetTurnPosition = value;
    }

    public void ImmediateTurnToPos(int value)
    {
        CurrentCharacter = value;
        int CurrentMarker = GetCurrentMarker();
        avatars[CurrentMarker].avatarData = GetAvatarData(CurrentCharacter);
        avatars[CurrentMarker].UpdateCharacter();
        checkMarks[CurrentMarker].SetActive(PlayerPrefsManager.storeManager.CharacterItems[CurrentCharacter].purchased);
        int leftMarker = CurrentMarker - 1;
        if (leftMarker < 0)
            leftMarker = 3;
        PlayerAvatarData leftAvatar = GetAvatarData(CurrentCharacter - 1);
        platforms[leftMarker].SetActive(leftAvatar != null);
        if (leftAvatar != null)
        {
            avatars[leftMarker].avatarData = leftAvatar;
            avatars[leftMarker].UpdateCharacter();
            checkMarks[leftMarker].SetActive(PlayerPrefsManager.storeManager.CharacterItems[CurrentCharacter - 1].purchased);
        }
        int rightMarker = CurrentMarker + 1;
        if (rightMarker > 3)
            rightMarker = 0;
        PlayerAvatarData rightAvatar = GetAvatarData(CurrentCharacter + 1);
        platforms[rightMarker].SetActive(rightAvatar != null);
        if (rightAvatar != null)
        {
            avatars[rightMarker].avatarData = rightAvatar;
            avatars[rightMarker].UpdateCharacter();
            checkMarks[rightMarker].SetActive(PlayerPrefsManager.storeManager.CharacterItems[CurrentCharacter + 1].purchased);
        }
    }

    private void Update()
    {
        float turnSpeed = defaultTurnSpeed;
        if (targetTurnPosition != -1)
        {
            if (targetTurnPosition == CurrentCharacter)
                targetTurnPosition = -1;
            else
            {
                if (targetTurnPosition < CurrentCharacter)
                    nextTurnDir = -1;
                else if (targetTurnPosition > CurrentCharacter)
                    nextTurnDir = 1;
                //turnSpeed = defaultTurnSpeed * 2;
            }
        }
        if (turnDir == 0 && nextTurnDir != 0)
        {
            turnDir = nextTurnDir;
            nextTurnDir = 0;
            CurrentCharacter += turnDir;
            int index = CurrentCharacter + 1;

            PlayerAvatarData playerAvatar = GetAvatarData(index);
            if (turnDir < 0)
            {
                index = CurrentCharacter - 1;
                playerAvatar = GetAvatarData(index);
            }
            if (playerAvatar != null)
            {
                avatars[behindMarker].avatarData = playerAvatar;
                avatars[behindMarker].UpdateCharacter();
                bool purchased = PlayerPrefsManager.storeManager.CharacterItems[index].purchased;
                checkMarks[behindMarker].SetActive(purchased);
            }
            panelManager.SetStoreInfo(CurrentCharacter);
            platforms[behindMarker].SetActive(playerAvatar != null);

            behindMarker += turnDir;

            if (behindMarker > 3)
                behindMarker = 0;
            else if (behindMarker < 0)
                behindMarker = 3;

            foreach (PlayerAvatar a in avatars)
            {
                a.SetAnimationAction(Actions.Idle, false);
            }
        }
        if (turnDir != 0)
        {
            Vector3 rot = transform.rotation.eulerAngles;
            float nextAngle = (turnAngle + (turnSpeed * turnDir)) % 360;
            if (nextAngle < 0)
                nextAngle = 360 + nextAngle;
            if (Mathf.Abs(rot.y - nextAngle) < (turnSpeed / 6))
            {
                rot.y = nextAngle;
                turnDir = 0;
                turnAngle = nextAngle;
                if (pref != null)
                    pref.CarouselPosition = CurrentCharacter;
                if (nextTurnDir == 0)
                {
                    int currentMarker = GetCurrentMarker();
                    avatars[currentMarker].SetAnimationAction(Actions.Posing);
                }
            }
            else
            {
                rot.y += turnDir * 300f * Time.deltaTime;
            }
            transform.rotation = Quaternion.Euler(rot.x, rot.y, rot.z);
        }
    }
}
