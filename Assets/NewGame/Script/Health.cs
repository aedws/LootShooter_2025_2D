using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    [Header("체력 설정")]
    public int maxHealth = 100;
    public int currentHealth;
    
    [Header("무적 시간")]
    public float invincibilityTime = 0.5f;
    private float lastDamageTime = -1f;
    
    // 이벤트들
    public event Action<int, int> OnHealthChanged; // (current, max)
    public event Action OnDeath;
    public event Action<int> OnDamaged; // damage amount
    public event Action<int> OnHealed; // heal amount
    
    [Header("디버그")]
    public bool isInvincible = false;
    private bool isDead = false;
    
    void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    public void TakeDamage(int damage)
    {
        // Debug.Log($"[Health DEBUG] {gameObject.name}.TakeDamage({damage}) 호출됨");
        
        // 이미 죽었으면 데미지 무시
        if (isDead) return;
        
        // Debug.Log($"[Health DEBUG] ❌ 무적 시간 중 (남은 시간: {invincibilityTime - (Time.time - lastDamageTime):F2}초)");
        
        // 무적 상태면 데미지 무시
        if (isInvincible)
        {
            // Debug.Log($"[Health DEBUG] ❌ 무적 상태");
            return;
        }
        
        // 무적 시간 체크
        if (Time.time - lastDamageTime < invincibilityTime)
        {
            return;
        }
        
        lastDamageTime = Time.time;
        
        int previousHealth = currentHealth;
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        // Debug.Log($"[Health DEBUG] ✅ 데미지 적용: {previousHealth} → {currentHealth} (데미지: {damage})");
        
        // 이벤트 발생
        OnDamaged?.Invoke(damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        // 체력이 0이 되면 죽음 처리
        if (currentHealth <= 0 && !isDead)
        {
            // Debug.Log($"[Health DEBUG] 💀 체력이 0이 되었습니다. 죽음 처리");
            Die();
        }
    }
    
    public void Heal(int amount)
    {
        if (isDead) return;
        
        currentHealth += amount;
        currentHealth = Mathf.Min(maxHealth, currentHealth);
        
        // Debug.Log($"[Health] {gameObject.name}이(가) {amount} 회복했습니다. 현재 체력: {currentHealth}/{maxHealth}");
        
        OnHealed?.Invoke(amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    public void SetMaxHealth(int newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    public void SetInvincible(bool invincible)
    {
        isInvincible = invincible;
    }
    
    public bool IsAlive()
    {
        return currentHealth > 0;
    }
    
    public float GetHealthPercentage()
    {
        return maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
    }
    
    void Die()
    {
        if (isDead) return;
        
        isDead = true;
        // Debug.Log($"[Health] {gameObject.name}이(가) 죽었습니다.");
        OnDeath?.Invoke();
    }
} 