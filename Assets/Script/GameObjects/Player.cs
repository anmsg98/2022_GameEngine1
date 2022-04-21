using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using Random = System.Random;

public class Player : MonoBehaviour
{
    // 게임 오브젝트
    private GameObject playerSword;
    private GameObject rightHand;

    public GameObject[] skillObj;
    
    // 카메라 셰이킹 이펙트
    private CameraShake cameraShake;
    public float duration;
    public float magnitude;
    
    // 캐릭터 움직임 및 애니메이션
    public float speed;
    private Animator _anim;
    private CharacterController _controller;
    private Camera cam;
    
    // 캐릭터 공격 중
    public bool underAttack;
    
    // 카메라 회전
    public bool toggleCameraRotation;
    public float smoothness = 10f;
    
    // 키 입력
    private bool f1Down;
    private bool f2Down;
    private bool s1Down;
    private bool s2Down;
    private bool s3Down;
    private bool s4Down;

    // 플레이어의 레벨
    private int level = 1;

    // 플레이어의 경험치
    public Slider expBar = null;
    public TMP_Text expText = null;
    private float exp = 0;


    void GetInput()
    {
        f1Down = Input.GetButtonDown("Fire1");
        f2Down = Input.GetButtonDown("Fire2");
        s1Down = Input.GetButtonDown("Skill1");
        s2Down = Input.GetButtonDown("Skill2");
        s3Down = Input.GetButtonDown("Skill3");
        s4Down = Input.GetButtonDown("Skill4");
    }

    private void Awake()
    {
        _anim = this.GetComponent<Animator>();
        _controller = this.GetComponent<CharacterController>();
        cam = Camera.main;
        cameraShake = cam.GetComponent<CameraShake>();
        playerSword = GameObject.Find("mixamorig:RightHand").transform.Find("Sword").gameObject;
        rightHand = GameObject.Find("mixamorig:RightHand");
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        Attack();
        Move();
        
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            toggleCameraRotation = true;
        }
        else
        {
            toggleCameraRotation = false;
        }
    }

    private void LateUpdate()
    {
        if (!toggleCameraRotation && !underAttack)
        {
            Vector3 playerRotate = Vector3.Scale(cam.transform.forward, new Vector3(1, 0, 1));
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(playerRotate),
                Time.deltaTime * smoothness);
        }
    }

    void Move()
    {
        if (!underAttack)
        {
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 right = transform.TransformDirection(Vector3.right);

            Vector3 moveDirection = forward * Input.GetAxisRaw("Vertical") + right * Input.GetAxisRaw("Horizontal");

            _controller.Move(moveDirection.normalized * speed * Time.deltaTime);

            _anim.SetBool("IS_RUN", moveDirection != Vector3.zero);
        }
    }

    void Attack()
    {
        if (!underAttack && f1Down)
        {
            playerSword.SetActive(true);
            underAttack = true;
            _anim.SetTrigger("DO_ATTACK1");
        }

        else if (!underAttack && f2Down)
        {
            playerSword.SetActive(true);
            underAttack = true;
            _anim.SetTrigger("DO_ATTACK2");
        }
        else if (!underAttack && s1Down)
        {
            underAttack = true;
            _anim.SetTrigger("DO_SKILL1");
        }
        
        else if (!underAttack && s2Down)
        {
            underAttack = true;
            _anim.SetTrigger("DO_SKILL2");
        }
        
        else if (!underAttack && s3Down)
        {
            underAttack = true;
            _anim.SetTrigger("DO_SKILL3");
        }
        
        else if (!underAttack && s4Down)
        {
            underAttack = true;
            _anim.SetTrigger("DO_SKILL4");
        }
        
    }

    void ShakeCamera()
    {
        StartCoroutine(cameraShake.Shake(duration, magnitude));
    }

    void DeadExplode()
    {
        Vector3 skillPos = transform.position;
        skillPos += transform.forward * 20;
        GameObject instantDeadExplode = Instantiate(skillObj[0], skillPos, transform.rotation);
    }
    
    void ThrowFire()
    {
        GameObject instantMagicFire = Instantiate(skillObj[1], rightHand.transform.position, transform.rotation);
        Rigidbody rigidMagicFire = instantMagicFire.GetComponent<Rigidbody>();
        rigidMagicFire.AddForce(transform.forward * 20.0f, ForceMode.Impulse);
        //rigidMagicFire.AddTorque(Vector3.back * 10, ForceMode.Impulse);
    }

    void Meteor()
    {
        StartCoroutine("SpawnMeteor");
    }
    
    IEnumerator SpawnMeteor()
    {
        WaitForSeconds spawnTime = new WaitForSeconds(0.4f);
        for (int i = 0; i < 5; i++)
        {
            Vector3 skillPos = transform.position;
            var forward = UnityEngine.Random.Range(10f, 20f);
            var side = UnityEngine.Random.Range(-20f, 20f);
            
            skillPos += transform.forward * forward;
            skillPos += transform.right * side;
            
            GameObject instantMeteor = Instantiate(skillObj[2], skillPos, transform.rotation);
            yield return spawnTime;
        }
    
    }
    
    void EnegyExplode()
    {
        Transform chest = GameObject.FindWithTag("Player").transform.Find("Camera").transform;
        Transform skillPos = GameObject.FindWithTag("Player").transform.Find("EnegyExplodePos").transform;
        GameObject instantEnergyExplode = Instantiate(skillObj[3], skillPos.transform.position, skillPos.transform.rotation);
        StartCoroutine(ActivateAttackCollision(instantEnergyExplode));
    }
    
    void AttackDisable()
    {
        underAttack = false;
        playerSword.SetActive(false);
    }

    IEnumerator ActivateAttackCollision(GameObject instantObj)
    {
        WaitForSeconds wait = new WaitForSeconds(2.0f);
        yield return wait;
        GameObject attackCollide = instantObj.transform.Find("DamageRange").gameObject;
        attackCollide.SetActive(true);
    }
    
    public IEnumerator IncreaseExp(float expIncrement)
    {
        const float maxExp = 500.0f;
        const float duration = 2.0f;
        float offsetPerFrame = (expIncrement / duration) * Time.deltaTime;
        float restExpIncrement = expIncrement;
        float expPer = 0.0f;

        while (restExpIncrement >= 0.0f)
        {
            exp += offsetPerFrame;
            restExpIncrement -= offsetPerFrame;

            if (exp >= maxExp)
            {
                exp = 0.0f;
                level += 1;

                GameManager.Instance.skillSelectionUI.UpdateSkillSelectionUI();
                GameManager.Instance.skillSelectionUI.gameObject.SetActive(true);

                Time.timeScale = 0.0f;
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
            }

            expPer = exp / maxExp;
            expBar.value = expPer;
            expText.text = 100.0f * expPer + "%";

            yield return null;
        }
    }
}
