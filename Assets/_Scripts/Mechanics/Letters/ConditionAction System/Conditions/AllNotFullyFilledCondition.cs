using TMPro;
using UnityEngine;
using System.Collections.Generic;

namespace Letters
{
    public class AllNotFullyFilledCondition : Condition
    {
        [SerializeField] private List<LetterFiller> _letterFillers;
        
        public override bool IsConditionMet()
        {
            if (_letterFillers == null || _letterFillers.Count == 0)
                return false;
            
            // Check that each LetterFiller is not fully filled.
            foreach (var letterFiller in _letterFillers)
            {
                // If the letterFiller or its TMP text is missing, consider the setup invalid
                // and return false.
                if (letterFiller == null || letterFiller.TmpText == null)
                    return false;
                
                // If any letter filler is fully filled, the condition is not met.
                if (IsLetterFillerFullyFilled(letterFiller))
                    return false;
            }
            
            // Every LetterFiller is missing at least one filled character.
            return true;
        }
        
        /// <summary>
        /// Checks whether the provided LetterFiller is fully filled.
        /// A fully filled LetterFiller has every visible character marked as filled.
        /// </summary>
        private bool IsLetterFillerFullyFilled(LetterFiller letterFiller)
        {
            TMP_TextInfo textInfo = letterFiller.TmpText.textInfo;
            
            // Iterate through all visible characters in the TMP text.
            for (int i = 0; i < textInfo.characterCount; i++)
            {
                var charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible)
                    continue;
                
                // If a visible character is not filled, this LetterFiller isn't fully filled.
                if (!letterFiller.filledCharacters[i])
                    return false;
            }
            
            // All visible characters are filled.
            return true;
        }
    }
}
