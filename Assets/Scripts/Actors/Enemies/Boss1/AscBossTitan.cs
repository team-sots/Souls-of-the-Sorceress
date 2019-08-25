﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActorBossTitan
{
    public class AscBossTitan : FightActorStateConector
    {
        // MEMO:ボスの仕様
        // 前提.
        // ボスの居るシーンには地面と、左右対になったすり抜け床が2対配置
        // 地面とすり抜け床、ステージ中央で分断された空間に左下から左、右、下から上の順に1ずつ番号を振る。すなわち
        //
        //  5   6
        // --- ---
        //  3   4
        // --- ---
        //  1   2
        // =======
        //
        // ボスの行動.
        // 1.プレイヤーとボスが同列にいる
        //   ->プレイヤーの居るところまでジャンプor落下
        // 2.プレイヤーとボスが同列に居ないorプレイヤーとボスがともに地面に居る
        //   ->プレイヤーに突進
        //     突進はステージ中央付近まで直進->プレイヤーの居る足場まで（必要なら）ジャンプ->壁まで直進->壁衝突時に「気絶」状態へ

        [SerializeField] GroundSensor wallSensor1;
        [SerializeField] GroundSensor wallSensor2;

        [SerializeField] string currentStateName;
        [SerializeField] Boss1Default boss1Default;
        [SerializeField] Fainted fainted;
        [SerializeField] RecoverFromFaint recoverFromFaint;
        [SerializeField] Boss1Smashed smashed;
        [SerializeField] Dead dead;

        public override SmashedState Smashed => smashed;

        public override ActorState DefaultState => boss1Default;


        protected override void BuildStateConnection()
        {
            ConnectStateFromDefault(() => wallSensor1.IsOnGround && wallSensor2.IsOnGround, fainted);
            ConnectState(fainted.JumpToRecover, fainted, recoverFromFaint);
        }

        protected override void BeforeStateUpdate()
        {
            currentStateName = Current.GetType().Name;
        }

        public void OnWeakpointDestroyed()
        {
            InterruptWith(dead);
        }

        [System.Serializable]
        class Boss1Default : DefaultState
        {
            [SerializeField] BossTitanAI titanAI;

            [SerializeField] ActorFunction.HorizontalMove horizontalMove;
            [SerializeField] ActorFunction.Jump jump0Step;
            [SerializeField] ActorFunction.Jump jump1Step;
            [SerializeField] ActorFunction.Jump jump2Step;
            [SerializeField] PassPlatform passPlatform;
            [SerializeField] ActorFunction.Directionable directionable;

            //protected override bool ShouldCotinue()
            //{
            //    return !wallSensor1.IsOnGround || !wallSensor2.IsOnGround;
            //}

            protected override void OnActive()
            {

                titanAI.Decide();

                horizontalMove.ManualUpdate(titanAI.MoveDirection);
                switch (titanAI.JumpUpRowsCount)
                {
                    case 0:
                        jump0Step.Update(titanAI.DoJump);
                        break;

                    case 1:
                        jump1Step.Update(titanAI.DoJump);
                        break;

                    case 2:
                        jump2Step.Update(titanAI.DoJump);
                        break;

                    case -1:
                    case -2:
                        passPlatform.Use(titanAI.DoPassPlatform);
                        break;
                }
            }
        }

        [System.Serializable]
        class Boss1Smashed : SmashedState
        {

        }

        [System.Serializable]
        class Fainted : ActorState
        {
            [SerializeField] float span;
            [SerializeField] BossTitanAI titanAI;
            [SerializeField] EnemySpawner enemySpawner;
            [SerializeField] Collider2D hitboxCollider;
            [SerializeField] Mortal weakPoint;
            [SerializeField] SpriteRenderer _spriteRenderer;

            IodoShibaUtil.ManualUpdateClass.ManualClock manualClock = new IodoShibaUtil.ManualUpdateClass.ManualClock();

            public float Span { get => span; }

            protected override bool ShouldCotinue() => true;

            public bool JumpToRecover() => manualClock.Clock >= span;
            protected override void OnInitialize()
            {
                manualClock.Reset();
                titanAI.enabled = false;
                hitboxCollider.enabled = false;
                weakPoint.IsInvulnerable = false;
                Debug.Log("Titan Boss is fainted.");

                //一時的
                _spriteRenderer.color = new Color(1, 1, 1, .5f);

                enemySpawner.Spawn();
            }

            protected override void OnActive()
            {
                manualClock.Update();
            }

            protected override void OnTerminate(bool isNormal)
            {
                weakPoint.IsInvulnerable = true;
                manualClock.Reset();

                //一時的
                _spriteRenderer.color = Color.white;
            }
        }

        [System.Serializable]
        class RecoverFromFaint : ActorState
        {
            [SerializeField] float span;
            [SerializeField] BossTitanAI titanAI;
            [SerializeField] ActorFunction.Directionable directionable;
            [SerializeField] Collider2D hitboxCollider;
            [SerializeField] Mortal weakPoint;

            IodoShibaUtil.ManualUpdateClass.ManualClock manualClock = new IodoShibaUtil.ManualUpdateClass.ManualClock();

            protected override bool ShouldCotinue() => manualClock.Clock < span;

            protected override void OnInitialize()
            {
                manualClock.Reset();
                directionable.SwitchDirection();
            }

            protected override void OnActive()
            {
                manualClock.Update();
            }

            protected override void OnTerminate(bool isNormal)
            {
                manualClock.Reset();
                titanAI.enabled = true;
                hitboxCollider.enabled = true;
            }
        }

        [System.Serializable]
        class Dead : ActorState
        {
            [SerializeField] SpriteRenderer _spriteRenderer;

            protected override bool ShouldCotinue() => true;
            protected override void OnInitialize()
            {
                Debug.Log("Boss Titan has dead.");
                _spriteRenderer.color = new Color(.5f,0,0,1);
            }
        }
    }
}
