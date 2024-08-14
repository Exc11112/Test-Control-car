using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode()]
public class SiblingIndex : MonoBehaviour
{
    public int index;
    public GameObject targetParent; // ตัวแปรสำหรับกำหนด parent ที่ต้องการ

    private void Awake()
    {
        SetParentAndSiblingIndex();
    }

    private void FixedUpdate()
    {
        SetParentAndSiblingIndex();
    }

    private void SetParentAndSiblingIndex()
    {
        if (targetParent != null)
        {
            // ย้าย GameObject ไปอยู่ใต้ parent ที่กำหนด
            transform.SetParent(targetParent.transform);

            // ตั้งค่า Sibling Index ของ GameObject
            transform.SetSiblingIndex(index);
        }
    }
}