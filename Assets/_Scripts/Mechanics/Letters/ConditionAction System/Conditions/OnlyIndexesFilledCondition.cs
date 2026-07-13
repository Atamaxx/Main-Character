using System.Collections.Generic;
using UnityEngine;

namespace Letters
{
    public class OnlyIndexesFilledCondition : Condition
    {
        [SerializeField] private LetterFiller _letterFiller;
        [SerializeField] private List<int> _letterIndexes = new List<int>();

        public override bool IsConditionMet()
        {
            return CheckIfOnlyIndexesFilled(_letterIndexes, _letterFiller);
        }

        public bool CheckIfOnlyIndexesFilled(List<int> allowedIndexes, LetterFiller letterFiller)
        {
            if (letterFiller.TmpText == null)
                return false;

            if (letterFiller.filledCharacters == null)
                return false;

            // Ensure all allowed indexes are within bounds.
            foreach (int index in allowedIndexes)
            {
                if (index < 0 || index >= letterFiller.filledCharacters.Length)
                    return false;
            }

            // Convert allowedIndexes to a HashSet for faster lookups.
            HashSet<int> allowedSet = new HashSet<int>(allowedIndexes);

            // Check each index of the filledCharacters array.
            for (int i = 0; i < letterFiller.filledCharacters.Length; i++)
            {
                if (allowedSet.Contains(i))
                {
                    // Allowed index must be filled.
                    if (!letterFiller.filledCharacters[i])
                        return false;
                }
                else
                {
                    // Non-allowed index must not be filled.
                    if (letterFiller.filledCharacters[i])
                        return false;
                }
            }
            return true;
        }
    }
}
