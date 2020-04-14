﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillState : MonoBehaviour
{
    public Ski01 ski01;
    public Ski02 ski02;
    public Ski03 ski03;
    public Ski04 ski04;
    Monster monster;
    Controller2D controller;

    [SerializeField]
    LayerMask playerLayer;
    [SerializeField]
    GameObject shootImpact;

    GameObject shootImpactOnScreen;

    public struct Ski01
    {
        public Vector3 angle;
        public float color;   //  0 is black ,1 is white, -1 is gray
        public float damage;
        public void PredictColor()
        {

        }
        public void PredictTheAngle()
        {

        }
        public void SetInitValue()
        {
            color = -1;
            damage = 10;
        }
    };
    public struct Ski02
    {
        public float color;   //  0 is black ,1 is white, -1 is gray
        public float damage;
        public float swordDis;
        public void PredictTheAngle()
        {

        }
        public void PredictColor()
        {

        }
        public void SetInitValue()
        {
            color = -1;
            damage = 10;
            swordDis = 3;
        }
    };
    public struct Ski03
    {
        public float color;   //  0 is black ,1 is white, -1 is gray
        public float damage;
        public float swordDis;
        public Vector3 jumpTarget;
        public Vector3 predictJumpTarget;
        public float maxjumpDis;
        public void PredictTheAngle()
        {

        }
        public void PredictColor()
        {

        }
        public void PredictJumpTarget(Vector3 playerPosition)
        {   //   do predict
            jumpTarget = playerPosition;
        }
        public void SetInitValue()
        {
            color = -1;
            damage = 10;
            swordDis = 3;
            jumpTarget = Player.Instance.transform.position;
            maxjumpDis = 10;
        }
    };
    public struct Ski04
    {
        public float color;   //  0 is black ,1 is white, -1 is gray
        public float damage;
        public void PredictTheAngle()
        {

        }
        public void PredictColor()
        {

        }
        public void SetInitValue()
        {
            color = -1;
            damage = 10;
        }
    };

    public void PreSkillPar01()
    {
        ski01.PredictColor();
        ski01.PredictTheAngle();
    }
    public void SetSkillPar01()
    {
        Vector2 position = gameObject.transform.position;
        position.x += (monster.forward == -1) ? -0.8f : 0.8f;   /* the revision */
        shootImpactOnScreen = Instantiate(shootImpact, position, Quaternion.identity);
        BossSnakeImpact bossSnakeImpact = shootImpactOnScreen.GetComponent<BossSnakeImpact>();
        Debug.Log("shoot");
        if (-1 == ski01.color)
        {   //  gray
            bossSnakeImpact.collisionMask = (1 << LayerMask.NameToLayer("Player_w"));
            bossSnakeImpact.collisionMask |= (1 << LayerMask.NameToLayer("Player_b"));
            bossSnakeImpact.damage = 2.5f*ski01.damage;
            shootImpactOnScreen.GetComponent<Animator>().SetFloat("color", 0);
        }
        else if (0 == ski01.color)
        {   //  black
            bossSnakeImpact.collisionMask = 0;
            bossSnakeImpact.collisionMask = (1 << LayerMask.NameToLayer("Player_w"));
            bossSnakeImpact.damage = ski01.damage;
            shootImpactOnScreen.GetComponent<Animator>().SetFloat("color", 1);
        }
        else if (1 == ski01.color)
        {   // white
            bossSnakeImpact.collisionMask = 0;
            bossSnakeImpact.collisionMask = (1 << LayerMask.NameToLayer("Player_b"));
            bossSnakeImpact.damage = ski01.damage;
            shootImpactOnScreen.GetComponent<Animator>().SetFloat("color", 2);
        }
    }
    public void Shoot(Vector3 playerPosition)
    {
        float degree = Vector3.Angle(new Vector3(1, 0, 0), playerPosition);
        shootImpactOnScreen.transform.Rotate(0, 0, degree, Space.World);
        BossSnakeImpact bossSnakeImpact = shootImpactOnScreen.GetComponent<BossSnakeImpact>();
        bossSnakeImpact.SetInit();
    }
    public void PreSkillPar02()
    {
        ski02.PredictColor();
    }
    public void SetSkillPar02()
    {

    }
    public IEnumerator Slave(float time)
    {
        float curTime = Time.time;
        float endTime = Time.time + time;
        Debug.Log("attack" + ski02.color);
        if (-1 == ski02.color)
        {   //  gray
            playerLayer = (1 << LayerMask.NameToLayer("Player_w"));
            playerLayer |= (1 << LayerMask.NameToLayer("Player_b"));
        }
        else if (0 == ski02.color)
        {   //  black
            playerLayer = 0;
            playerLayer = (1 << LayerMask.NameToLayer("Player_w"));
        }
        else if (1 == ski02.color)
        {   // white
            playerLayer = 0;
            playerLayer = (1 << LayerMask.NameToLayer("Player_b"));
        }
        bool isHit = false;
        StartCoroutine(MonPhysic.ChangeX(monster, 15, 5, time / 3));
        while (Time.time < endTime)
        {
            for (int i = 1; i < controller.horizontalRayCount-1; i++)
            {
                Vector2 rayOrigin = (monster.forward == -1) ? controller.raycastOrigins.bottomLeft : controller.raycastOrigins.bottomRight;
                rayOrigin += Vector2.up * controller.horizontalRaySpacing * i;
                Vector2 direction = Vector2.right * monster.forward;
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, direction, ski02.swordDis, playerLayer);
                Debug.DrawRay(rayOrigin, direction * ski02.swordDis, Color.white);
                if (hit && false == isHit)
                {
                    Debug.Log("slave toutch the player");
                    isHit = true;
                }
            }
            yield return null;
        }
    }
    public float PreSkillPar03(Vector3 position)
    {
        ski03.PredictColor();
        ski03.PredictJumpTarget(position);
        return ski03.jumpTarget.x - gameObject.transform.position.x;
    }
    public float SetSkillPar03()
    {   //  it will return the deltaX betweem boss and player
        float temp = monster.playerPosition.x - gameObject.transform.position.x;
        if (ski03.maxjumpDis <= Mathf.Abs(temp)) return ski03.maxjumpDis;
        else return temp;
    }
    public IEnumerator JumpSlave(float time)
    {
        float curTime = Time.time;
        float endTime = Time.time + time;
        if (-1 == ski02.color)
        {   //  gray
            playerLayer = (1 << LayerMask.NameToLayer("Player_w"));
            playerLayer |= (1 << LayerMask.NameToLayer("Player_b"));
        }
        else if (0 == ski02.color)
        {   //  black
            playerLayer = 0;
            playerLayer = (1 << LayerMask.NameToLayer("Player_w"));
        }
        else if (1 == ski02.color)
        {   // white
            playerLayer = 0;
            playerLayer = (1 << LayerMask.NameToLayer("Player_b"));
        }
        bool isHit = false;
        StartCoroutine(MonPhysic.ChangeX(monster, 10, 5, time / 3));
        while (Time.time < endTime)
        {
            for (int i = 1; i < controller.horizontalRayCount - 1; i++)
            {
                Vector2 rayOrigin = (monster.forward == -1) ? controller.raycastOrigins.bottomLeft : controller.raycastOrigins.bottomRight;
                rayOrigin += Vector2.up * controller.horizontalRaySpacing * i;
                Vector2 direction = Vector2.right * monster.forward;
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, direction, ski02.swordDis, playerLayer);
                Debug.DrawRay(rayOrigin, direction * ski02.swordDis, Color.white);
                if (hit && false == isHit)
                {
                    Debug.Log("slave toutch the player");
                    isHit = true;
                }
            }
            yield return null;
        }
    }
    public void PreSkillPar04()
    {
        ski04.PredictColor();
    }
    public void SetSkillPar04()
    {

    }
    public IEnumerator Flash(float time)
    {
        /* how to disappear */
        bool isHit = false;
        StartCoroutine(MonPhysic.ChangeX(monster, 15, 5, time / 3));
        yield return null;
    }
    void Start()
    {
        controller = gameObject.GetComponent<Controller2D>();
        ski01.SetInitValue();
        ski02.SetInitValue();
        ski03.SetInitValue();
        monster = gameObject.GetComponent<Monster>();
    }
}