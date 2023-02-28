using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Codesign
{
    public class LevelSystem : MonoBehaviour
    {
        [field: SerializeField] public int CurrentLevel { get; private set; } = 1;
        [field: SerializeField] public int MaxLevel { get; private set; } = 10;

        [field: SerializeField] public float CurrentExperience { get; private set; }
        [field: SerializeField] public float ExperienceToNextLevel { get; private set; }

        [SerializeField] private EvaluationCurve experienceCurve;

        [FoldoutGroup("Event")] public UnityEvent OnLevelUpdate;

        private void OnValidate()
        {
            SetExperienceToNextLevel();
            experienceCurve.TestEvaluate();
        }

        private void Awake()
        {
            SetExperienceToNextLevel();
        }

        public void EarnExperience(int experience)
        {
            if (CurrentLevel == MaxLevel) return;

            CurrentExperience += experience;
            while (CurrentExperience >= ExperienceToNextLevel)
            {
                LevelUp();
            }
        }

        void LevelUp()
        {
            CurrentExperience -= ExperienceToNextLevel;
            CurrentLevel = Mathf.Clamp(CurrentLevel + 1, 0, MaxLevel);
            SetExperienceToNextLevel();
            OnLevelUpdate?.Invoke();
        }

        void SetExperienceToNextLevel()
        {
            ExperienceToNextLevel = experienceCurve.EvaluateInt(CurrentLevel);
        }

        public void GetExperienceSource(ExperienceSource source)
        {
            EarnExperience(source.ExperienceReward);
        }

        public void GetExperienceSource(Collider collider)
        {
            if (collider.TryGetComponent(out ExperienceSource source))
            {
                EarnExperience(source.ExperienceReward);
            }
        }
    }
}