using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UltimaIII.Avalonia.Services.Audio;
using UltimaIII.Core.Engine;
using UltimaIII.Core.Enums;
using UltimaIII.Core.Models;

namespace UltimaIII.Avalonia.ViewModels;

public partial class FortuneTellerViewModel : ViewModelBase
{
    private readonly GameEngine _gameEngine;
    private readonly MainViewModel _mainViewModel;
    private readonly IAudioService _audioService;
    private int _currentPhase; // 0=name, 1-7=questions, 8=reveal
    private readonly Dictionary<CharacterClass, int> _classWeights = new();
    private readonly Dictionary<Race, int> _raceWeights = new();
    private CharacterClass _chosenClass;
    private Race _chosenRace;

    [ObservableProperty]
    private string _gypsyText = string.Empty;

    [ObservableProperty]
    private string _characterName = string.Empty;

    [ObservableProperty]
    private bool _isNamePhase = true;

    [ObservableProperty]
    private bool _isQuestionPhase;

    [ObservableProperty]
    private bool _isRevealPhase;

    [ObservableProperty]
    private bool _canProceed;

    public ObservableCollection<AnswerChoiceViewModel> CurrentAnswers { get; } = new();

    public FortuneTellerViewModel(GameEngine gameEngine, MainViewModel mainViewModel)
    {
        _gameEngine = gameEngine;
        _mainViewModel = mainViewModel;
        _audioService = AudioService.Instance;

        foreach (var cls in Enum.GetValues<CharacterClass>())
            _classWeights[cls] = 0;
        foreach (var race in Enum.GetValues<Race>())
            _raceWeights[race] = 0;

        GypsyText = FortuneQuestions.NamePromptText;
    }

    partial void OnCharacterNameChanged(string value)
    {
        CanProceed = !string.IsNullOrWhiteSpace(value);
    }

    [RelayCommand]
    private void SubmitName()
    {
        if (string.IsNullOrWhiteSpace(CharacterName)) return;
        _audioService.PlaySoundEffect(SoundEffect.MenuConfirm);
        CharacterName = CharacterName.Trim();
        TransitionToQuestion(1);
    }

    [RelayCommand]
    private void SelectAnswer(int answerIndex)
    {
        var question = FortuneQuestions.Questions[_currentPhase - 1];
        if (answerIndex < 0 || answerIndex >= question.Answers.Count) return;

        _audioService.PlaySoundEffect(SoundEffect.MenuSelect);

        var answer = question.Answers[answerIndex];

        foreach (var (cls, weight) in answer.ClassWeights)
            _classWeights[cls] += weight;
        foreach (var (race, weight) in answer.RaceWeights)
            _raceWeights[race] += weight;

        if (_currentPhase < 7)
            TransitionToQuestion(_currentPhase + 1);
        else
            TransitionToReveal();
    }

    private void TransitionToQuestion(int phase)
    {
        _currentPhase = phase;
        IsNamePhase = false;
        IsQuestionPhase = true;
        IsRevealPhase = false;

        var question = FortuneQuestions.Questions[phase - 1];
        GypsyText = question.NarrativeText;

        CurrentAnswers.Clear();
        string[] labels = { "A)", "B)", "C)", "D)" };
        for (int i = 0; i < question.Answers.Count; i++)
        {
            CurrentAnswers.Add(new AnswerChoiceViewModel
            {
                Label = labels[i],
                Text = question.Answers[i].Text,
                Index = i
            });
        }
    }

    private void TransitionToReveal()
    {
        _currentPhase = 8;
        IsNamePhase = false;
        IsQuestionPhase = false;
        IsRevealPhase = true;
        CurrentAnswers.Clear();

        (_chosenClass, _chosenRace) = DetermineClassAndRace();

        GypsyText = string.Format(FortuneQuestions.RevealTemplate,
            CharacterName, _chosenClass, _chosenRace) +
            "\n\n" +
            string.Format(FortuneQuestions.FarewellText, CharacterName);
    }

    private (CharacterClass, Race) DetermineClassAndRace()
    {
        var sortedClasses = _classWeights.OrderByDescending(kv => kv.Value).ToList();
        var sortedRaces = _raceWeights.OrderByDescending(kv => kv.Value).ToList();

        var chosenClass = sortedClasses[0].Key;

        var chosenRace = Race.Human;
        foreach (var (race, _) in sortedRaces)
        {
            if (CanMeetRequirements(chosenClass, race))
            {
                chosenRace = race;
                break;
            }
        }

        return (chosenClass, chosenRace);
    }

