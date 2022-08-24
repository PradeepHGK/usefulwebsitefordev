using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchFingerID : MonoBehaviour
{
    RaycastHit2D hit;
    Vector2[] touches = new Vector2[10];

    List<TouchObjects> touchObjects = new List<TouchObjects>();

    void Update()
    {
        if (Input.touchCount > 0)
        {
            foreach (Touch t in Input.touches)
            {
                touches[t.fingerId] = (Vector2)Camera.main.ScreenToWorldPoint(Input.GetTouch(t.fingerId).position);
                if (Input.GetTouch(t.fingerId).phase == TouchPhase.Began)
                {
                    hit = Physics2D.Raycast(touches[t.fingerId], Vector2.zero);

                    if (hit)
                    {
                        touchObjects.Add(new TouchObjects(hit.transform.gameObject, t.fingerId));
                    }
                }
                else if (Input.GetTouch(t.fingerId).phase == TouchPhase.Moved)
                {
                    TouchObjects touchObj = touchObjects.Find(touch => touch.fingerID == t.fingerId);
                    touchObj.selectedItem.transform.position = touches[t.fingerId];
                }
                else if (Input.GetTouch(t.fingerId).phase == TouchPhase.Ended)
                {
                    TouchObjects touchObj = touchObjects.Find(touch => touch.fingerID == t.fingerId);
                    touchObj.selectedItem.transform.position = touchObj.initPos;
                    touchObjects.RemoveAt(touchObjects.IndexOf(touchObj));
                }
            }
        }
    }

    void FinalDragAndDrop()
    {
        try
        {
            if (Input.touchCount > 0)
            {
                foreach (Touch t in Input.touches)
                {
                    touches[t.fingerId] = Camera.main.ScreenToWorldPoint(t.position);
                    Debug.Log($"fingerid: {t.fingerId}");
                    if (Input.GetTouch(t.fingerId).phase == TouchPhase.Began)
                    {
                        if (touches.Length > 0)
                        {
                            hit = Physics2D.Raycast(touches[t.fingerId], Vector2.zero);
                            if (hit.collider != null && hit.transform.gameObject.CompareTag("Drag"))
                            {
                                touchObjects.Add(new TouchObjects(hit.transform.gameObject, t.fingerId));
                            }
                        }
                    }
                    else if (Input.GetTouch(t.fingerId).phase == TouchPhase.Moved)
                    {
                        if (touchObjects.Count > 0)
                        {
                            TouchObjects touchObj = touchObjects.Find(touch => touch.fingerID == t.fingerId);
                            if (touchObj != null && touchObj.selectedItem != null)
                                touchObj.selectedItem.transform.position = touches[t.fingerId];
                        }
                    }
                    else if (Input.GetTouch(t.fingerId).phase == TouchPhase.Ended)
                    {
                        if (touchObjects.Count > 0)
                        {
                            TouchObjects touchObj = touchObjects.Find(touch => touch.fingerID == t.fingerId);

                            if (touchObj != null)
                            {
                                var _dragObj = touchObj.selectedItem.GetComponent<PuzzleDrag>();
                                var isThisMatch = _dragObj._canMatch;

                                //Debug.Log($"Distance: {Vector2.Distance(transform.position, _dragObj._CurrentSlot.transform.position)}");
                                var dist = Mathf.Abs(Vector2.Distance(transform.position, _dragObj._CurrentSlot.transform.position));
                                if (dist >= 2.5 && dist <= 3 && isThisMatch)
                                {
                                    touchObj.selectedItem.transform.localPosition = _dragObj._CurrentSlot.transform.localPosition;
                                    _dragObj._CurrentSlot.GetComponent<PuzzleSlot>().Placed();
                                }
                                else
                                {
                                    if (touchObj.selectedItem != null)
                                    {
                                        touchObj.selectedItem.transform.position = touchObj.initPos;
                                    }

                                    AudioManager.Instance.PlayAudio(SoundType.ErrorClick);
                                }

                                touchObjects.Clear();
                                Array.Clear(touches, t.fingerId, touches.Length);
                            }
                        }

                        if (touches.Length > 0)
                            Array.Clear(touches, 0, touches.Length);

                    }

                    if (Input.GetTouch(t.fingerId).phase == TouchPhase.Canceled)
                    {
                        Debug.Log($"Canceled");
                        //touchObjects.RemoveAt(touchObjects.IndexOf(touchObj));
                    }
                }
            }

        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }
}



[System.Serializable]
public class TouchObjects
{
    public GameObject selectedItem;
    public int fingerID;
    public Vector2 initPos;

    public TouchObjects(GameObject objSelected, int newFingerId)
    {
        this.fingerID = newFingerId;
        this.selectedItem = objSelected;
        this.initPos = objSelected.transform.position;
    }
}
