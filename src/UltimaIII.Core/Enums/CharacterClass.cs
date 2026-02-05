namespace UltimaIII.Core.Enums;

/// <summary>
/// Character classes available in Ultima III.
/// Each class has stat requirements and determines available equipment and spells.
/// </summary>
public enum CharacterClass
{
    Fighter,    // Basic warrior, no magic
    Cleric,     // Healer, cleric spells
    Wizard,     // Offensive magic, wizard spells
    Thief,      // Stealth abilities, can disarm traps
    Paladin,    // Fighter + Cleric spells
    Barbarian,  // Extra HP, no magic
    Lark,       // Thief + Wizard spells
    Illusionist,// Wizard + Thief abilities
    Druid,      // Cleric + Wizard spells
    Alchemist,  // Limited both spell types
    Ranger      // Fighter + limited Wizard spells
}
