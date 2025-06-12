using System;
using UnityEngine;

[Serializable]
public struct BoneLimit
{
    [Tooltip("Drag the bone Transform here.")]
    public Transform bone;

    [Header("Minimum Local Euler Angles (°)")]
    [Tooltip("Copy your bone’s Local Rotation (x,y,z) and paste here for the minimum clamp.")]
    public Vector3 minLocalEuler;

    [Header("Maximum Local Euler Angles (°)")]
    [Tooltip("Copy your bone’s Local Rotation (x,y,z) and paste here for the maximum clamp.")]
    public Vector3 maxLocalEuler;
}
