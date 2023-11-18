﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ENTITY_NanoBotT1 : EntityBase, IDamager
{
    
    [SerializeField] Animator animator;
    [SerializeField] Rigidbody2D rb2D;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] LayerMask blockMask;
    [SerializeField] GameObject particle;
    GameManager manager;

    public bool followingPlayer = false;
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

    public override EntityBase Spawn(string[] args, Vector2 spawnPos)
    {
        manager = GameManager.gameManagerReference;
        transform.GetChild(0).GetComponent<DamagersCollision>().target = this;
        transform.GetChild(0).GetComponent<DamagersCollision>().entity = GetComponent<EntityCommonScript>();
        transform.SetParent(GameManager.gameManagerReference.entitiesContainer.transform);
        active = true;
        return this;
    }
    public static EntityBase StaticSpawn(string[] args, Vector2 spawnPos)
    {
        return Instantiate(GameManager.gameManagerReference.EntitiesGameObject[(int)Entities.NanoBotT1], spawnPos, Quaternion.identity).GetComponent<ENTITY_NanoBotT1>().Spawn(args, spawnPos);
    }

    public void Hit(int damageDeal, EntityCommonScript procedence, bool ignoreImunity = false, float knockback = 1f, bool penetrate = false)
    {
        Hp = Hp - damageDeal;
        if (CheckGrounded() && !animator.GetBool("damaged")) rb2D.velocity = new Vector2(rb2D.velocity.x, 10f);
        animator.SetBool("damaged", true);
        Invoke("UnDamage", 0.6f);
    }

    public override void Despawn()
    {
        if (animator.GetBool("dead"))
        {
            for (int i = 0; i < 20; i++)
            {
                GameObject g = Instantiate(particle, transform.position, Quaternion.identity);
                g.GetComponent<ParticleController>().Spawn();
            }
            int drops = Random.Range(0, 3);
            ManagingFunctions.DropItem(65, transform.position, drops);
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

    void Update ()
    {
        if (manager == null) manager = GameManager.gameManagerReference;
        if (manager.InGame) AiFrame();
	}

    public override void AiFrame()
    {
        if (followingPlayer && !manager.player.alive) followingPlayer = false;

        if (animator.GetBool("lookingSide"))
        {
            if(animator.GetBool("isMoving") == false)
            {
                if(Random.Range(0,250) == 0)
                {
                    animator.SetBool("isMoving",true);
                    if(Random.Range(0,2) == 0)
                    {
                        spriteRenderer.flipX = true;
                    }
                    else
                    {
                        spriteRenderer.flipX = false;
                    }
                }
            }
            else
            {
                if (CheckGrounded() && !animator.GetBool("dead") && !animator.GetBool("damaged"))
                {
                    if (!Physics2D.Raycast(transform.position, Vector2.right * Mathf.Clamp(rb2D.velocity.x, -1, 1), 0.8f, blockMask))
                    {
                        if (!spriteRenderer.flipX)
                        {
                            rb2D.velocity = new Vector2(2, rb2D.velocity.y);
                        }
                        else
                        {
                            rb2D.velocity = new Vector2(-2, rb2D.velocity.y);
                        }

                        if(followingPlayer &&  manager.player.transform.position.y < transform.position.y - 1)
                        {
                            manager.DropOn(transform.position - Vector3.up * 0.8f, 0.5f);
                        }
                    }
                    else if (rb2D.velocity.y == 0 && CheckGrounded())
                    {
                        rb2D.velocity = new Vector2(rb2D.velocity.x, 10f);
                    }
                }
                else if (!Physics2D.Raycast(transform.position - new Vector3(0, 0.4f, 0), Vector2.right * Mathf.Clamp(rb2D.velocity.x, -1, 1), 0.75f, blockMask) && !animator.GetBool("dead") && !animator.GetBool("damaged"))
                {
                    if (!spriteRenderer.flipX)
                    {
                        rb2D.velocity = new Vector2(2, rb2D.velocity.y);
                    }
                    else
                    {
                        rb2D.velocity = new Vector2(-2, rb2D.velocity.y);
                    }
                }

                if(animator.GetBool("damaged"))
                {
                    rb2D.velocity = new Vector2(Mathf.Clamp(transform.position.x - manager.player.transform.position.x, -1, 1) * 3, rb2D.velocity.y);
                }

                if (Random.Range(0, 85) == 0 && !followingPlayer)
                {
                    animator.SetBool("lookingSide", false);
                }
            }
        }
        else
        {
            animator.SetBool("isMoving", false);
        }

        if(animator.GetBool("isMoving") == false)
        if (Random.Range(0, 100) == 0)
        {
            animator.SetBool("lookingSide", true);
            if (Random.Range(0, 2) == 0)
            {
                spriteRenderer.flipX = true;
            }
            else
            {
                spriteRenderer.flipX = false;
            }
        }

        if (Vector2.Distance(manager.player.transform.position, transform.position) < 15 && manager.player.alive)
        {
            followingPlayer = true;
            animator.SetBool("isMoving", true);
            if (Mathf.Repeat(manager.frameTimer, 10) == 0)
            {
                if (Mathf.Sign(manager.player.transform.position.x - transform.position.x) < 0)
                {
                    spriteRenderer.flipX = true;
                }
                else
                {
                    spriteRenderer.flipX = false;
                }
            }
        }
        else
        {
            followingPlayer = false;
        }

        if(Random.Range(0,1000) == 0)
        {
            if (Vector2.Distance(transform.position, GameManager.gameManagerReference.player.transform.position) > 20) Despawn();
        }

        if(Vector2.Distance(manager.player.transform.position,transform.position) < 1 && !animator.GetBool("dead") && followingPlayer)
        {
            manager.player.LoseHp(3);
        }
    }

    public bool CheckGrounded(float raycastDist = 0.85f)
    {
        bool Grounded = false;
        float raycastDistance = raycastDist;
        Vector2 pos = new Vector2(transform.position.x, transform.position.y);

        if (Physics2D.Raycast(pos, Vector2.down, raycastDistance, blockMask)) Grounded = true;
        if (Physics2D.Raycast(pos, Vector2.down, raycastDistance, blockMask))
        {
            Debug.DrawRay(pos, Vector3.down * raycastDistance, Color.green);
        }
        else
        {
            Debug.DrawRay(pos, Vector3.down * raycastDistance, Color.red);
        }

        return Grounded;
    }
}