    private static bool CanMeetRequirements(CharacterClass cls, Race race)
    {
        var reqs = ClassDefinition.Get(cls).Requirements;
        var mods = RaceDefinition.Get(race).StatModifiers;

        int minStr = Math.Clamp(Math.Max(Stats.MinStat, reqs.MinStrength - mods.StrengthMod), Stats.MinStat, Stats.MaxStat);
        int minDex = Math.Clamp(Math.Max(Stats.MinStat, reqs.MinDexterity - mods.DexterityMod), Stats.MinStat, Stats.MaxStat);
        int minInt = Math.Clamp(Math.Max(Stats.MinStat, reqs.MinIntelligence - mods.IntelligenceMod), Stats.MinStat, Stats.MaxStat);
        int minWis = Math.Clamp(Math.Max(Stats.MinStat, reqs.MinWisdom - mods.WisdomMod), Stats.MinStat, Stats.MaxStat);

        return (minStr + minDex + minInt + minWis) <= Stats.StartingStatPoints;
    }

    private static Stats OptimizeStats(CharacterClass cls, Race race)
    {
        var reqs = ClassDefinition.Get(cls).Requirements;
        var mods = RaceDefinition.Get(race).StatModifiers;

        // Start with minimum base stats needed to meet class requirements after racial mods
        int[] stats =
        {
            Math.Clamp(Math.Max(Stats.MinStat, reqs.MinStrength - mods.StrengthMod), Stats.MinStat, Stats.MaxStat),
            Math.Clamp(Math.Max(Stats.MinStat, reqs.MinDexterity - mods.DexterityMod), Stats.MinStat, Stats.MaxStat),
            Math.Clamp(Math.Max(Stats.MinStat, reqs.MinIntelligence - mods.IntelligenceMod), Stats.MinStat, Stats.MaxStat),
            Math.Clamp(Math.Max(Stats.MinStat, reqs.MinWisdom - mods.WisdomMod), Stats.MinStat, Stats.MaxStat)
        };

        int remaining = Stats.StartingStatPoints - stats.Sum();

        // Distribute remaining points in class priority order
        int[] priority = GetStatPriority(cls);
        foreach (int idx in priority)
        {
            int addable = Math.Min(remaining, Stats.MaxStat - stats[idx]);
            stats[idx] += addable;
            remaining -= addable;
            if (remaining <= 0) break;
        }

        // Overflow: distribute to any stat not yet maxed
        while (remaining > 0)
        {
            bool distributed = false;
            for (int i = 0; i < 4 && remaining > 0; i++)
            {
                if (stats[i] < Stats.MaxStat)
                {
                    stats[i]++;
                    remaining--;
                    distributed = true;
                }
            }
            if (!distributed) break;
        }

        return new Stats(stats[0], stats[1], stats[2], stats[3]);
    }

    // Priority indices: 0=STR, 1=DEX, 2=INT, 3=WIS
    private static int[] GetStatPriority(CharacterClass cls) => cls switch
    {
        CharacterClass.Fighter     => new[] { 0, 1, 3, 2 },
        CharacterClass.Cleric      => new[] { 3, 0, 1, 2 },
        CharacterClass.Wizard      => new[] { 2, 3, 1, 0 },
        CharacterClass.Thief       => new[] { 1, 0, 2, 3 },
        CharacterClass.Paladin     => new[] { 0, 3, 1, 2 },
        CharacterClass.Barbarian   => new[] { 0, 1, 2, 3 },
        CharacterClass.Lark        => new[] { 1, 2, 0, 3 },
        CharacterClass.Illusionist => new[] { 2, 1, 3, 0 },
        CharacterClass.Druid       => new[] { 2, 3, 1, 0 },
        CharacterClass.Alchemist   => new[] { 2, 3, 1, 0 },
        CharacterClass.Ranger      => new[] { 0, 1, 2, 3 },
        _                          => new[] { 0, 1, 2, 3 }
    };

    [RelayCommand]
    private void BeginAdventure()
    {
        _audioService.PlaySoundEffect(SoundEffect.MenuConfirm);

        var stats = OptimizeStats(_chosenClass, _chosenRace);
        var character = Character.Create(CharacterName, _chosenRace, _chosenClass, stats);
        _gameEngine.Party.AddMember(character);

        _mainViewModel.StartGame();
    }
}

public partial class AnswerChoiceViewModel : ObservableObject
{
    [ObservableProperty]
    private string _text = string.Empty;

    [ObservableProperty]
    private string _label = string.Empty;

    public int Index { get; init; }
}
