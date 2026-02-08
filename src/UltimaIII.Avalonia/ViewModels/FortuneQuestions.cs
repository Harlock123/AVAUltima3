using System.Collections.Generic;
using UltimaIII.Core.Enums;

namespace UltimaIII.Avalonia.ViewModels;

public record FortuneAnswer(
    string Text,
    Dictionary<CharacterClass, int> ClassWeights,
    Dictionary<Race, int> RaceWeights
);

public record FortuneQuestion(
    string NarrativeText,
    List<FortuneAnswer> Answers
);

public static class FortuneQuestions
{
    public const string NamePromptText =
        "Come closer, child... Sit.\n\n" +
        "I am Madame Zara, reader of fortunes and seer of fates. " +
        "The stars have drawn thee to my wagon on this moonlit night, " +
        "and I sense a great destiny stirring within thee.\n\n" +
        "Before I read the stars for thee, tell me... What is thy name?";

    public const string RevealTemplate =
        "The crystal ball blazes with inner light! The spirits cry out in unison!\n\n" +
        "I see it now, {0}! Thy true nature is revealed at last...\n\n" +
        "The blood of the {2} flows through thy veins, " +
        "and thou art destined to walk the path of the {1}!";

    public const string FarewellText =
        "The die have been cast. The chips have been set into motion.\n\n" +
        "Go now, {0}. Thy destiny awaits beyond this forest. " +
        "Others will join thy cause in time, drawn by the same fate " +
        "that brought thee to my wagon this night.\n\n" +
        "May the stars guide thy steps...";

