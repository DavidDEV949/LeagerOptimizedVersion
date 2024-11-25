﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ENTITY_Sheep : EntityBase, IDamager
{
    [SerializeField] Animator animator;
    [SerializeField] Rigidbody2D rb2D;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] SpriteRenderer woolRenderer;
    [SerializeField] LayerMask blockMask;
    [SerializeField] GameObject particle;
    EntityCommonScript entityScript;
    GameManager manager;

    public PlayerController followingPlayer;
    float HpMax = 10f;
    float HP = 10f;


    public override int Hp
    {
        get { return Mathf.FloorToInt(HP); }

        set
        {
            if (value > HpMax)
            {
                HP = HpMax;
            }
            else if (value <= 0f)
            {
                HP = 0;
                Kill(null);
            }
            else
            {
                HP = value;
            }

            if (value > 0)
            {
                GetComponent<EntityCommonScript>().saveToFile = true;
                GetComponent<EntityUnloader>().canDespawn = false;
            }
        }
    }

    public override EntityCommonScript EntityCommonScript => entityScript;

    public override string[] GenerateArgs()
    {
        return null;
    }

    public override EntityBase Spawn(string[] args, Vector2 spawnPos)
    {
        manager = GameManager.gameManagerReference;
        transform.GetChild(0).GetComponent<DamagersCollision>().target = this;
        transform.GetChild(0).GetComponent<DamagersCollision>().entity = GetComponent<EntityCommonScript>();
        transform.SetParent(GameManager.gameManagerReference.entitiesContainer.transform);
        entityScript = GetComponent<EntityCommonScript>();
        Active = true;

        return this;
    }
    public static EntityBase StaticSpawn(string[] args, Vector2 spawnPos)
    {
        return Instantiate(GameManager.gameManagerReference.EntitiesGameObject[(int)Entities.Sheep], spawnPos, Quaternion.identity).GetComponent<ENTITY_Sheep>().Spawn(args, spawnPos);
    }

    public void Hit(int damageDeal, EntityCommonScript procedence, bool ignoreImunity = false, float knockback = 1f, bool penetrate = false)
    {
        Hp = Hp - damageDeal;
        if (CheckGrounded() && !animator.GetBool("damaged")) rb2D.velocity = new Vector2(rb2D.velocity.x, 10f);
        animator.SetBool("damaged", true);
        Invoke("UnDamage", 0.6f);
        followingPlayer = procedence.GetComponent<PlayerController>();
        HealthBarManager.self.UpdateHealthBar(transform, HP, HpMax, Vector2.up);
    }

    public override void Despawn()
    {
        if (animator.GetBool("dead") && !entityScript.entityStates.Contains(EntityState.Burning))
        {
            for (int i = 0; i < 20; i++)
            {
                GameObject g = Instantiate(particle, transform.position, Quaternion.identity);
                g.GetComponent<ParticleController>().Spawn();
            }
            int drops = Random.Range(1, 3);
            ManagingFunctions.DropItem(129, transform.position, amount: drops);
        }
        Destroy(gameObject);
    }

    public void UnDamage()
    {
        animator.SetBool("damaged", false);
    }

    public override void Kill(string[] args)
    {
        animator.SetBool("dead", true);
        Invoke("Despawn", 1f);
    }

    void Update()
    {
        if (manager == null) manager = GameManager.gameManagerReference;
        if (manager.InGame) AiFrame();
    }

    public override void AiFrame()
    {
        bool moving = animator.GetBool("isMoving");
        bool dead = animator.GetBool("dead");
        bool damaged = animator.GetBool("damaged");

        if (followingPlayer != null)
        {
            if (!followingPlayer.alive) followingPlayer = null;
        }

        if (!moving)
        {
            if (Random.Range(0, 250) == 0)
            {
                animator.SetBool("isMoving", true);
                moving = true;
            }
        }
        else
        {
            if (!dead && !damaged)
            {
                if (CheckGrounded())
                {
                    if (followingPlayer == null)
                        rb2D.velocity = new Vector2(ManagingFunctions.ParseBoolToInt(!spriteRenderer.flipX) * 2, rb2D.velocity.y);
                    else
                        rb2D.velocity = new Vector2(ManagingFunctions.ParseBoolToInt(!spriteRenderer.flipX) * 5, rb2D.velocity.y);

                    int boolint = ManagingFunctions.ParseBoolToInt(!spriteRenderer.flipX);

                    if (SendRaycast(0.85f * Mathf.Abs(rb2D.velocity.x / 2), Vector2.right * boolint, Vector2.up, true))
                    {
                        rb2D.velocity = new Vector2(rb2D.velocity.x, 13f);
                    }
                    else if (SendRaycast(0.85f * Mathf.Abs(rb2D.velocity.x / 2), Vector2.right * boolint, Vector2.zero))
                    {
                        rb2D.velocity = new Vector2(rb2D.velocity.x, 10f);
                    }
                    else if (!SendRaycast(0.85f * Mathf.Abs(rb2D.velocity.x / 2), Vector2.down, Vector2.right * boolint * 0.7f) && (!SendRaycast(1.85f, Vector2.down, Vector2.right * boolint * 0.7f * Mathf.Abs(rb2D.velocity.x / 2)) || SendRaycast(0.85f, Vector2.down, Vector2.right * boolint * 1.4f * Mathf.Abs(rb2D.velocity.x / 2))))
                    {
                        rb2D.velocity = new Vector2(rb2D.velocity.x, 10.5f);
                    }

                }
                else
                {
                    if (!SendRaycast(0.5f, Vector2.right * ManagingFunctions.ParseBoolToInt(!spriteRenderer.flipX), Vector2.down * 0.52f, true) && !SendRaycast(0.5f, Vector2.right * ManagingFunctions.ParseBoolToInt(!spriteRenderer.flipX), Vector2.up * 0.52f, true) && !SendRaycast(0.5f, Vector2.right * ManagingFunctions.ParseBoolToInt(!spriteRenderer.flipX), Vector2.zero, true))
                    {
                        if (followingPlayer == null)
                            rb2D.velocity = new Vector2(ManagingFunctions.ParseBoolToInt(!spriteRenderer.flipX) * 2, rb2D.velocity.y);
                        else
                            rb2D.velocity = new Vector2(ManagingFunctions.ParseBoolToInt(!spriteRenderer.flipX) * 5, rb2D.velocity.y);
                    }
                    else
                    {
                        rb2D.velocity = new Vector2(0, rb2D.velocity.y);
                    }
                }



            }

            if (damaged)
            {
                rb2D.velocity = new Vector2(Mathf.Clamp(transform.position.x - manager.player.transform.position.x, -1, 1) * 3, rb2D.velocity.y);
            }

            if (Random.Range(0, 85) == 0 && !followingPlayer)
            {
                animator.SetBool("isMoving", false);
                moving = false;
            }
        }

        if (!moving)
            if (Random.Range(0, 100) == 0)
            {
                animator.SetBool("isMoving", true);
                moving = true;

                if (Random.Range(0, 2) == 0)
                {
                    spriteRenderer.flipX = true;
                    woolRenderer.flipX = true;
                }
                else
                {
                    spriteRenderer.flipX = false;
                    woolRenderer.flipX = false;
                }
            }

        if (entityScript.entityStates.Contains(EntityState.Swimming))
        {
            rb2D.velocity = new Vector2(rb2D.velocity.x * 0.6f, 2);
        }

        if (entityScript.entityStates.Contains(EntityState.Burning))
        {
            Kill(null);
        }
        if (followingPlayer != null)
            if (Vector2.Distance(followingPlayer.transform.position, transform.position) < 15 && followingPlayer.alive)
            {
                animator.SetBool("isMoving", true);
                moving = true;

                if (Mathf.Repeat(manager.frameTimer, 10) == 0)
                {
                    if (Mathf.Sign(followingPlayer.transform.position.x - transform.position.x) > 0)
                    {
                        spriteRenderer.flipX = true;
                        woolRenderer.flipX = true;
                    }
                    else
                    {
                        spriteRenderer.flipX = false;
                        woolRenderer.flipX = false;
                    }
                }
            }
            else
            {
                followingPlayer = null;
            }
        else
        {
            followingPlayer = null;
        }

        if (Random.Range(0, 1000) == 0)
        {
            if (Vector2.Distance(transform.position, Camera.main.transform.position) > 20) Despawn();
        }
    }


    public bool CheckGrounded()
    {
        return SendRaycast(0.55f, Vector2.down, Vector2.zero);
    }

    public bool SendRaycast(float raycastDist, Vector2 raycastDir, Vector2 localOffset, bool ignoreSlabs = false)
    {
        bool colliding = false;
        Vector2 startpos = (Vector2)transform.position + localOffset;

        if (ignoreSlabs)
        {
            RaycastHit2D rayHit = Physics2D.Raycast(startpos, raycastDir, raycastDist, blockMask);
            if (rayHit)
                colliding = rayHit.collider.transform.GetComponent<PlatformEffector2D>() == null;
            else colliding = false;
        }
        else
            colliding = Physics2D.Raycast(startpos, raycastDir, raycastDist, blockMask);

        if (colliding)
        {
            Debug.DrawRay(startpos, raycastDir * raycastDist, Color.green);
        }
        else
        {
            Debug.DrawRay(startpos, raycastDir * raycastDist, Color.red);
        }

        return colliding;
    }
}
