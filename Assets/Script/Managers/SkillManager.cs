using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillManager : MonoBehaviour
{
    private static SkillManager instance;

    public SkillDB skillDB;
    public SkillSelectionUI skillSelectionUI;

    public GameObject skillInfoPanel;
    public Text[] skillInfoPanelTexts;

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
        // 스킬 레벨을 관리하기 위한 딕셔너리를 생성한다.
        skillLevelDict = new Dictionary<SKILL_TYPE, int>();

        int allSkillCount = skillDB.skillBundles.Length - 3;

        for (int i = 0; i < allSkillCount; ++i)
        {
            skillLevelDict.Add((SKILL_TYPE)i, 0);
        }
    }

    public bool HasSkill(SKILL_TYPE skillType)
    {
        if (skillType < 0 || (int)skillType >= skillDB.skillBundles.Length - 3)
        {
            Debug.LogError("인덱스를 벗어났습니다.");

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

    public bool IsSkillSlotFull()
    {
        return skillCount == skillSlots.Length;
    }

    public int GetSkillLevel(SKILL_TYPE skillType)
    {
        if (skillType < 0 || (int)skillType >= skillDB.skillBundles.Length - 3)
        {
            Debug.LogError("인덱스를 벗어났습니다.");

            return -1;
        }
        else if (!skillLevelDict.ContainsKey(skillType))
        {
            Debug.LogError("유효하지 않은 스킬입니다.");

            return -1;
        }

        return skillLevelDict[skillType];
    }

    public SKILL_TYPE GetSkillTypeInSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= skillSlots.Length)
        {
            Debug.LogError("인덱스를 벗어났습니다.");

            return SKILL_TYPE.None;
        }
        else if (skillSlots[slotIndex].IsEmpty)
        {
            Debug.LogError("해당 스킬 슬롯은 비어있습니다.");

            return SKILL_TYPE.None;
        }

        return skillSlots[slotIndex].SkillType;
    }

    public int GetSkillSlotIndex(SKILL_TYPE skillType)
    {
        // 해당 스킬을 보유하고 있다면, 그 스킬을 보유한 슬롯의 인덱스를 반환한다.
        if (skillType < 0 || (int)skillType >= skillDB.skillBundles.Length - 3)
        {
            Debug.LogError("인덱스를 벗어났습니다.");

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
        else if (skillType < 0 || (int)skillType >= skillDB.skillBundles.Length - 3)
        {
            Debug.LogError("인덱스를 벗어났습니다.");

            return;
        }
        else if (HasSkill(skillType))
        {
            Debug.LogError("이미 보유중인 스킬입니다.");

            return;
        }

        skillSlots[skillCount].RegisterSkill(skillType);
        IncreaseSkillLevel(skillType);

        skillCount += 1;
    }

    public void IncreaseSkillLevel(SKILL_TYPE skillType)
    {
        if (skillType < 0 || (int)skillType >= skillDB.skillBundles.Length - 3)
        {
            Debug.LogError("인덱스를 벗어났습니다.");

            return;
        }

        int slotIndex = GetSkillSlotIndex(skillType);

        // slotIndex가 -1인 경우, 슬롯에 없는 스킬이다.
        if (slotIndex >= 0)
        {
            int skillLevel = skillLevelDict[skillType];

            if (skillLevel < 5)
            {
                skillSlots[slotIndex].IncreaseSkillLevel(skillLevel);

                // 복사된 skillLevel이 아닌 원본 값을 증가시켜주어야 한다.
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

                SoundManager.Instance.PlaySFX("ThrowFire");
                break;
            case SKILL_TYPE.DeadExplode:
                genPosition = playerTransform.position;
                genPosition += 15.0f * playerTransform.forward;

                PoolingManager.Instance.GetSkillEffect("DeadExplode", genPosition, playerTransform.rotation);
                SoundManager.Instance.PlaySFX("DeadExplode");
                break;
            case SKILL_TYPE.Genesis:
                StartCoroutine(GenerateBeam());
                break;
            case SKILL_TYPE.EnergyDischarge:
                StartCoroutine(Discharge());
                break;
            case SKILL_TYPE.LightningArrow:
                StartCoroutine(ExplodeArrow());
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
        SoundManager.Instance.PlaySFX("Genesis_Before");

        yield return new WaitForSeconds(2.0f);

        WaitForSeconds spawnTime = new WaitForSeconds(0.5f);
        Transform playerTransform = GameManager.Instance.player.transform;

        for (int i = 0; i < 5; ++i)
        {
            Vector3 skillPos = playerTransform.position;

            skillPos += Random.Range(5.0f, 10.0f) * playerTransform.forward;
            skillPos += Random.Range(-10.0f, 10.0f) * playerTransform.right;

            PoolingManager.Instance.GetSkillEffect("Genesis", skillPos, Quaternion.identity);
            SoundManager.Instance.PlaySFX("Genesis_After");

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

        SoundManager.Instance.PlaySFX("EnergyDischarge");

        yield return new WaitForSeconds(2.0f);

        skillRange.enabled = true;
    }

    private IEnumerator ExplodeArrow()
    {
        Transform playerTransform = GameManager.Instance.player.transform;
        Vector3 skillPos = playerTransform.position;

        GameObject skill = PoolingManager.Instance.GetSkillEffect("LightningArrow", skillPos, playerTransform.rotation);
        SphereCollider skillRange = skill.GetComponent<SphereCollider>();

        skillRange.enabled = false;
        SoundManager.Instance.PlaySFX("LightningArrow");

        yield return new WaitForSeconds(3.5f);

        skillRange.enabled = true;
    }

    private IEnumerator GenerateField()
    {
        Transform playerTransform = GameManager.Instance.player.transform;
        Vector3 genPosition = playerTransform.position;
        GameObject skill = PoolingManager.Instance.GetSkillEffect("GravityField", genPosition, playerTransform.rotation);
        BoxCollider skillRange = skill.GetComponent<BoxCollider>();

        skillRange.enabled = false;
        SoundManager.Instance.PlaySFX("GravityField");

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

        SoundManager.Instance.PlaySFX("FlameThrower");

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
