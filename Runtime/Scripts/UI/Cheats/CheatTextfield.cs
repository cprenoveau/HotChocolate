using UnityEngine;
using UnityEngine.UI;

namespace HotChocolate.UI.Cheats
{
    public class CheatTextfield : MonoBehaviour
    {
        public Text label;
        public InputField value;

        public delegate void ValueChanged(string value);
        public event ValueChanged OnValueChanged;

        public void Init(string name, string value, InputField.CharacterValidation validation)
        {
            label.text = Utils.TextFormat.ToDisplayName(name);

            this.value.text = value;
            this.value.characterValidation = validation;

            this.value.onEndEdit.AddListener((string val) => { OnValueChanged(val); });
        }
    }
}