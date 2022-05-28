using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillSelectionUI : MonoBehaviour
{
    public Text[] skillUINames = null;
    public Text[] skillUIInfo = null;

    private SKILL_TYPE[] selectedSkillTypes = new SKILL_TYPE[3];

    public void ActivateUI()
    {
        transform.gameObject.SetActive(true);

        List<int> typeList = new List<int>();
        SkillDB skillDB = PoolingManager.Instance.skillDB;

        for (int i = 0; i < 3;)
        {
            int randomType = Random.Range(0, skillDB.skillPrefabs.Length);

            if (!typeList.Contains(randomType))
            {
                int skillLevel = SkillManager.Instance.GetSkillLevel((SKILL_TYPE)randomType);

                if (skillLevel < 5)
                {
                    SkillPrefab skill = skillDB.skillPrefabs[randomType];

                    selectedSkillTypes[i] = (SKILL_TYPE)randomType;
                    skillUINames[i].text = skill.skillName + " (LV." + skillLevel + ")";
                    skillUIInfo[i].text = skill.skillInfo;
                    ++i;

                    typeList.Add(randomType);
                }
            }
        }

        Time.timeScale = 0.0f;
        Cursor.lockState = CursorLockMode.Confined;
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
}
