using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode()]
public class SiblingIndex : MonoBehaviour
{
    public int index;
    public GameObject targetParent; // ���������Ѻ��˹� parent ����ͧ���

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
            // ���� GameObject ������� parent ����˹�
            transform.SetParent(targetParent.transform);

            // ��駤�� Sibling Index �ͧ GameObject
            transform.SetSiblingIndex(index);
        }
    }
}