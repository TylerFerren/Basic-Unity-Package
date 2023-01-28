using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSystem : MonoBehaviour
{
    [field: SerializeField] public int CurrentLevel { get; private set; }
    [field: SerializeField] public int MaxLevel { get; private set; }

    [field: SerializeField] public float CurrentExperience { get; private set; }
    [field: SerializeField] public float ExperienceToNextLevel { get; private set; }
    [field: SerializeField] public AnimationCurve ExperenceCurve { get; private set; }

    private void Awake()
    {
        SetExperienceToNextLevel();
    }

    public void EarnExperience(int experience) {
        CurrentExperience += experience;

        if (CurrentExperience >= ExperienceToNextLevel) {
            CurrentExperience -= ExperienceToNextLevel;
            CurrentLevel = Mathf.Clamp(CurrentLevel + 1, 0, MaxLevel);
            SetExperienceToNextLevel();
        }
    }

    void SetExperienceToNextLevel() {
        ExperienceToNextLevel = Mathf.Round(ExperenceCurve.Evaluate(CurrentLevel + 1));
    }

    public void GetExperienceSource(ExperienceSource source) {
        EarnExperience(source.ExperienceReward);
    }

    public void GetExperienceSource(Collider collider) {
        if (collider.TryGetComponent(out ExperienceSource source)) {
            EarnExperience(source.ExperienceReward);
        }
    }

}
