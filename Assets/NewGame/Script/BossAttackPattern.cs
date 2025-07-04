using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Boss Attack Pattern", menuName = "LootShooter/Boss Attack Pattern")]
public class BossAttackPattern : ScriptableObject
{
    [Header("패턴 기본 정보")]
    public string patternName = "기본 패턴";
    public BossPhase targetPhase = BossPhase.Phase1;
    public float patternDuration = 10f; // 패턴 지속 시간
    public float patternCooldown = 5f; // 패턴 쿨다운
    
    [Header("공격 시퀀스")]
    public List<BossAttackSequence> attackSequences = new List<BossAttackSequence>();
    
    [Header("패턴 특수 효과")]
    public bool hasEntranceEffect = false;
    public GameObject entranceEffect;
    public AudioClip entranceSound;
    
    public bool hasExitEffect = false;
    public GameObject exitEffect;
    public AudioClip exitSound;
    
    [Header("패턴 설명")]
    [TextArea(3, 6)]
    public string patternDescription = "이 패턴에 대한 설명입니다.";
    
    [System.Serializable]
    public class BossAttackSequence
    {
        public string sequenceName = "공격 시퀀스";
        public List<BossAttack> attacks = new List<BossAttack>();
        public float sequenceDelay = 0f; // 이전 시퀀스와의 간격
        public bool isRandomOrder = false; // 공격 순서 랜덤화
        public int repeatCount = 1; // 반복 횟수
    }
    
    // 패턴 실행
    public System.Collections.IEnumerator ExecutePattern(BossEnemy boss)
    {
        if (boss == null) yield break;
        
        // 패턴 시작 효과
        if (hasEntranceEffect)
        {
            PlayEntranceEffect(boss.transform.position);
        }
        
        // 각 공격 시퀀스 실행
        foreach (BossAttackSequence sequence in attackSequences)
        {
            yield return new WaitForSeconds(sequence.sequenceDelay);
            yield return ExecuteAttackSequence(boss, sequence);
        }
        
        // 패턴 종료 효과
        if (hasExitEffect)
        {
            PlayExitEffect(boss.transform.position);
        }
    }
    
    System.Collections.IEnumerator ExecuteAttackSequence(BossEnemy boss, BossAttackSequence sequence)
    {
        for (int repeat = 0; repeat < sequence.repeatCount; repeat++)
        {
            List<BossAttack> attacksToExecute = new List<BossAttack>(sequence.attacks);
            
            // 랜덤 순서로 실행
            if (sequence.isRandomOrder)
            {
                for (int i = 0; i < attacksToExecute.Count; i++)
                {
                    int randomIndex = Random.Range(i, attacksToExecute.Count);
                    BossAttack temp = attacksToExecute[i];
                    attacksToExecute[i] = attacksToExecute[randomIndex];
                    attacksToExecute[randomIndex] = temp;
                }
            }
            
            // 각 공격 실행
            foreach (BossAttack attack in attacksToExecute)
            {
                if (boss == null || !boss.IsAlive()) yield break;
                
                // 공격 실행
                yield return ExecuteSingleAttack(boss, attack);
                
                // 공격 간 간격
                yield return new WaitForSeconds(attack.cooldown);
            }
        }
    }
    
    System.Collections.IEnumerator ExecuteSingleAttack(BossEnemy boss, BossAttack attack)
    {
        if (boss == null) yield break;
        
        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null) yield break;
        
        // 공격 범위 체크
        float distanceToPlayer = Vector2.Distance(boss.transform.position, player.position);
        if (distanceToPlayer > attack.range && attack.attackType != BossAttackType.AreaAttack)
        {
            // 범위 밖이면 플레이어에게 이동
            yield return MoveTowardsPlayer(boss, player);
        }
        
