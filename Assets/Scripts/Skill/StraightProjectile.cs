using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StraightProjectile : Projectile
{
    protected override void Move()
    {
        // 부모의 Move()는 rb.velocity = direction * speed; 를 수행합니다.
        // 직선 투사체는 이 로직만으로 충분합니다.
        base.Move();
    }
}
