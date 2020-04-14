﻿/*
 * for the flying
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(BoxCollider2D))]
public class MonsterAI_03 : MonoBehaviour
{
    protected enum AI_State
    {
        Stand = 0,
        Walk,
        Warning,
        Battle,
        Return
    }
    [SerializeField]
    protected Monster monster;  // access the monster script
    [SerializeField]
    protected Controller2D controller;

    protected Vector3 position; // record monster's position;
    protected float distance;   // record the dis with spawn point
    [SerializeField]
    protected float reDistance = 10;    //  record the max battle distance
    [SerializeField]
    protected float sustainTime, speed, reationTime = 1, jumpForce;
    [SerializeField]
    protected AI_State ai = AI_State.Stand;

    public Vector3 spawnPosition;   //  record the spawn point

    private bool isFirst;
    private int proThesold;
    private float flyingTheshold;
    RaycastHit2D hit;
    [SerializeField]
    float seeDistance = 0;  //  the monster's max sight distance
    [SerializeField]
    LayerMask collisionMask, collisionMaskForSight;
    //LayerMask collisionMaskForPLayer;
    private void Awake()
    {
        Init();
    }
    public virtual void Update()
    {
        if (isFirst)
        {
            isFirst = false;
            switch (ai)
            {
                case (AI_State.Stand):
                    StartCoroutine(Stand(sustainTime/2));
                    break;
                case (AI_State.Walk):
                    StartCoroutine(Walk(sustainTime));
                    RandomForward();
                    break;
                case (AI_State.Warning):
                    StartCoroutine(Warning(reationTime));
                    break;
                case (AI_State.Battle):
                    break;
                case (AI_State.Return):
                    StartCoroutine(Return());
                    break;
            }
        }
        //  update monster's position
        position = gameObject.transform.position;
        distance = Vector3.Distance(position, spawnPosition);
        //  when in Battel we check the distance
        EnterBattleDetect();
        if (FlyingDetect()) monster.ChangeVelocityY(jumpForce, 7);
        TouchPlayerDetect();
    }

    protected virtual IEnumerator Stand(float sustainTime)
    {
        //Debug.Log("We enter to the Stand state");
        monster.ChangeVelocityX(0, 1);
        yield return new WaitForSeconds(sustainTime / 2);
        sustainTime = RandomState(AI_State.Stand);
        RandomForward();
    }
    protected virtual IEnumerator Walk(float sustainTime)
    {
        Coroutine refeee = StartCoroutine(MoveForward());
        //Debug.Log("We enter to the Walk state");
        yield return new WaitForSeconds(sustainTime);
        StopCoroutine(refeee);
        monster.ChangeVelocityX(0, 1);
        sustainTime = RandomState(AI_State.Walk);
        RandomForward();
    }
    protected virtual IEnumerator Return()
    {   //  in this script just check the x position
        Debug.Log("We enter to the Return state");
        float toward = spawnPosition.x - gameObject.transform.position.x;
        monster.forward = (int)Mathf.Sign(toward);
        Coroutine refeee = StartCoroutine(GoToSpawnPoint(toward, spawnPosition));
        yield return null;
    }
    protected virtual IEnumerator Warning(float warningTime)
    {
        Debug.Log("We enter to the Warning state");
        float warningSeeDis = seeDistance * 1.5f;   //  increase see distance
        monster.ChangeVelocityX(0, 1);
        yield return new WaitForSeconds(reationTime);   // wait for the monster react
        Vector2 origin = gameObject.transform.position;
        Vector2 dir = Vector2.right * monster.forward;
        if (LookAround())
        {
            StopAllCoroutines();
            TurnToState(AI_State.Battle);
        }
        else
        {
            TurnToState(AI_State.Stand);
        }
        yield return null;
    }

    protected void TurnToState(AI_State state)
    {
        isFirst = true;
        ai = state;
    }

    private bool CheckDistance()
    {
        if (distance > reDistance)
        {
            TurnToState(AI_State.Return);
            return true;
        }
        return false;
    }
    private IEnumerator GoToSpawnPoint(float toward, Vector3 targetPosition)
    {   // it just dectect the x direction
        while (Mathf.Sign(toward) == monster.forward)
        {
            monster.ChangeVelocityX(Mathf.Sign(toward) * speed * 3, 9);    // 3 for faster
            yield return null;
            toward = spawnPosition.x - position.x;
            //hit = Physics2D.Raycast(origin, dir, seeDistance, collisionMaskForPLayer);
        }
        Debug.Log("monster back to the spawn point");
        TurnToState(AI_State.Stand);
        yield return null;
    }

    private IEnumerator MoveForward()
    {
        while (true)
        {
            float velocityX = monster.forward * speed;
            monster.ChangeVelocityX(velocityX, 1);
            yield return null;
        }
    }
    private bool LookAround()
    {
        Vector2 origin = gameObject.transform.position;
        Vector2 baseDir = Vector2.right * monster.forward;
        for (int i = -5; i <= 5; i++)
        {
            Vector2 dir = MonPhysic.RotateClockwise(baseDir, 70 / 10 * i);
            hit = Physics2D.Raycast(origin, dir, seeDistance, collisionMaskForSight);
            if (hit)
            {
                if (hit.collider.gameObject.name == "player")
                {
                    StopAllCoroutines();
                    TurnToState(AI_State.Warning);
                    return true;
                }
                Debug.DrawRay(origin, dir * hit.distance, Color.yellow);
            }
            else Debug.DrawRay(origin, dir * seeDistance, Color.yellow);
        }
        return false;
    }
    private void RandomForward()
    {   // according to the distance , increase the probility of change forward
        float f = UnityEngine.Random.Range(0.0f, reDistance);
        if (f < distance)
        {   // turn to the spawnPoint
            monster.ChangeForward((int)Mathf.Sign(spawnPosition.x - position.x));
        }
    }
    private float RandomState(AI_State curState)
    {   //  random for stand, walk, jump
        //  curState will be choosed less, use the moving theshold
        if (proThesold < 0 || proThesold > 100) proThesold = 50;
        if (AI_State.Stand == curState) proThesold -= 7;
        else if (AI_State.Walk == curState) proThesold += 7;
        AI_State theS = 0;
        int decisionNum = UnityEngine.Random.Range(1, 100);
        if (decisionNum < proThesold) theS = AI_State.Stand;
        else theS = AI_State.Walk;
        TurnToState(theS);
        float susT = 1 + UnityEngine.Random.Range(0, 1.5f);
        return susT;    //  return the time the state sustained
    }
    protected int ForwardPlayer()
    {
        Vector3 toward = monster.playerVec;
        float deltaX = toward.x;
        float deltaY = toward.y;
        //monster.forward = (int)Mathf.Sign(deltaX);
        monster.ChangeForward(monster.forward = (int)Mathf.Sign(deltaX));
        return (int)Mathf.Sign(deltaX);
    }
    void TouchPlayerDetect()
    {
        if (MonPhysic.TouchPlayer(controller, monster))
        {   //  if monster touch the player
            if (AI_State.Battle != ai)
            {
                StopAllCoroutines();
                TurnToState(AI_State.Battle);
            }
            Debug.Log("Physic touch");
        }
    }
    void EnterBattleDetect()
    {
        if (AI_State.Battle == ai) CheckDistance();
        else if (AI_State.Stand == ai || AI_State.Walk == ai)
        {   //  monster will double check if player is in when monster is in warning 
            LookAround();
            //Debug.Log(monster.monsterState.isHurt);
            if (monster.monsterState.isHurt)
            {   //  if monster is hurt, go into battle state
                StopAllCoroutines();
                TurnToState(AI_State.Warning);
            }
        }
    }
    bool FlyingDetect()
    {
        if (monster.velocity.y < flyingTheshold
            && ai != AI_State.Battle)
        {
            //Debug.Log("flying");
            return true;
        }
        return false;
    }
    void Init()
    {
        monster = gameObject.GetComponent<Monster>();
        controller = gameObject.GetComponent<Controller2D>();
        sustainTime = 2;
        proThesold = 50;  //  Probability of each state
        position = gameObject.transform.position;
        spawnPosition = position;
        isFirst = true;
        reationTime = 1.0f;
        collisionMask |= (1 << LayerMask.NameToLayer("Platform"));
        collisionMask |= (1 << LayerMask.NameToLayer("Platform_w"));
        collisionMask |= (1 << LayerMask.NameToLayer("Platform_b"));
        collisionMask |= (1 << LayerMask.NameToLayer("Obstacle"));
        collisionMask |= (1 << LayerMask.NameToLayer("Obstacle_w"));
        collisionMask |= (1 << LayerMask.NameToLayer("Obstacle_b"));
        collisionMaskForSight |= (1 << LayerMask.NameToLayer("Player_w"));
        collisionMaskForSight |= (1 << LayerMask.NameToLayer("Player_b"));
        collisionMaskForSight |= (1 << LayerMask.NameToLayer("Platform"));
        collisionMaskForSight |= (1 << LayerMask.NameToLayer("Platform_w"));
        collisionMaskForSight |= (1 << LayerMask.NameToLayer("Platform_b"));
        collisionMaskForSight |= (1 << LayerMask.NameToLayer("Obstacle"));
        collisionMaskForSight |= (1 << LayerMask.NameToLayer("Obstacle_w"));
        collisionMaskForSight |= (1 << LayerMask.NameToLayer("Obstacle_b"));
        monster.ChangeVelocityX(0, 0);
        flyingTheshold = (-1) * jumpForce;
    }
}