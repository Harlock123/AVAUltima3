using System.Collections.Generic;
using System.Linq;
using UltimaIII.Core.Enums;
using UltimaIII.Core.Models;

namespace UltimaIII.Core.Engine;

public static class FieldSpellService
{
    public static List<Spell> GetFieldSpells(Character caster)
    {
        return Spell.GetSpellsForClass(caster.ClassDef)
            .Where(s => !s.IsCombatOnly)
            .OrderBy(s => s.Level)
            .ToList();
    }

    public static bool CanCastFieldSpells(Character c)
    {
        if (!c.IsAlive || !c.CanAct) return false;
        if (c.CurrentMP <= 0) return false;
        return GetFieldSpells(c).Count > 0;
    }

    public static string CastFieldSpell(Character caster, Spell spell, Character target)
    {
        if (!caster.CanAct)
            return $"{caster.Name} cannot act!";

        if (spell.IsCombatOnly)
            return $"{spell.Name} can only be used in combat!";

        if (!caster.SpendMana(spell.ManaCost))
            return $"Not enough MP! ({spell.ManaCost} needed)";

        // Healing
        if (spell.HealAmount > 0)
        {
            if (!target.IsAlive)
                return $"{target.Name} is dead. {caster.Name} wasted {spell.ManaCost} MP.";

            int before = target.CurrentHP;
            target.Heal(spell.HealAmount);
            int healed = target.CurrentHP - before;
            return $"{caster.Name} casts {spell.Name}! {target.Name} healed for {healed} HP.";
        }

        // Cure status
        if (spell.CuresStatus != StatusEffect.None)
        {
            // Special case: Rec_Su resurrects dead allies
            if (spell.CuresStatus.HasFlag(StatusEffect.Dead))
            {
                if (target.IsAlive)
                    return $"{target.Name} is not dead! {caster.Name} wasted {spell.ManaCost} MP.";

                target.Status = StatusEffect.None;
                target.CurrentHP = 1;
                return $"{caster.Name} casts {spell.Name}! {target.Name} is resurrected!";
            }

            var cured = target.Status & spell.CuresStatus;
            if (cured != StatusEffect.None)
            {
                target.Status &= ~spell.CuresStatus;
                return $"{caster.Name} casts {spell.Name}! {target.Name} cured of {cured}.";
            }
            return $"{caster.Name} casts {spell.Name} on {target.Name}, but there is nothing to cure.";
        }

        // Self-targeting utility spells (light, locate, protection, etc.)
        return $"{caster.Name} casts {spell.Name}.";
    }

    public static string CastPartySpell(Character caster, Spell spell, IReadOnlyList<Character> party)
    {
        if (!caster.CanAct)
            return $"{caster.Name} cannot act!";

        if (!caster.SpendMana(spell.ManaCost))
            return $"Not enough MP! ({spell.ManaCost} needed)";

        if (spell.HealAmount > 0)
        {
            int totalHealed = 0;
            int count = 0;
            foreach (var member in party)
            {
                if (!member.IsAlive) continue;
                int before = member.CurrentHP;
                member.Heal(spell.HealAmount);
                totalHealed += member.CurrentHP - before;
                count++;
            }
            return $"{caster.Name} casts {spell.Name}! Party healed ({count} members, {totalHealed} total HP).";
        }

        if (spell.CuresStatus != StatusEffect.None)
        {
            int count = 0;
            foreach (var member in party)
            {
                if (!member.IsAlive) continue;
                var cured = member.Status & spell.CuresStatus;
                if (cured != StatusEffect.None)
                {
                    member.Status &= ~spell.CuresStatus;
                    count++;
                }
            }
            return count > 0
                ? $"{caster.Name} casts {spell.Name}! {count} party member(s) cured."
                : $"{caster.Name} casts {spell.Name}, but no one needed curing.";
        }

        return $"{caster.Name} casts {spell.Name} on the party.";
    }
}