        // 공격 실행
        switch (attack.attackType)
        {
            case BossAttackType.MeleeAttack:
                yield return ExecuteMeleeAttack(boss, attack, player);
                break;
            case BossAttackType.RangedAttack:
                yield return ExecuteRangedAttack(boss, attack, player);
                break;
            case BossAttackType.ChargeAttack:
                yield return ExecuteChargeAttack(boss, attack, player);
                break;
            case BossAttackType.SummonMinions:
                yield return ExecuteSummonMinions(boss, attack);
                break;
            case BossAttackType.AreaAttack:
                yield return ExecuteAreaAttack(boss, attack);
                break;
            case BossAttackType.SpecialAbility:
                yield return ExecuteSpecialAbility(boss, attack);
                break;
        }
    }
    
    System.Collections.IEnumerator MoveTowardsPlayer(BossEnemy boss, Transform player)
    {
        float moveTime = 2f;
        float timer = 0f;
        
        while (timer < moveTime)
        {
            if (boss == null || player == null) yield break;
            
            Vector2 direction = (player.position - boss.transform.position).normalized;
            boss.GetComponent<Rigidbody2D>().linearVelocity = direction * boss.moveSpeed;
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        if (boss != null)
        {
            boss.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        }
    }
    
    System.Collections.IEnumerator ExecuteMeleeAttack(BossEnemy boss, BossAttack attack, Transform player)
    {
        // 근접 공격 애니메이션
        Animator animator = boss.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        
        // 선행 동작
        yield return new WaitForSeconds(attack.windupTime);
        
        // 데미지 적용
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(boss.transform.position, player.position);
            if (distanceToPlayer <= attack.range)
            {
                Health playerHealth = player.GetComponent<Health>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage((int)attack.damage);
                }
            }
        }
        
        // 공격 이펙트
        if (attack.attackEffect != null)
        {
            Instantiate(attack.attackEffect, boss.transform.position, Quaternion.identity);
        }
    }
    
    System.Collections.IEnumerator ExecuteRangedAttack(BossEnemy boss, BossAttack attack, Transform player)
    {
        // 원거리 공격 애니메이션
        Animator animator = boss.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("RangedAttack");
        }
        
        // 선행 동작
        yield return new WaitForSeconds(attack.windupTime);
        
        // 투사체 발사 (나중에 구현)
        if (player != null)
        {
            Vector2 direction = (player.position - boss.transform.position).normalized;
            // GameObject projectile = Instantiate(attack.projectilePrefab, boss.transform.position, Quaternion.identity);
            // projectile.GetComponent<Projectile>().Init(direction, (int)attack.damage);
        }
    }
    
    System.Collections.IEnumerator ExecuteChargeAttack(BossEnemy boss, BossAttack attack, Transform player)
    {
        // 돌진 공격 애니메이션
        Animator animator = boss.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Charge");
        }
        
        // 선행 동작
        yield return new WaitForSeconds(attack.windupTime);
        
        // 돌진 실행
        if (player != null)
        {
            Vector2 chargeDirection = (player.position - boss.transform.position).normalized;
            Rigidbody2D rb = boss.GetComponent<Rigidbody2D>();
            
            float chargeSpeed = boss.moveSpeed * 2f;
            float chargeDuration = 1f;
            
            float timer = 0f;
            while (timer < chargeDuration)
            {
                if (boss == null) yield break;
                
                rb.linearVelocity = chargeDirection * chargeSpeed;
                timer += Time.deltaTime;
                yield return null;
            }
            
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }
    
    System.Collections.IEnumerator ExecuteSummonMinions(BossEnemy boss, BossAttack attack)
    {
        // 하수인 소환 애니메이션
        Animator animator = boss.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Summon");
        }
        
        // 하수인 소환 로직 (BossEnemy에서 처리)
        // boss.SummonMinions();
        
        yield return new WaitForSeconds(1f);
    }
    
    System.Collections.IEnumerator ExecuteAreaAttack(BossEnemy boss, BossAttack attack)
    {
        // 범위 공격 애니메이션
        Animator animator = boss.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("AreaAttack");
        }
        
        // 선행 동작
        yield return new WaitForSeconds(attack.windupTime);
        
        // 범위 내 모든 플레이어에게 데미지
        Collider2D[] hits = Physics2D.OverlapCircleAll(boss.transform.position, attack.range);
        
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Health playerHealth = hit.GetComponent<Health>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage((int)attack.damage);
                }
            }
        }
        
        // 범위 공격 이펙트
        if (attack.attackEffect != null)
        {
            Instantiate(attack.attackEffect, boss.transform.position, Quaternion.identity);
        }
    }
    
    System.Collections.IEnumerator ExecuteSpecialAbility(BossEnemy boss, BossAttack attack)
    {
        // 특수 능력 애니메이션
        Animator animator = boss.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("SpecialAbility");
        }
        
        // 특수 능력 실행 (텔레포트, 실드 등)
        // boss.UseSpecialAbility();
        
        yield return new WaitForSeconds(1f);
    }
    
    void PlayEntranceEffect(Vector3 position)
    {
        if (entranceEffect != null)
        {
            Instantiate(entranceEffect, position, Quaternion.identity);
        }
        
        if (entranceSound != null)
        {
            AudioSource.PlayClipAtPoint(entranceSound, position);
        }
    }
    
    void PlayExitEffect(Vector3 position)
    {
        if (exitEffect != null)
        {
            Instantiate(exitEffect, position, Quaternion.identity);
        }
        
        if (exitSound != null)
        {
            AudioSource.PlayClipAtPoint(exitSound, position);
        }
    }
    
    // 패턴 정보 반환
    public string GetPatternInfo()
    {
        string info = $"패턴: {patternName}\n";
        info += $"페이즈: {targetPhase}\n";
        info += $"지속시간: {patternDuration}초\n";
        info += $"쿨다운: {patternCooldown}초\n";
        info += $"공격 시퀀스: {attackSequences.Count}개\n";
        info += $"설명: {patternDescription}";
        
        return info;
    }
} 