﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static System.Math;

//原因不明の動作不良の疑いあり
//動作不良の内容…たまに十分な高さまでジャンプしなかったり、高くジャンプしすぎたりする
//再現ができず、原因の検証ができていない
public class Jump : BasicAbility
{
    [SerializeField] float jumpSpeed;
    [SerializeField] float maxPushForceMagnitude;
    [SerializeField] float maxJumpHeight;
    [SerializeField] Rigidbody2D targetRigidbody;
    float jumpBorder;
    Transform targetTransform;
    float f;
    float t = 0;
    
    // Use this for initialization
    private void Awake()
    {
        targetTransform = targetRigidbody.transform;
    }

    protected override void OnInitialize()
    {
        jumpBorder = targetTransform.position.y + maxJumpHeight;
    }

    public override bool ContinueUnderBlocked => true;

    protected override bool ShouldContinue(bool ordered)
    {
        return ordered && targetTransform.position.y < jumpBorder && !(t > 0.01 && targetRigidbody.velocity.y <= 0);
    }

    protected override void OnActive(bool ordered)
    {
        f = Min(targetRigidbody.mass * (jumpSpeed - targetRigidbody.velocity.y) / Time.deltaTime,
                        maxPushForceMagnitude);
        t += Time.deltaTime;
        Debug.Log("Jumping"+Activated);
    }

    protected override void OnTerminate()
    {
        t = 0;
    }

    private void FixedUpdate()
    {
        if (!Activated) { return; }
        targetRigidbody.AddForce(f * Vector2.up);
    }
}
