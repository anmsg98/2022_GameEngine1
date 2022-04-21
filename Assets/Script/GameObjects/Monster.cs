using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : Entity
{
    private Animator animator = null;
    private List<Material> materials = new List<Material>();

    private void Awake()
    {
        animator = transform.GetComponent<Animator>();
        
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

    private void OnCollisionEnter(Collision collision)
    {
        if (IsAlive)
        {
            // collision.gameObject.tag�� �ϸ� �浹�� �Ͼ �������� �ֻ���� �������� ������ collider.tag�� �ؾ���!
            if (collision.collider.CompareTag("Floor") || collision.collider.CompareTag("Monster"))
            {
                animator.SetTrigger("Land");
            }
            else if (collision.collider.CompareTag("Weapon"))
            {
                if (!IsHit)
                {
                    StartCoroutine(HitEffect());

                    Health -= 50;

                    if (IsAlive)
                    {
                        animator.SetTrigger("Hit");
                    }
                    else
                    {
                        animator.SetTrigger("Die");

                        GameManager.Instance.RestMonsterCount -= 1;
                        GameManager.Instance.IncreasePlayerExp(100.0f);
                    }
                }
            }
        }
    }

    IEnumerator HitEffect()
    {
        IsHit = true;

        foreach (Material material in materials)
        {
            material.color = new Color(1.0f, 0.6f, 0.6f, 1.0f);
        }

        yield return new WaitForSeconds(0.15f);

        IsHit = false;

        foreach (Material material in materials)
        {
            material.color = Color.white;
        }
    }

    IEnumerator ReserveToDestroyObject()
    {
        yield return new WaitForSeconds(5.0f);

        Destroy(gameObject);
    }
}
