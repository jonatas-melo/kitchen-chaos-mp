using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    private Transform target;

    private void LateUpdate()
    {
        if (target == null) return;

        transform.position = target.position;
        transform.rotation = target.rotation;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
