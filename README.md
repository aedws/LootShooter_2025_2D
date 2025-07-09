# LootShooter_2025_2D : 2025 하반기 개인 프로젝트 구성

## 개발 목표 : 
- 개인 개발 능력 향상
- 유니티 6대응 능력 향상
- 커서 AI 활용 능력 향상
- 에셋 스토어 활용도 향상
- Google Sheet API 활용
- 실시간 데이터테이블 반영 관련 적용

### 목차
-----
- 데이터 테이블 관리 형식
- 이후 정리하여 추가 예정
-----
### 스프레드시트(데이터 테이블)관리 형식
#### 무기 데이터 (Weapons) - 34개 컬럼
| 컬럼 | 필드명 | 설명 | 예시 |
|------|--------|------|------|
| A | weaponName | 무기 이름 | M4A1 |
| B | weaponType | 무기 타입 | AR, HG, MG, SG, SMG, SR |
| C | rarity | 희귀도 | Common, Rare, Epic, Legendary, Primordial |
| D | flavorText | 무기 설명 | 강력한 돌격소총입니다. |
| E | fireRate | 발사 속도 | 0.1 |
| F | damage | 데미지 | 25 |
| G | projectileSpeed | 총알 속도 | 10.0 |
| H | maxAmmo | 최대 탄약 | 30 |
| I | currentAmmo | 현재 탄약 | 30 |
| J | reloadTime | 재장전 시간 | 2.0 |
| K | infiniteAmmo | 무한 탄약 | false |
| L | baseSpread | 기본 탄퍼짐 | 0.0 |
| M | maxSpread | 최대 탄퍼짐 | 5.0 |
| N | spreadIncreaseRate | 탄퍼짐 증가율 | 1.0 |
| O | spreadDecreaseRate | 탄퍼짐 감소율 | 2.0 |
| P | pelletsPerShot | 샷건 탄환 수 | 6 |
| Q | shotgunSpreadAngle | 샷건 퍼짐 각도 | 30.0 |
| R | warmupTime | 머신건 예열 시간 | 1.0 |
| S | maxWarmupFireRate | 최대 예열 발사속도 | 0.05 |
| T | singleFireOnly | 단발 전용 | true |
| U | aimingRange | 조준 거리 | 15.0 |
| V | movementSpeedMultiplier | 이동속도 배율 | 1.0 |
| W | recoilForce | 반동 강도 | 1.0 |
| X | recoilDuration | 반동 지속시간 | 0.1 |
| Y | recoilRecoverySpeed | 반동 회복속도 | 5.0 |
| Z | criticalChance | 크리티컬 확률 | 0.1 |
| AA | criticalMultiplier | 크리티컬 배율 | 2.0 |
| AB | pierceCount | 관통 개수 | 0 |
| AC | pierceDamageReduction | 관통 데미지 감소 | 0.1 |
| AD | hasTracerRounds | 예광탄 효과 | false |
| AE | hasMuzzleFlash | 화염 효과 | true |
| AF | hasExplosiveKills | 폭발 효과 | false |
| AG | explosionRadius | 폭발 반경 | 2.0 |
| AH | smgDashSpeedBonus | SMG 대시 후 이동속도 증가량 | 2.0 |
| AI | smgDashSpeedDuration | SMG 대시 후 이동속도 증가 지속시간 | 3.0 |


#### 방어구 데이터 (Armors) - 14개 컬럼
| 컬럼 | 필드명 | 설명 | 예시 |
|------|--------|------|------|
| A | armorName | 방어구 이름 | 강화 헬멧 |
| B | armorType | 방어구 타입 | Helmet, Chest, Legs, Boots, Shoulder, Accessory |
| C | rarity | 희귀도 | Common, Rare, Epic, Legendary |
| D | description | 설명 | 머리를 보호하는 강화 헬멧입니다. |
| E | defense | 방어력 | 15 |
| F | maxHealth | 최대 체력 보너스 | 0 |
| G | damageReduction | 데미지 감소율 | 0.1 |
| H | moveSpeedBonus | 이동속도 보너스 | 0.0 |
| I | jumpForceBonus | 점프력 보너스 | 0.0 |
| J | dashCooldownReduction | 대시 쿨다운 감소 | 0.0 |
| K | hasRegeneration | 체력 재생 | false |
| L | regenerationRate | 재생 속도 | 1.0 |
| M | hasInvincibilityFrame | 무적 시간 증가 | false |
| N | invincibilityBonus | 무적 시간 보너스 | 0.0 | 