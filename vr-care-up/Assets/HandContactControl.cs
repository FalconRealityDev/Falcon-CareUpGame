using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HandContactControl : MonoBehaviour
{
    Dictionary<PickableObject, int> pickableInAreaCounters = new Dictionary<PickableObject, int>();
    PickupHighliteControl pickupHighliteControl;
    PlayerScript player;
    public bool isLeftHand = true;
    private void OnEnable()
    {
        ClearObjectsFromArea();
    }

    void Start()
    {
        pickupHighliteControl = GameObject.FindObjectOfType<PickupHighliteControl>();
        player = GameObject.FindObjectOfType<PlayerScript>();
    }

    private void OnTriggerEnter(Collider collision)
    {
        PickableObject pickableObject = collision.GetComponent<PickableObject>();
        if (pickableObject == null)
            return;
        HandPresence handWithThisObject = player.GetHandWithThisObject(pickableObject.gameObject);
        if (handWithThisObject != null)
        {
            if (isLeftHand == handWithThisObject.IsLeftHand())
                return;
        }
        
        AddObjectToArea(pickableObject);
        if (pickupHighliteControl != null)
            pickupHighliteControl.InitUpdateHighlite();
    }

    private void OnTriggerExit(Collider collision)
    {
        PickableObject pickableObject = collision.GetComponent<PickableObject>();
        if (pickableObject == null)
            return;
        RemoveObjectFromArea(pickableObject);
        if (pickupHighliteControl != null)
            pickupHighliteControl.InitUpdateHighlite();
    }

    public List<PickableObject> GetObjectsInArea()
    {
        List<PickableObject> pickableCurrentlyInArea = new List<PickableObject>();
        foreach(PickableObject p in pickableInAreaCounters.Keys)
        {
            if (pickableInAreaCounters[p] > 0)
                pickableCurrentlyInArea.Add(p);
        }

        return pickableCurrentlyInArea;
    }

    private void AddObjectToArea(PickableObject pickableObject)
    {
        if (pickableObject == null)
            return;
        if (pickableInAreaCounters.Keys.Contains(pickableObject))
            pickableInAreaCounters[pickableObject] += 1;
        else
            pickableInAreaCounters[pickableObject] = 1;
    }

    private void RemoveObjectFromArea(PickableObject pickableObject)
    {
        if (pickableInAreaCounters.Keys.Contains(pickableObject))
        {
            pickableInAreaCounters[pickableObject] -= 1;
            if (pickableInAreaCounters[pickableObject] <= 0)
                pickableInAreaCounters.Remove(pickableObject);
        }

        // if (pickableObject != null && (pickablesInArea.Contains(pickableObject)))
        //     pickablesInArea.Remove(pickableObject);
    }

    public void ClearObjectsFromArea()
    {
        pickableInAreaCounters.Clear();
        if (pickupHighliteControl != null)
            pickupHighliteControl.InitUpdateHighlite();
    }

    protected void OnDisable()
    {
        ClearObjectsFromArea();
    }
}
