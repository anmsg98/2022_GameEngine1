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
            if (collision.collider.tag == "Floor")
            {
                animator.SetTrigger("Land");
            }
            else if (collision.collider.tag == "Weapon")
            {
                if (!IsHit)
                {
                    Health -= 50;

                    StartCoroutine(HitEffect());

                    if (!IsAlive)
                    {
                        animator.SetTrigger("Die");
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
}
