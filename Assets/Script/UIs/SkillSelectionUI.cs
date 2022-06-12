using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillSelectionUI : MonoBehaviour
{
    public Text[] skillUINames;
    public Image[] skillIcons;
    public Sprite blankedSkillIcon;
    public Text[] skillUIInfo;
    public GameObject levelUpParticle;

    private Animator animator;
    private SKILL_TYPE[] selectedSkillTypes = new SKILL_TYPE[3];

    private void Awake()
    {
        animator = transform.GetComponent<Animator>();
    }

    public void ActivateUI()
    {
        transform.gameObject.SetActive(true);
        animator.Play("Show", -1, 0.0f);

        SkillDB skillDB = SkillManager.Instance.skillDB;

        // ��ų ������ ��� ���� ��쿡��, ������ ��ų�� ���õǾ� �Ѵ�.
        if (SkillManager.Instance.IsSkillSlotFull())
        {
            int selectedIndex = 0;

            for (int i = 1; i < 4; ++i)
            {
                SKILL_TYPE skillTypeInSlot = SkillManager.Instance.GetSkillTypeInSlot(i);

                if (skillTypeInSlot == SKILL_TYPE.None)
                {
                    break;
                }

                int skillLevel = SkillManager.Instance.GetSkillLevel(skillTypeInSlot);

                if (0 < skillLevel && skillLevel < 5)
                {
                    SkillData skill = skillDB.skillBundles[(int)skillTypeInSlot];

                    selectedSkillTypes[selectedIndex] = skillTypeInSlot;
                    skillUINames[selectedIndex].text = skill.skillName + " (LV." + skillLevel + ")";
                    skillIcons[selectedIndex].sprite = skill.skillIcon;
                    skillUIInfo[selectedIndex].text = skill.skillInfo;

                    selectedIndex += 1;
                }
            }

            if (selectedIndex < 3)
            {
                for (int i = selectedIndex; i < 3; ++i)
                {
                    skillUINames[selectedIndex].text = "";
                    skillIcons[selectedIndex].sprite = blankedSkillIcon;
                    skillUIInfo[selectedIndex].text = "";
                }
            }
        }
        else
        {
            // ���õ� ���ɼ��� �ִ� ��ų�� �����ϴ� ����Ʈ
            List<SKILL_TYPE> candidateSkillTypeList = new List<SKILL_TYPE>();
            int allSkillCount = SkillManager.Instance.skillDB.skillBundles.Length;

            for (int i = 0; i < allSkillCount; ++i)
            {
                // ��ų������ 5�̸��̶�� �ĺ� ����Ʈ�� �����Ѵ�.
                if (SkillManager.Instance.GetSkillLevel((SKILL_TYPE)i) < 5)
                {
                    candidateSkillTypeList.Add((SKILL_TYPE)i);
                }
            }

            // 3���� ����â �� �ĺ� ����Ʈ�� ũ�⸸ŭ�� ��ų ������ �ҷ��´�.
            int candidateSkillCount = candidateSkillTypeList.Count;
            List<int> selectedIndexList = new List<int>();

            for (int i = 0; i < 3;)
            {
                int randomIndex = Random.Range(1, candidateSkillCount);

                if (i < candidateSkillCount && !selectedIndexList.Contains(randomIndex))
                {
                    SkillData skill = skillDB.skillBundles[(int)candidateSkillTypeList[randomIndex]];
                    int skillLevel = SkillManager.Instance.GetSkillLevel(candidateSkillTypeList[randomIndex]);

                    selectedSkillTypes[i] = candidateSkillTypeList[randomIndex];
                    skillUINames[i].text = skill.skillName + " (LV." + skillLevel + ")";
                    skillIcons[i].sprite = skill.skillIcon;

                    float currentDamage = skillLevel * skill.skillDamage;
                    float upgradedDamage = (skillLevel + 1) * skill.skillDamage;
                    float damageInreament = upgradedDamage - currentDamage;
                    
                    skillUIInfo[i].text = skill.skillInfo.Replace("O", upgradedDamage + "(+" + damageInreament + ")");

                    selectedIndexList.Add(randomIndex);
                    i += 1;
                }
                else if (i >= candidateSkillCount)
                {
                    skillUINames[i].text = "";
                    skillIcons[i].sprite = blankedSkillIcon;
                    skillUIInfo[i].text = "";

                    i += 1;
                }
            }
        }
        
        Time.timeScale = 0.0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnClickSelectButton(int index)
    {
        Time.timeScale = 1.0f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (SkillManager.Instance.HasSkill(selectedSkillTypes[index]))
        {
            // �̹� ��ϵ� ��ų�̶�� ������ ������Ų��.
            SkillManager.Instance.IncreaseSkillLevel(selectedSkillTypes[index]);
        }
        else
        {
            SkillManager.Instance.RegisterSkill(selectedSkillTypes[index]);
        }

        transform.gameObject.SetActive(false);
    }

    public void ShowLevelUpEffect()
    {
        Vector3 genPosition = transform.position;

        genPosition.y += 20.0f;

        Instantiate(levelUpParticle, genPosition, Quaternion.identity);
    }
}
