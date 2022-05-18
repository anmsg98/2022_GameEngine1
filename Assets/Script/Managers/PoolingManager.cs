using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolingManager : MonoBehaviour
{
    private static PoolingManager instance = null;

    public ObjectDB objectDB = null;

    // ������ �ڽĵ��� ������� ��ü�̴�.
    public Transform monsters = null;

    private Dictionary<string, GameObject> prefabDict;
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
        foreach (var objectPrefab in objectDB.objectPrefabs)
        {
            prefabDict.Add(objectPrefab.prefabName, objectPrefab.prefab);
        }
    }

    public GameObject Get(string objectName)
    {
        if (!prefabDict.ContainsKey(objectName))
        {
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

            return possibleObject;
        }
        else
        {
            GameObject newObject = Instantiate(prefabDict[objectName]);

            newObject.transform.parent = monsters;
            managedObjects[objectName].Add(newObject);

            return newObject;
        }
    }

    public GameObject Get(string objectName, Vector3 position, Quaternion quaternion)
    {
        GameObject managedObject = Get(objectName);

        managedObject.transform.position = position;
        managedObject.transform.rotation = quaternion;
        
        Monster monsterScript = managedObject.GetComponent<Monster>();

        monsterScript.Health = 100;

        return managedObject;
    }
}
