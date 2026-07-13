using UnityEngine;
using TMPro;
using NaughtyAttributes;

namespace Letters
{
    public class LetterSplitter : MonoBehaviour
    {
        [SerializeField] private TMP_Text _tmpText;
        
        [Button("Split Text into Letters")]
        private void SplitTextIntoLetters()
        {
            if (_tmpText == null)
            {
                Debug.LogError("TMP_Text component is not assigned!");
                return;
            }
            
            string originalText = _tmpText.text;
            TMP_TextInfo textInfo = _tmpText.textInfo;
            _tmpText.ForceMeshUpdate();
            
            GameObject container = new GameObject(originalText[0..(originalText.Length / 3)] + "_letters");
            container.transform.SetParent(_tmpText.transform.parent);
            container.transform.localPosition = _tmpText.transform.localPosition;
            container.transform.localRotation = _tmpText.transform.localRotation;
            container.transform.localScale = _tmpText.transform.localScale;
            
            for (int i = 0; i < textInfo.characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible) continue;
                
                GameObject letterObj = Instantiate(_tmpText.gameObject, container.transform);
                letterObj.name = "Letter_" + originalText[i];
                
                TMP_Text letterTMP = letterObj.GetComponent<TMP_Text>();
                if (letterTMP != null)
                {
                    letterTMP.text = originalText[i].ToString();
                }
                
                RectTransform letterTransform = letterObj.GetComponent<RectTransform>();
                if (letterTransform != null)
                {
                    Vector3 worldPosition = _tmpText.transform.TransformPoint(charInfo.bottomLeft);
                    letterTransform.position = worldPosition;
                }
            }
        }
        
        [Button("Split Text into Words")]
        private void SplitTextIntoWords()
        {
            if (_tmpText == null)
            {
                Debug.LogError("TMP_Text component is not assigned!");
                return;
            }
            
            TMP_TextInfo textInfo = _tmpText.textInfo;
            _tmpText.ForceMeshUpdate();
            
            GameObject container = new GameObject(_tmpText.text[0..(_tmpText.text.Length / 3)] + "_words");
            container.transform.SetParent(_tmpText.transform.parent);
            container.transform.localPosition = _tmpText.transform.localPosition;
            container.transform.localRotation = _tmpText.transform.localRotation;
            container.transform.localScale = _tmpText.transform.localScale;
            
            for (int i = 0; i < textInfo.wordCount; i++)
            {
                TMP_WordInfo wordInfo = textInfo.wordInfo[i];
                string wordText = _tmpText.text.Substring(wordInfo.firstCharacterIndex, wordInfo.characterCount);
                
                GameObject wordObj = Instantiate(_tmpText.gameObject, container.transform);
                wordObj.name = "Word_" + wordText;
                
                TMP_Text wordTMP = wordObj.GetComponent<TMP_Text>();
                if (wordTMP != null)
                {
                    wordTMP.text = wordText;
                }
                
                RectTransform wordTransform = wordObj.GetComponent<RectTransform>();
                if (wordTransform != null)
                {
                    Vector3 worldPosition = _tmpText.transform.TransformPoint(textInfo.characterInfo[wordInfo.firstCharacterIndex].bottomLeft);
                    wordTransform.position = worldPosition;
                }
            }
        }
    }
}