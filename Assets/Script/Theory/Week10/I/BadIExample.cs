using UnityEngine;

public class BadIExample : MonoBehaviour
{
    public interface ICharacterSkills // Interface รวมใหญ่
    {
        // สกิลทางกายภาพ
        void PhysicalAttack();
        void UseShield();

        // สกิลเวทมนตร์
        void CastSpell(string spellName);
        void CheckMana();

        // สกิลสนับสนุน
        void Heal(int amount);
        void ApplyBuff();
    }

    // คลาส นักรบ (Warrior) - เน้นการต่อสู้ทางกายภาพ
    public class MeleeFighter : ICharacterSkills
    {
        // ใช้งานจริง: สกิลทางกายภาพ
        public void PhysicalAttack()
        {
            Debug.Log("Warrior swings sword for physical damage.");
        }

        public void UseShield()
        {
            Debug.Log("Warrior raises shield to block.");
        }

        // โค้ดที่ถูกบังคับให้มี: สกิลเวทมนตร์และสนับสนุน
        public void CastSpell(string spellName)
        {
            // นักรบไม่มีเวทมนตร์ จึงถูกบังคับให้มีเมธอดที่ว่างเปล่าหรือโยน Exception
            throw new System.Exception("A Melee Fighter cannot cast spells!");
        }

        public void CheckMana()
        {
            // นักรบไม่มีมานา จึงถูกบังคับให้มีเมธอดที่ไม่มีความหมาย
        }

        public void Heal(int amount)
        {
            // นักรบไม่สามารถรักษาตัวเองได้
            Debug.Log("Melee Fighter tries to heal... but fails.");
        }

        public void ApplyBuff()
        {
            // บังคับให้ต้องมีเมธอดนี้
        }
    }
}
