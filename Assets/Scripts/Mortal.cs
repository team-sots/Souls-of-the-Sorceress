﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D)),DisallowMultipleComponent]
public class Mortal : MonoBehaviour,IodoShiba.ManualUpdateClass.IManualUpdate
{
    public class DealtAttackInfo
    {
        public Mortal attacker;
        public AttackData attackData;
        public Vector2 relativePosition;

        public DealtAttackInfo(Mortal attacker, AttackData attackData,Vector2 relativePosition)
        {
            this.attacker = attacker;
            this.attackData = attackData;
            this.relativePosition = relativePosition;
        }
    }

    public class Viewer : MonoBehaviour
    {
        [SerializeField] Mortal target;
        protected float Health { get => target.health; }
        protected float MaxHealth { get => target.maxHealth; }
    }

    [SerializeField] protected float health;
    [SerializeField] protected float maxHealth;
    [SerializeField] private  bool isInvulnerable;
    [SerializeField] UnityEngine.Events.UnityEvent dyingCallbacks;
    [SerializeField] Rigidbody2D selfRigidbody;
    [SerializeField] List<AttackConverter> dealingAttackConverters;
    [SerializeField] List<AttackConverter> dealtAttackConverters;
    [SerializeField] float initialStunTime = 0.3f;

    AttackData argAttackData = new AttackData();
    GameObject argObj;
    System.Action<bool> argSucceedCallback;
    float leftStunTime = 0.3f;
    int dealtAttackCount = 0;
    List<DealtAttackInfo> dealtAttackInfos = new List<DealtAttackInfo>(4);
    Actor actor;

    public bool IsInvulnerable { get => isInvulnerable; set => isInvulnerable = value; }
    public Actor Actor { get => actor == null ? (actor = GetComponent<Actor>()) : actor; }

    protected virtual void Awake()
    {
        Actor.MortalUpdate = ManualUpdate;
    }
    protected virtual void OnAttacked(GameObject attackObj,AttackData attack) { }

    protected virtual void OnTriedAttack(Mortal attacker, AttackData dealt, in Vector2 relativePosition) { }

    public virtual void Dying() { Destroy(gameObject); }

    public void ConvertDealingAttack(AttackData dealee)
    {
        dealingAttackConverters.ForEach(dac => dac.Convert(dealee));
    }
    protected void ConvertDealtAttack(AttackData dealt)
    {
        //dealtAttackConverters.TrueForAll(dac => dac.Convert(dealt));
        dealtAttackConverters.ForEach(dac => dac.Convert(dealt));
    }
    

    public void TryAttack(GameObject argObj, AttackData argAttackData, System.Action<bool> succeedCallback)
    {
        this.argObj = argObj;
        AttackData.Copy(this.argAttackData, argAttackData);
        this.argSucceedCallback = succeedCallback;
    }
    public void TryAttack(Mortal attacker, AttackData argAttackData,in Vector2 relativePosition)
    {
        OnTriedAttack(attacker, argAttackData, relativePosition);

        if (isInvulnerable) { return; }

        if (dealtAttackCount >= dealtAttackInfos.Count)
        {
            dealtAttackInfos.Add(new DealtAttackInfo(attacker, new AttackData(argAttackData), relativePosition));
        }
        else if (dealtAttackInfos[dealtAttackCount] == null)
        {
            dealtAttackInfos[dealtAttackCount] = new DealtAttackInfo(attacker, new AttackData(argAttackData), relativePosition);
        }
        else
        {
            dealtAttackInfos[dealtAttackCount].attacker = attacker;
            AttackData.Copy(dealtAttackInfos[dealtAttackCount].attackData, argAttackData);
            dealtAttackInfos[dealtAttackCount].relativePosition = relativePosition;
        }
        dealtAttackCount++;
    }

