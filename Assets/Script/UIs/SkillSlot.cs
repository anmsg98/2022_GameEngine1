using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillSlot : MonoBehaviour
{
    public Image skillFilter;
    public Text skillCoolTimeText;
    public Image[] skillLevelImages;

    private bool isEmpty = true;
    private bool isUsable;

    private SKILL_TYPE skillType;
    private float skillCoolTime;
    private float currentCoolTime;

    public bool IsEmpty
    {
        get
        {
            return isEmpty;
        }
    }

    public bool IsUsable
    {
        get
        {
            return isUsable;
        }
    }

    public SKILL_TYPE SkillType
    {
        get
        {
            return skillType;
        }
    }

    public void RegisterSkill(SKILL_TYPE newSkillType, float coolTime)
    {
        if (isEmpty)
        {
            isEmpty = false;
            isUsable = true;

            skillType = newSkillType;
            skillCoolTime = coolTime;
        }
    }

    public IEnumerator CalculateCoolTime()
    {
        isUsable = false;
        skillCoolTimeText.gameObject.SetActive(true);
        currentCoolTime = skillCoolTime;

        // ��ų ��ư�� ������.
        skillFilter.fillAmount = 1.0f;

        while (skillFilter.fillAmount > 0.0f)
        {
            float deltaTime = Time.smoothDeltaTime / skillCoolTime;

            skillFilter.fillAmount -= deltaTime;
            currentCoolTime -= Time.smoothDeltaTime;
            skillCoolTimeText.text = string.Format("{0:0.0}", currentCoolTime);

            yield return null;
        }

        // ��ų ��Ÿ���� ������ ��ų�� ����� �� �ִ� ���·� �����Ѵ�.
        isUsable = true;
        currentCoolTime = 0.0f;
        skillCoolTimeText.gameObject.SetActive(false);
    }
}
