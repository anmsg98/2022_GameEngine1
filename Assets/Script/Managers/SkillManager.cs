using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum SKILL_TYPE
{
    ThrowFire,
    DeadExplode,
    Genesis,
    EnergyDischarge,
};

public class SkillManager : MonoBehaviour
{
    private static SkillManager instance;

    public SkillSelectionUI skillSelectionUI;

    public Sprite skillLevelImage;

    public SkillSlot[] skillSlots;
    private int skillCount;

    private Dictionary<SKILL_TYPE, int> skillLevelDict;

    public static SkillManager Instance
    {
        get
        {
            if (instance == null)
            {
                return FindObjectOfType<SkillManager>();

                if (instance == null)
                {
                    GameObject gameManager = new GameObject(nameof(SkillManager));

                    instance = gameManager.AddComponent<SkillManager>();
                }
            }

            return instance;
        }
    }

    private void Awake()
    {
        // ��ų ������ �����ϱ� ���� ��ųʸ��� �����Ѵ�.
        skillLevelDict = new Dictionary<SKILL_TYPE, int>();

        int allSkillCount = PoolingManager.Instance.skillDB.skillPrefabs.Length;

        for (int i = 0; i < allSkillCount; ++i)
        {
            skillLevelDict.Add((SKILL_TYPE)i, 0);
        }
    }

    public bool HasSkill(SKILL_TYPE skillType)
    {
        if (skillType < 0 || (int)skillType >= PoolingManager.Instance.skillDB.skillPrefabs.Length)
        {
            Debug.LogError("�ε����� ������ϴ�.");

            return false;
        }

        int slotCount = skillSlots.Length;

        for (int i = 0; i < slotCount; ++i)
        {
            if (!skillSlots[i].IsEmpty && skillSlots[i].SkillType == skillType)
            {
                return true;
            }
        }

        return false;
    }

    public int GetSkillLevel(SKILL_TYPE skillType)
    {
        if (skillType < 0 || (int)skillType >= PoolingManager.Instance.skillDB.skillPrefabs.Length)
        {
            Debug.LogError("�ε����� ������ϴ�.");

            return -1;
        }
        else if (!skillLevelDict.ContainsKey(skillType))
        {
            Debug.LogError("��ȿ���� ���� ��ų�Դϴ�.");

            return -1;
        }

        return skillLevelDict[skillType];
    }

    public int GetSkillSlotIndex(SKILL_TYPE skillType)
    {
        // �ش� ��ų�� �����ϰ� �ִٸ�, �� ��ų�� ������ ������ �ε����� ��ȯ�Ѵ�.
        if (skillType < 0 || (int)skillType >= PoolingManager.Instance.skillDB.skillPrefabs.Length)
        {
            Debug.LogError("�ε����� ������ϴ�.");

            return -1;
        }

        int slotCount = skillSlots.Length;

        for (int i = 0; i < slotCount; ++i)
        {
            if (!skillSlots[i].IsEmpty && skillSlots[i].SkillType == skillType)
            {
                return i;
            }
        }

        return -1;
    }

    public void RegisterSkill(SKILL_TYPE skillType)
    {
        if (skillCount >= skillSlots.Length)
        {
            return;
        }
        else if (skillType < 0 || (int)skillType >= PoolingManager.Instance.skillDB.skillPrefabs.Length)
        {
            Debug.LogError("�ε����� ������ϴ�.");

            return;
        }
        else if (HasSkill(skillType))
        {
            Debug.LogError("�̹� �������� ��ų�Դϴ�.");

            return;
        }

        skillSlots[skillCount].RegisterSkill(skillType);
        IncreaseSkillLevel(skillType);

        skillCount += 1;
    }

    public void IncreaseSkillLevel(SKILL_TYPE skillType)
    {
        if (skillType < 0 || (int)skillType >= PoolingManager.Instance.skillDB.skillPrefabs.Length)
        {
            Debug.LogError("�ε����� ������ϴ�.");

            return;
        }

        int slotIndex = GetSkillSlotIndex(skillType);

        // slotIndex�� -1�� ���, ���Կ� ���� ��ų�̴�.
        if (slotIndex >= 0)
        {
            int skillLevel = skillLevelDict[skillType];

            if (skillLevel < 5)
            {
                skillSlots[slotIndex].IncreaseSkillLevel(skillLevel);

                // ����� skillLevel�� �ƴ� ���� ���� ���������־�� �Ѵ�.
                skillLevelDict[skillSlots[slotIndex].SkillType] += 1;
            }
        }
    }

    public void UseSkill(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= skillCount)
        {
            return;
        }

        if (!skillSlots[slotIndex].IsEmpty && skillSlots[slotIndex].IsUsable)
        { 
            PoolingManager.Instance.skillDB.skillPrefabs[(int)skillSlots[slotIndex].SkillType].prefab.GetComponent<BaseSkill>().UseSkill();
            StartCoroutine(skillSlots[slotIndex].CalculateCoolTime());
        }
    }

    public void GenerateEffect(int skillIndex)
    {
        if (skillIndex < 0 || skillIndex >= skillSlots.Length)
        {
            print("�ε����� ������ϴ�.");

            return;
        }

        switch ((SKILL_TYPE)skillIndex)
        {
            case SKILL_TYPE.ThrowFire:
                Vector3 genPosition = GameManager.Instance.player.rightHand.transform.position;

                GameObject fire = PoolingManager.Instance.GetSkillEffect("ThrowFire", genPosition, Quaternion.identity);
                Rigidbody fireRigidbody = fire.GetComponent<Rigidbody>();

                fireRigidbody.velocity = Vector3.zero;
                fireRigidbody.angularVelocity = Vector3.zero;
                fireRigidbody.AddForce(15.0f * GameManager.Instance.player.transform.forward, ForceMode.Impulse);
                break;
            case SKILL_TYPE.DeadExplode:
                Transform playerTransform = GameManager.Instance.player.transform;
                Vector3 skillPos = playerTransform.position;

                skillPos += 15.0f * playerTransform.forward;

                PoolingManager.Instance.GetSkillEffect("DeadExplode", skillPos, Quaternion.identity);
                break;
            case SKILL_TYPE.Genesis:
                StartCoroutine(GenerateBeam());
                break;
            case SKILL_TYPE.EnergyDischarge:
                StartCoroutine(Discharge());
                break;
        }
    }

    private IEnumerator GenerateBeam()
    {
        WaitForSeconds spawnTime = new WaitForSeconds(0.3f);
        Transform playerTransform = GameManager.Instance.player.transform;

        for (int i = 0; i < 5; ++i)
        {
            Vector3 skillPos = playerTransform.position;

            skillPos += Random.Range(5.0f, 10.0f) * playerTransform.forward;
            skillPos += Random.Range(-10.0f, 10.0f) * playerTransform.right;

            PoolingManager.Instance.GetSkillEffect("Genesis", skillPos, Quaternion.identity);

            yield return spawnTime;
        }
    }

    private IEnumerator Discharge()
    {
        Transform playerTransform = GameManager.Instance.player.transform;
        Vector3 skillPos = playerTransform.position;

        skillPos.y += 1.1f;

        GameObject skill = PoolingManager.Instance.GetSkillEffect("EnergyDischarge", skillPos, Quaternion.identity);
        SphereCollider skillRange = skill.GetComponent<SphereCollider>();

        skillRange.enabled = false;

        yield return new WaitForSeconds(2.0f);

        skillRange.enabled = true;
    }
}
