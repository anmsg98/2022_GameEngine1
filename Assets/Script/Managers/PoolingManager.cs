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

    public GameObject GetObject(string objectName, Vector3 position, Quaternion quaternion)
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
            possibleObject.GetComponent<Monster>().Health = 100;

            return possibleObject;
        }
        else
        {
            GameObject newObject = Instantiate(prefabDict[objectName], position, quaternion, monsters);

            managedObjects[objectName].Add(newObject);

            return newObject;
        }
    }
}
