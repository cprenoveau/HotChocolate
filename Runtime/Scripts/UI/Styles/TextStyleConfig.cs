using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace HotChocolate.UI.Styles
{
    [CreateAssetMenu(fileName = "TextStyle", menuName = "HotChocolate/UI/Styles/Text Style", order = 1)]
    public class TextStyleConfig : ScriptableObject
    {
        public TMP_FontAsset font;
        public List<FontStyles> fontStyles = new List<FontStyles>();
        public float fontSizeMin = 24;
        public float fontSizeMax = 24;
        public Color color = Color.white;

        public enum TextFormat
        {
            None,
            UpperCase,
            LowerCase,
            FirstLetterUpperCase
        }

        public TextFormat format;
    }
}