    public static readonly List<FortuneQuestion> Questions = new()
    {
        // Q1: The Crystal Ball
        new FortuneQuestion(
            "I peer into the crystal ball... I see a crossroads. " +
            "You stand before two paths. One leads through a dark forest, " +
            "the other across a sunlit meadow. But then a third path appears " +
            "-- a crumbling staircase descending into the earth.\n\n" +
            "Which calls to you?",
            new List<FortuneAnswer>
            {
                new("The dark forest -- I fear nothing.",
                    new() { [CharacterClass.Fighter] = 3, [CharacterClass.Barbarian] = 3, [CharacterClass.Ranger] = 2 },
                    new() { [Race.Human] = 2, [Race.Dwarf] = 2 }),
                new("The sunlit meadow -- beauty guides me.",
                    new() { [CharacterClass.Cleric] = 3, [CharacterClass.Druid] = 2, [CharacterClass.Paladin] = 2 },
                    new() { [Race.Bobbit] = 3, [Race.Elf] = 1 }),
                new("The staircase below -- secrets await.",
                    new() { [CharacterClass.Wizard] = 3, [CharacterClass.Illusionist] = 2, [CharacterClass.Alchemist] = 2 },
                    new() { [Race.Fuzzy] = 3, [Race.Elf] = 1 }),
                new("I would wait and watch before choosing.",
                    new() { [CharacterClass.Thief] = 3, [CharacterClass.Lark] = 2, [CharacterClass.Ranger] = 2 },
                    new() { [Race.Elf] = 2, [Race.Human] = 2 })
            }
        ),

        // Q2: The Cards of Fate
        new FortuneQuestion(
            "I draw three cards from the deck of fate... " +
            "The first shows a great battle. Swords clash and shields splinter.\n\n" +
            "Tell me, how do you prevail in conflict?",
            new List<FortuneAnswer>
            {
                new("With steel in hand and strength of arm.",
                    new() { [CharacterClass.Fighter] = 3, [CharacterClass.Barbarian] = 2, [CharacterClass.Paladin] = 1 },
                    new() { [Race.Dwarf] = 3, [Race.Human] = 1 }),
                new("With prayer and divine protection.",
                    new() { [CharacterClass.Cleric] = 3, [CharacterClass.Paladin] = 3, [CharacterClass.Druid] = 1 },
                    new() { [Race.Bobbit] = 2, [Race.Dwarf] = 2 }),
                new("With cunning and a blade in the dark.",
                    new() { [CharacterClass.Thief] = 3, [CharacterClass.Lark] = 2, [CharacterClass.Illusionist] = 1 },
                    new() { [Race.Elf] = 3, [Race.Human] = 1 }),
                new("With arcane power -- fire and lightning.",
                    new() { [CharacterClass.Wizard] = 3, [CharacterClass.Illusionist] = 2, [CharacterClass.Druid] = 1 },
                    new() { [Race.Fuzzy] = 3, [Race.Elf] = 1 })
            }
        ),

        // Q3: The Moon Reading
        new FortuneQuestion(
            "The twin moons whisper their secrets... " +
            "One moon asks: what do you treasure most in a companion?",
            new List<FortuneAnswer>
            {
                new("Loyalty and unwavering courage.",
                    new() { [CharacterClass.Paladin] = 3, [CharacterClass.Fighter] = 2, [CharacterClass.Barbarian] = 1 },
                    new() { [Race.Dwarf] = 2, [Race.Human] = 2 }),
                new("Wisdom and spiritual depth.",
                    new() { [CharacterClass.Cleric] = 3, [CharacterClass.Druid] = 3, [CharacterClass.Alchemist] = 1 },
                    new() { [Race.Bobbit] = 3, [Race.Dwarf] = 1 }),
                new("Cleverness and quick thinking.",
                    new() { [CharacterClass.Thief] = 2, [CharacterClass.Lark] = 3, [CharacterClass.Ranger] = 1 },
                    new() { [Race.Elf] = 3, [Race.Fuzzy] = 1 }),
                new("Knowledge of forbidden things.",
                    new() { [CharacterClass.Wizard] = 2, [CharacterClass.Alchemist] = 3, [CharacterClass.Illusionist] = 2 },
                    new() { [Race.Fuzzy] = 3, [Race.Elf] = 1 })
            }
        ),

        // Q4: The Palm Reading
        new FortuneQuestion(
            "Give me your hand, child... Ah, your lifeline tells a story. " +
            "I see you in a great library, ancient tomes surrounding you.\n\n" +
            "What draws your eye?",
            new List<FortuneAnswer>
            {
                new("A manual of swordcraft and tactics.",
                    new() { [CharacterClass.Fighter] = 2, [CharacterClass.Barbarian] = 2, [CharacterClass.Ranger] = 3 },
                    new() { [Race.Human] = 3, [Race.Dwarf] = 1 }),
                new("Sacred texts of healing and blessing.",
                    new() { [CharacterClass.Cleric] = 3, [CharacterClass.Paladin] = 2, [CharacterClass.Druid] = 1 },
                    new() { [Race.Bobbit] = 2, [Race.Human] = 2 }),
                new("A grimoire of powerful incantations.",
                    new() { [CharacterClass.Wizard] = 3, [CharacterClass.Illusionist] = 3, [CharacterClass.Lark] = 1 },
                    new() { [Race.Fuzzy] = 2, [Race.Elf] = 2 }),
                new("A journal of herbcraft and alchemy.",
                    new() { [CharacterClass.Alchemist] = 3, [CharacterClass.Druid] = 3, [CharacterClass.Ranger] = 1 },
                    new() { [Race.Bobbit] = 2, [Race.Fuzzy] = 1, [Race.Human] = 1 })
            }
        ),

        // Q5: The Tea Leaves
        new FortuneQuestion(
            "I swirl the cup and read the leaves... They form shapes. " +
            "I see a child in danger on a cliff's edge.\n\n" +
            "What do you do?",
            new List<FortuneAnswer>
            {
                new("Rush forward to grab them before they fall.",
                    new() { [CharacterClass.Barbarian] = 3, [CharacterClass.Fighter] = 2, [CharacterClass.Paladin] = 2 },
                    new() { [Race.Human] = 2, [Race.Dwarf] = 2 }),
                new("Pray for divine intervention.",
                    new() { [CharacterClass.Cleric] = 3, [CharacterClass.Paladin] = 2, [CharacterClass.Druid] = 1 },
                    new() { [Race.Bobbit] = 3, [Race.Dwarf] = 1 }),
                new("Use a levitation spell to lift them to safety.",
                    new() { [CharacterClass.Wizard] = 2, [CharacterClass.Illusionist] = 3, [CharacterClass.Lark] = 2 },
                    new() { [Race.Fuzzy] = 2, [Race.Elf] = 2 }),
                new("Carefully creep along the edge to reach them.",
                    new() { [CharacterClass.Thief] = 3, [CharacterClass.Ranger] = 2, [CharacterClass.Lark] = 1 },
                    new() { [Race.Elf] = 3, [Race.Bobbit] = 1 })
            }
        ),

        // Q6: The Runes
        new FortuneQuestion(
            "I cast the runes upon the cloth... They speak of your deepest nature.\n\n" +
            "Tell me -- when the night is darkest, what sustains you?",
            new List<FortuneAnswer>
            {
                new("The fire in my blood -- rage against the darkness.",
                    new() { [CharacterClass.Barbarian] = 3, [CharacterClass.Fighter] = 3 },
                    new() { [Race.Dwarf] = 3, [Race.Human] = 1 }),
                new("Faith that the dawn will come.",
                    new() { [CharacterClass.Cleric] = 2, [CharacterClass.Paladin] = 3, [CharacterClass.Druid] = 2 },
                    new() { [Race.Bobbit] = 2, [Race.Human] = 2 }),
                new("My wits -- I adapt and survive.",
                    new() { [CharacterClass.Thief] = 2, [CharacterClass.Ranger] = 3, [CharacterClass.Alchemist] = 2 },
                    new() { [Race.Elf] = 2, [Race.Human] = 2 }),
                new("Understanding -- darkness is merely the absence of light, and I can create light.",
                    new() { [CharacterClass.Wizard] = 3, [CharacterClass.Druid] = 2, [CharacterClass.Illusionist] = 2 },
                    new() { [Race.Fuzzy] = 3, [Race.Elf] = 1 })
            }
        ),

        // Q7: The Final Vision
        new FortuneQuestion(
            "One last vision forms in the crystal... " +
            "I see the end of your story.\n\n" +
            "How do you wish to be remembered?",
            new List<FortuneAnswer>
            {
                new("As a champion who never faltered.",
                    new() { [CharacterClass.Fighter] = 2, [CharacterClass.Paladin] = 3, [CharacterClass.Barbarian] = 2 },
                    new() { [Race.Human] = 3, [Race.Dwarf] = 1 }),
                new("As a healer who eased the suffering of many.",
                    new() { [CharacterClass.Cleric] = 3, [CharacterClass.Druid] = 2, [CharacterClass.Alchemist] = 2 },
                    new() { [Race.Bobbit] = 3, [Race.Human] = 1 }),
                new("As the one who unlocked the universe's greatest secrets.",
                    new() { [CharacterClass.Wizard] = 3, [CharacterClass.Illusionist] = 2, [CharacterClass.Alchemist] = 2 },
                    new() { [Race.Fuzzy] = 3, [Race.Elf] = 1 }),
                new("As a shadow -- spoken of in whispers, but never truly seen.",
                    new() { [CharacterClass.Thief] = 3, [CharacterClass.Lark] = 3, [CharacterClass.Illusionist] = 1 },
                    new() { [Race.Elf] = 3, [Race.Fuzzy] = 1 })
            }
        )
    };
}
