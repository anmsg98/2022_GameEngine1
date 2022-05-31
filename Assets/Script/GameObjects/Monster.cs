using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Monster : Entity
{
    private Animator animator = null;
    private List<Material> materials = new List<Material>();

    [HideInInspector]
    public StateMachine<Monster> stateMachine;

    [HideInInspector]
    public NavMeshAgent navMeshAgent;

    [HideInInspector]
    public Rigidbody rigidBody;

    private void Awake()
    {
        stateMachine = new StateMachine<Monster>(this, Monster_ChaseState.Instance);

        animator = transform.GetComponent<Animator>();
        navMeshAgent = transform.GetComponent<NavMeshAgent>();
        rigidBody = transform.GetComponent<Rigidbody>();

        // �ش� ��ü�� ������ �ִ� ��� ���͸����� ĳ�� �س��´�.
        // �̶�, �̸��� �ߺ��ȴٸ� �ϳ��� �����ϵ��� �Ѵ�.
        SkinnedMeshRenderer[] skinnedMeshRenderers = transform.GetComponentsInChildren<SkinnedMeshRenderer>();
        List<string> isIncluded = new List<string>();

        foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
        {
            foreach (Material material in skinnedMeshRenderer.materials)
            {
                if (!isIncluded.Contains(material.name))
                {
                    materials.Add(material);
                    isIncluded.Add(material.name);
                }
            }
        }
    }

    private void Update()
    {
        if (IsAlive && !IsHit)
        {
            stateMachine.LogicUpdate();
        }
    }

    private void FixedUpdate()
    {
        if (IsAlive && !IsHit)
        {
            stateMachine.PhysicsUpdate();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (IsAlive)
        {
            // collision.gameObject.tag�� �ϸ� �浹�� �Ͼ �������� �ֻ���� �������� ������ collider.tag�� �ؾ���!
            if (collision.collider.CompareTag("Floor") || collision.collider.CompareTag("Monster"))
            {
                animator.SetTrigger("Land");
                navMeshAgent.enabled = true;
            }
            else if (collision.collider.CompareTag("Weapon"))
            {
                if (!IsHit)
                {
                    StartCoroutine(Hit());
                }
            }
        }
    }

    private IEnumerator Hit()
    {
        IsHit = true;
        Health -= 50;

        if (IsAlive)
        {
            foreach (Material material in materials)
            {
                material.color = new Color(1.0f, 0.7f, 0.7f, 1.0f);
            }

            animator.SetTrigger("Hit");

            yield return new WaitForSeconds(0.2f);

            foreach (Material material in materials)
            {
                material.color = Color.white;
            }
        }
        else
        {
            navMeshAgent.enabled = false;
            animator.SetTrigger("Die");

            GameManager.Instance.RestMonsterCount -= 1;
            GameManager.Instance.IncreasePlayerExp(100.0f);
        }

        IsHit = false;
    }

    IEnumerator ReserveToDestroyObject()
    {
        yield return new WaitForSeconds(1.0f);

        transform.gameObject.SetActive(false);
        navMeshAgent.enabled = false;
    }
}