    public void ManualUpdate()
    {
        if (dealtAttackCount == 0) { return; }

        float rxsum = 0;
        AttackData result = new AttackData();
        for (int i = 0; i < dealtAttackCount; ++i) //1フレームの間に与えられた複数の攻撃と相対座標を統合する
        {
            dealtAttackConverters.ForEach(dac => dac.Convert(dealtAttackInfos[i].attackData));//攻撃の変換
            result.damage += dealtAttackInfos[i].attackData.damage;//ダメージ
            result.knockBackImpulse
                += new Vector2(
                    -Mathf.Sign(dealtAttackInfos[i].relativePosition.x)*dealtAttackInfos[i].attackData.knockBackImpulse.x,
                    dealtAttackInfos[i].attackData.knockBackImpulse.y);//ノックバック

            if(result.hitstopSpan < dealtAttackInfos[i].attackData.hitstopSpan)
            {
                result.hitstopSpan = dealtAttackInfos[i].attackData.hitstopSpan;
            }//ヒットストップ
            rxsum += dealtAttackInfos[i].relativePosition.x;
        }
        dealtAttackCount = 0;//攻撃を全て統合したのでカウンターを0にし、与えられた攻撃を忘却する

        if (result.damage <= 0 && result.knockBackImpulse.magnitude < 0.01) { return; }//攻撃が無意味ならば処理を中断

        health -= result.damage; //体力を減算する
        selfRigidbody.velocity = Vector2.zero; //Actorの動きを止める
        selfRigidbody.AddForce(
            result.knockBackImpulse,
            ForceMode2D.Force); //ノックバックを与える

        //ヒットストップを与える（未実装）

        actor.OnAttacked.Invoke();//被攻撃時のコールバック関数を呼び出し
        
        
    }
}


//[RequireComponent(typeof(Rigidbody2D)), DisallowMultipleComponent]
//public class Mortal : PassiveBehaviour, ActorBehaviour.IParamableWith<GameObject, AttackData, System.Action<bool>>
//{
//    [SerializeField] protected float health;
//    [SerializeField] protected float maxHealth;
//    [SerializeField] UnityEngine.Events.UnityEvent dyingCallbacks;
//    [SerializeField] Rigidbody2D selfRigidbody;
//    [SerializeField] List<AttackConverter> dealingAttackConverters;
//    [SerializeField] List<AttackConverter> dealtAttackConverters;

//    AttackData argAttackData = new AttackData();
//    GameObject argObj;
//    System.Action<bool> argSucceedCallback;
//    float leftStunTime = 0.3f;
//    [SerializeField, UnityEngine.Serialization.FormerlySerializedAs("leftStunTime")] float initialStunTime = 0.3f;

//    protected virtual void OnAttacked(GameObject attackObj, AttackData attack) { }
//    protected virtual bool IsInvulnerable() { return false; }
//    public virtual void Dying() { Destroy(gameObject); }

//    public void ConvertDealingAttack(AttackData dealee)
//    {
//        dealingAttackConverters.ForEach(dac => dac.Convert(dealee));
//    }
//    protected void ConvertDealtAttack(AttackData dealt)
//    {
//        dealtAttackConverters.TrueForAll(dac => dac.Convert(dealt));
//    }

//    protected override bool ShouldContinue(bool ordered)
//    {
//        leftStunTime -= Time.deltaTime;
//        if (leftStunTime < 0)
//        {
//            leftStunTime = 0;
//            return false;
//        }
//        return true;
//    }

//    protected override void OnInitialize()
//    {
//        _OnAttackedInternal(argObj, argAttackData);
//    }

//    private void _OnAttackedInternal(GameObject attackerObj, AttackData givenData)
//    {
//        if (!IsInvulnerable())
//        {
//            OnAttacked(attackerObj, givenData);
//            int kbdir = System.Math.Sign(transform.position.x - attackerObj.transform.position.x);
//            givenData.knockBackImpact.x *= kbdir;
//            //ConvertDealtAttack(data);
//            if (!dealtAttackConverters.TrueForAll(dac => dac.Convert(givenData)))
//            {
//                if (argSucceedCallback != null) argSucceedCallback(false);
//            }

//            health -= givenData.damage;

//            leftStunTime = initialStunTime;

//            selfRigidbody.velocity = Vector2.zero;
//            selfRigidbody.AddForce(givenData.knockBackImpact);

//            Debug.Log(gameObject.name + " damaged");

//            if (health <= 0)
//            {
//                dyingCallbacks.Invoke();
//                Dying();
//            }

//            if (argSucceedCallback != null)
//            {
//                argSucceedCallback(true);
//            }
//        }
//        else if (argSucceedCallback != null)
//        {
//            argSucceedCallback(false);
//        }


//    }

//    public void SetParams(GameObject argObj, AttackData argAttackData, System.Action<bool> succeedCallback)
//    {
//        this.argObj = argObj;
//        AttackData.DeepCopy(this.argAttackData, argAttackData);
//        this.argSucceedCallback = succeedCallback;
//        if (Activated) { _OnAttackedInternal(argObj, argAttackData); }
//    }
//}
