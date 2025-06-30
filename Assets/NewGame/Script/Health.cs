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
    
    [Header("디버그")]
    public bool isInvincible = false;
    
    void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    public void TakeDamage(int damage)
    {
        Debug.Log($"[Health DEBUG] {gameObject.name}.TakeDamage({damage}) 호출됨");
        
        // 무적 시간 체크
        if (Time.time - lastDamageTime < invincibilityTime)
        {
            Debug.Log($"[Health DEBUG] ❌ 무적 시간 중 (남은 시간: {invincibilityTime - (Time.time - lastDamageTime):F2}초)");
            return;
        }
            
        if (isInvincible)
        {
            Debug.Log($"[Health DEBUG] ❌ 무적 상태");
            return;
        }
        
        int previousHealth = currentHealth;
            
        // 데미지 적용
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        lastDamageTime = Time.time;
        
        Debug.Log($"[Health DEBUG] ✅ 데미지 적용: {previousHealth} → {currentHealth} (데미지: {damage})");
        
        // 이벤트 호출
        OnDamaged?.Invoke(damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        // 죽음 체크
        if (currentHealth <= 0)
        {
            Debug.Log($"[Health DEBUG] 💀 체력이 0이 되었습니다. 죽음 처리");
            Die();
        }
    }
    
    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(maxHealth, currentHealth);
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log($"[Health] {gameObject.name}이(가) {amount} 회복했습니다. 현재 체력: {currentHealth}/{maxHealth}");
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
        Debug.Log($"[Health] {gameObject.name}이(가) 죽었습니다.");
        OnDeath?.Invoke();
    }
} 