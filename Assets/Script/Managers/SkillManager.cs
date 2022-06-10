using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillManager : MonoBehaviour
{
    private static SkillManager instance;

    public SkillDB skillDB;
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

        int allSkillCount = skillDB.skillBundles.Length;

        for (int i = 0; i < allSkillCount; ++i)
        {
            skillLevelDict.Add((SKILL_TYPE)i, 0);
        }
    }

    public bool HasSkill(SKILL_TYPE skillType)
    {
        if (skillType < 0 || (int)skillType >= skillDB.skillBundles.Length)
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
        if (skillType < 0 || (int)skillType >= skillDB.skillBundles.Length)
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
        if (skillType < 0 || (int)skillType >= skillDB.skillBundles.Length)
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
        else if (skillType < 0 || (int)skillType >= skillDB.skillBundles.Length)
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
        if (skillType < 0 || (int)skillType >= skillDB.skillBundles.Length)
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
            skillDB.skillBundles[(int)skillSlots[slotIndex].SkillType].prefab.GetComponent<BaseSkill>().UseSkill();
            StartCoroutine(skillSlots[slotIndex].CalculateCoolTime());
        }
    }

    public void GenerateEffect(int skillIndex)
    {
        Transform playerTransform = GameManager.Instance.player.transform;
        Vector3 genPosition;

        switch ((SKILL_TYPE)skillIndex)
        {
            case SKILL_TYPE.ThrowFire:
                genPosition = GameManager.Instance.player.rightHand.transform.position;

                GameObject fire = PoolingManager.Instance.GetSkillEffect("ThrowFire", genPosition, playerTransform.rotation);
                Rigidbody fireRigidbody = fire.GetComponent<Rigidbody>();

                fireRigidbody.velocity = Vector3.zero;
                fireRigidbody.angularVelocity = Vector3.zero;
                fireRigidbody.AddForce(15.0f * GameManager.Instance.player.transform.forward, ForceMode.Impulse);
                break;
            case SKILL_TYPE.DeadExplode:
                genPosition = playerTransform.position;
                genPosition += 15.0f * playerTransform.forward;

                PoolingManager.Instance.GetSkillEffect("DeadExplode", genPosition, playerTransform.rotation);
                break;
            case SKILL_TYPE.Genesis:
                StartCoroutine(GenerateBeam());
                break;
            case SKILL_TYPE.EnergyDischarge:
                StartCoroutine(Discharge());
                break;
            case SKILL_TYPE.LightningArrow:
                genPosition = GameManager.Instance.player.rightHand.transform.position;

                PoolingManager.Instance.GetSkillEffect("LightningArrow", genPosition, playerTransform.rotation);
                break;
            case SKILL_TYPE.GravityField:
                StartCoroutine(GenerateField());
                break;
            case SKILL_TYPE.FlameThrowing:
                StartCoroutine(GenerateFlame());
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

    private IEnumerator GenerateField()
    {
        Transform playerTransform = GameManager.Instance.player.transform;
        Vector3 genPosition = playerTransform.position;
        GameObject skill = PoolingManager.Instance.GetSkillEffect("GravityField", genPosition, playerTransform.rotation);
        BoxCollider skillRange = skill.GetComponent<BoxCollider>();

        skillRange.enabled = false;

        yield return new WaitForSeconds(0.3f);

        skillRange.enabled = true;
    }

    private IEnumerator GenerateFlame()
    {
        Transform playerTransform = GameManager.Instance.player.transform;
        Vector3 genPosition = GameManager.Instance.player.rightHand.transform.position;

        GameObject skill = PoolingManager.Instance.GetSkillEffect("FlameThrowing", genPosition, playerTransform.rotation);
        BoxCollider skillRange = skill.GetComponent<BoxCollider>();

        const float duration = 1.5f;
        float centerDeltaZPerFrame = (5.0f / duration) * Time.deltaTime;
        float sizeDeltaZPerFrame = ((10.0f - skillRange.size.z) / duration) * Time.deltaTime;

        while (skillRange.center.z < 5.0f || skillRange.size.z < 10.0f)
        {
            skillRange.center += Vector3.forward * centerDeltaZPerFrame;
            skillRange.size += Vector3.forward * sizeDeltaZPerFrame;

            yield return null;
        }

        skillRange.center = Vector3.zero;
        skillRange.size = 0.6f * Vector3.one;
    }
}
