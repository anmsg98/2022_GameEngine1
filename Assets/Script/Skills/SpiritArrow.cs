using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiritArrow : BaseSkill
{
    private void OnEnable()
    {
        StartCoroutine(InActive(2.0f));
    }

    public override void UseSkill()
    {
        GameManager.Instance.player.TransAnimation("SpiritArrow");
    }
}
