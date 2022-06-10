using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolingManager : MonoBehaviour
{
    private static PoolingManager instance = null;

    public MonsterDB monsterDB = null;

    // ������ �ڽĵ��� ��� ���� �θ�ü�̴�.      
    public Transform monsters = null;

    // �ν��Ͻ��� �������� �����ϱ� ���� ��ųʸ��̴�.
    private Dictionary<string, GameObject> prefabDict;

    // Ǯ������ �����ǰ� �ִ� ��ü���� �����ϱ� ���� ��ųʸ��̴�.
    private Dictionary<string, List<GameObject>> managedObjects;

    public static PoolingManager Instance
    {
        get
        {
            if (instance == null)
            {
                return FindObjectOfType<PoolingManager>();

                if (instance == null)
                {
                    GameObject poolingManager = new GameObject(nameof(PoolingManager));

                    instance = poolingManager.AddComponent<PoolingManager>();
                }
            }

            return instance;
        }
    }

    private void Awake()
    {
        prefabDict = new Dictionary<string, GameObject>();
        managedObjects = new Dictionary<string, List<GameObject>>();

        // �����ͺ��̽��� ��ųʸ��� �籸���Ѵ�.
        foreach (MonsterData monsterData in monsterDB.monsterBundles)
        {
            prefabDict.Add(monsterData.monsterName, monsterData.prefab);
        }

        foreach (SkillData skillData in SkillManager.Instance.skillDB.skillBundles)
        {
            prefabDict.Add(skillData.skillName, skillData.prefab);
        }
    }

    public GameObject GetMonster(string objectName, Vector3 position, Quaternion quaternion)
    {
        if (!prefabDict.ContainsKey(objectName))
        {
            Debug.Log(objectName + " �������� �������� �ʽ��ϴ�.");

            return null;
        }

        if (!managedObjects.ContainsKey(objectName))
        {
            managedObjects.Add(objectName, new List<GameObject>());
        }

        if (managedObjects[objectName].Any(obj => !obj.activeInHierarchy))
        {
            GameObject possibleObject = managedObjects[objectName].FirstOrDefault(obj => !obj.activeInHierarchy);
            
            possibleObject.SetActive(true);
            possibleObject.GetComponent<Monster>().Health = 100;
            possibleObject.transform.position = position;
            possibleObject.transform.rotation = quaternion;

            return possibleObject;
        }

        GameObject newObject = Instantiate(prefabDict[objectName], position, quaternion, monsters);

        managedObjects[objectName].Add(newObject);

        return newObject;
    }

    public GameObject GetSkillEffect(string objectName, Vector3 position, Quaternion quaternion)
    {
        if (!prefabDict.ContainsKey(objectName))
        {
            Debug.Log(objectName + " �������� �������� �ʽ��ϴ�.");

            return null;
        }

        if (!managedObjects.ContainsKey(objectName))
        {
            managedObjects.Add(objectName, new List<GameObject>());
        }

        if (managedObjects[objectName].Any(obj => !obj.activeInHierarchy))
        {
            GameObject possibleObject = managedObjects[objectName].FirstOrDefault(obj => !obj.activeInHierarchy);

            possibleObject.SetActive(true);
            possibleObject.transform.position = position;
            possibleObject.transform.rotation = quaternion;

            // ������ ��ƼŬ �ý����� ����� �ϵ��� �������ش�.
            foreach (ParticleSystem particleSystem in possibleObject.GetComponentsInChildren<ParticleSystem>())
            {
                particleSystem.Simulate(0.0f, true, true);
                particleSystem.Play();
            }

            return possibleObject;
        }

        GameObject newObject = Instantiate(prefabDict[objectName], position, quaternion);

        managedObjects[objectName].Add(newObject);

        return newObject;
    }
}
