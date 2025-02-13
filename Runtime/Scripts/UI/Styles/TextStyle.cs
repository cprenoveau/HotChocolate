using System.Linq;
using TMPro;
using UnityEngine;

namespace HotChocolate.UI.Styles
{
    [RequireComponent(typeof(TMP_Text))]
    public class TextStyle : MonoBehaviour
    {
        public TextStyleConfig config;

        public bool ignoreFont;
        public bool ignoreFontStyle;
        public bool ignoreFontSizeMin;
        public bool ignoreFontSizeMax;
        public bool ignoreColor;
        public bool ignoreFormat;

        private void Start()
        {
            Refresh();
        }

        public void Refresh()
        {
            var text = GetComponent<TMP_Text>();

            if (!ignoreFont) text.font = config.font;
            if (!ignoreFontStyle)
            {
                text.fontStyle = 0;
                foreach (var style in config.fontStyles)
                {
                    text.fontStyle |= style;
                }
            }
            if (!ignoreFontSizeMin) text.fontSizeMin = config.fontSizeMin;
            if (!ignoreFontSizeMax) text.fontSize = text.fontSizeMax = config.fontSizeMax;
            if (!ignoreColor) text.color = config.color;

            if (!ignoreFormat)
            {
                switch (config.format)
                {
                    case TextStyleConfig.TextFormat.None:
                        break;

                    case TextStyleConfig.TextFormat.UpperCase:
                        text.text = text.text.ToUpper();
                        break;

                    case TextStyleConfig.TextFormat.LowerCase:
                        text.text = text.text.ToLower();
                        break;

                    case TextStyleConfig.TextFormat.FirstLetterUpperCase:
                        if (text.text.Length > 0)
                        {
                            text.text = text.text.ToLower();
                            text.text = text.text.First().ToString().ToUpper() + text.text.Substring(1);
                        }
                        break;
                }
            }
        }
    }
}
