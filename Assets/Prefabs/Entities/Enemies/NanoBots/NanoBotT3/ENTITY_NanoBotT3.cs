﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ENTITY_NanoBotT3 : EntityBase, IDamager
{
    [SerializeField] AudioClip[] explosionSounds;
    [SerializeField] Animator animator;
    [SerializeField] Rigidbody2D rb2D;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] LayerMask blockMask;
    [SerializeField] GameObject particle;
    [SerializeField] GameObject explosion;
    EntityCommonScript entityScript;
    GameManager manager;

    public PlayerController followingPlayer;
    float HpMax = 15f;
    float HP = 15f;


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
        return Instantiate(GameManager.gameManagerReference.EntitiesGameObject[(int)Entities.NanoBotT3], spawnPos, Quaternion.identity).GetComponent<ENTITY_NanoBotT3>().Spawn(args, spawnPos);
    }

    public void Hit(int damageDeal, EntityCommonScript procedence, bool ignoreImunity = false, float knockback = 1f, bool penetrate = false)
    {
        Hp = Hp - damageDeal;

        if(procedence.EntityType == "nanobot3" && procedence != entityScript && animator.enabled)
        {
            Boom();
        }
        else if (HP > 0)
        {
            if (CheckGrounded() && !animator.GetBool("damaged")) rb2D.velocity = new Vector2(rb2D.velocity.x, 10f);
            animator.SetBool("damaged", true);
            Invoke("UnDamage", 0.6f);
            HealthBarManager.self.UpdateHealthBar(transform, HP, HpMax, Vector2.up);
        }
    }

    public void Boom()
    {
        animator.enabled = false;
        Instantiate(explosion, transform.position, Quaternion.identity).transform.localScale = new Vector3(6f, 6f, 1f);
        Invoke("Despawn", 0.3f);
        manager.soundController.PlaySfxSound(explosionSounds[Random.Range(0, explosionSounds.Length)], ManagingFunctions.VolumeDistance(Vector2.Distance(manager.player.transform.position, transform.position), 40));

        foreach (EntityCommonScript entity in manager.entitiesContainer.GetComponentsInChildren<EntityCommonScript>())
        {
            if (Vector2.Distance(entity.transform.position, transform.position) < 4f)
            {
                if (entity.gameObject.GetComponent<IDamager>() != null)
                {
                    entity.gameObject.GetComponent<IDamager>().Hit(15, GetComponent<EntityCommonScript>(), true, 1.5f, true);
                }
            }
        }

        foreach (EntityCommonScript entity in manager.dummyObjects.GetComponentsInChildren<EntityCommonScript>())
        {
            if (Vector2.Distance(entity.transform.position, transform.position) < 4f)
            {
                if (entity.gameObject.GetComponent<PlayerController>() != null)
                {
                    entity.gameObject.GetComponent<PlayerController>().LoseHp(15, GetComponent<EntityCommonScript>(), true, 1.5f, true);
                }
            }
        }

        manager.TileExplosionAt((int)transform.position.x, (int)transform.position.y, 3, 4);
    }

    public override void Despawn()
    {
        if (animator.GetBool("dead") && !animator.GetBool("makeboom") && !entityScript.entityStates.Contains(EntityState.Burning))
        {
            for (int i = 0; i < 20; i++)
            {
                GameObject g = Instantiate(particle, transform.position, Quaternion.identity);
                g.GetComponent<ParticleController>().Spawn();
            }
            int drops = Random.Range(0, 3);
            ManagingFunctions.DropItem(65, transform.position, amount: drops);
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
        if (manager.InGame && HP > 0 && !animator.GetBool("makeboom")) AiFrame();

        if (manager.InGame && HP > 0 && animator.GetBool("makeboom"))
            if (followingPlayer)
                if (Vector2.Distance(followingPlayer.transform.position, transform.position) > 5.5f)
                {
                    animator.SetBool("makeboom", false);
                }
    }

    public override void AiFrame()
    {
        bool lookingToSide = animator.GetBool("lookingSide");
        bool moving = animator.GetBool("isMoving");
        bool dead = animator.GetBool("dead");
        bool damaged = animator.GetBool("damaged");

        if (followingPlayer != null)
        {
            if (!followingPlayer.alive) followingPlayer = null;
        }


        if (lookingToSide)
        {
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
                        if (followingPlayer)
                        {
                            rb2D.velocity = new Vector2(ManagingFunctions.ParseBoolToInt(!spriteRenderer.flipX) * 3, rb2D.velocity.y);
                        }
                        else
                        {
                            rb2D.velocity = new Vector2(ManagingFunctions.ParseBoolToInt(!spriteRenderer.flipX) * 2, rb2D.velocity.y);
                        }

                        int boolint = ManagingFunctions.ParseBoolToInt(!spriteRenderer.flipX);

                        if (SendRaycast(0.85f, Vector2.right * boolint, Vector2.up, true))
                        {
                            rb2D.velocity = new Vector2(rb2D.velocity.x, 14f);
                        }
                        else if (SendRaycast(0.85f, Vector2.right * boolint, Vector2.zero))
                        {
                            rb2D.velocity = new Vector2(rb2D.velocity.x, 11f);
                        }
                        else if(!SendRaycast(0.85f, Vector2.down, Vector2.right * boolint * 0.7f) && (!SendRaycast(1.85f, Vector2.down, Vector2.right * boolint * 0.7f) || SendRaycast(0.85f, Vector2.down, Vector2.right * boolint * 1.4f)))
                        {
                            rb2D.velocity = new Vector2(rb2D.velocity.x, 10.5f);
                        }

                    }
                    else
                    {
                        if (!SendRaycast(0.5f, Vector2.right * ManagingFunctions.ParseBoolToInt(!spriteRenderer.flipX), Vector2.down * 0.79f, true) && !SendRaycast(0.5f, Vector2.right * ManagingFunctions.ParseBoolToInt(!spriteRenderer.flipX), Vector2.up * 0.79f, true) && !SendRaycast(0.5f, Vector2.right * ManagingFunctions.ParseBoolToInt(!spriteRenderer.flipX), Vector2.zero, true))
                        {
                            if (followingPlayer)
                            {
                                rb2D.velocity = new Vector2(ManagingFunctions.ParseBoolToInt(!spriteRenderer.flipX) * 3, rb2D.velocity.y);
                            }
                            else
                            {
                                rb2D.velocity = new Vector2(ManagingFunctions.ParseBoolToInt(!spriteRenderer.flipX) * 2, rb2D.velocity.y);
                            }
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
                    animator.SetBool("lookingSide", false);
                    lookingToSide = false;
                }
            }
        }
        else
        {
            animator.SetBool("isMoving", false);
            moving = false;
        }

        if (!moving)
            if (Random.Range(0, 100) == 0)
            {
                animator.SetBool("lookingSide", true);
                lookingToSide = true;

                if (Random.Range(0, 2) == 0)
                {
                    spriteRenderer.flipX = true;
                }
                else
                {
                    spriteRenderer.flipX = false;
                }
            }

        if (entityScript.entityStates.Contains(EntityState.Swimming))
        {
            rb2D.velocity = new Vector2(rb2D.velocity.x * 0.6f, 2);
        }

        if (entityScript.entityStates.Contains(EntityState.OnFire))
        {
            animator.SetBool("makeboom", true);
        }

        if (entityScript.entityStates.Contains(EntityState.Burning))
        {
            Kill(null);
        }

        GameObject nearestPlayer = null;
        float nearestDist = 999999f;

        for (int i = 0; i < manager.dummyObjects.childCount; i++)
        {
            GameObject player = manager.dummyObjects.GetChild(i).gameObject;
            if (Vector2.Distance(player.transform.position, transform.position) < nearestDist)
            {
                nearestDist = Vector2.Distance(player.transform.position, transform.position);
                nearestPlayer = player;
            }
        }

        if (nearestPlayer != null)
            if (Vector2.Distance(nearestPlayer.transform.position, transform.position) < 15 && nearestPlayer.GetComponent<PlayerController>().alive)
            {
                followingPlayer = nearestPlayer.GetComponent<PlayerController>();
                animator.SetBool("isMoving", true);
                animator.SetBool("lookingSide", true);
                moving = true;
                lookingToSide = true;

                if (Mathf.Repeat(manager.frameTimer, 10) == 0)
                {
                    if (Mathf.Sign(followingPlayer.transform.position.x - transform.position.x) < 0)
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

        if (followingPlayer)
        {
            if (followingPlayer.transform.position.y < transform.position.y - 1)
            {
                manager.DropOn(transform.position - Vector3.up * 0.8f, 0.5f);
            }

            if (Vector2.Distance(followingPlayer.transform.position, transform.position) < 1 && !dead && !animator.GetBool("makeboom") && followingPlayer)
            {
                animator.SetBool("makeboom", true);
            }
        }
    }

    public bool CheckGrounded()
    {
        return SendRaycast(0.85f, Vector2.down, Vector2.zero);
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
