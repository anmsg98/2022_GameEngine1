using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;

    public Player              player = null;
    public MonsterGenerator    monsterGenerator = null;
    public SkillSelectionUI    skillSelectionUI = null;
    public GameObject          triggers = null;

    private bool               isGameOver = false;
    private int                stage = 0;
    private int                restMonsterCount = 0;

    public bool IsGameOver
    {
        get
        { 
            return isGameOver;
        }

        set
        { 
            isGameOver = value;
        }
    }

    public int Stage
    {
        get
        {
            return stage;
        }

        set
        {
            if (stage <= 2)
            {
                stage = value;
            }
        }
    }

    public int RestMonsterCount
    {
        get
        {
            return restMonsterCount;
        }

        set
        {
            restMonsterCount = value;
            
            if (restMonsterCount <= 0)
            {
                restMonsterCount = 0;

                if (stage <= 2)
                {
                    // ���� ���������� ���� ���� ���� Ʈ���Ÿ� Ȱ��ȭ��Ų��.
                    triggers.transform.GetChild(0).GetChild(2 * stage).gameObject.SetActive(true);
                    triggers.transform.GetChild(0).GetChild(2 * stage + 1).gameObject.SetActive(true);

                    // ���� ���������� ���� ������ �ݴ� Ʈ���Ÿ� Ȱ��ȭ��Ų��.
                    triggers.transform.GetChild(1).GetChild(stage).gameObject.SetActive(true);
                }
            }
        }
    }

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                return FindObjectOfType<GameManager>();
            }

            return instance;
        }
    }

    private void Awake()
    {
        player = GameObject.FindWithTag("Player").transform.GetComponent<Player>();
    }

    private void Start()
    {
        if (monsterGenerator != null)
        {
            StartCoroutine(monsterGenerator.CreateMonster());
        }
    }

    public void IncreasePlayerExp(float expIncreament)
    {
        StartCoroutine(player.IncreaseExp(expIncreament));
    }

    public void PrepareNextStage()
    {
        Stage += 1;
        monsterGenerator.PrepareNextStage();
    }
}
