using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HotChocolate.UI.Cheats
{
    public class CheatComboBox : MonoBehaviour
    {
        public Text label;
        public Dropdown dropdown;

        public delegate void ValueChanged(int value);
        public event ValueChanged OnValueChanged;

        private string[] _values;
        private int _index;

        public void Init(string name, int index, string[] values)
        {
            label.text = Utils.TextFormat.ToDisplayName(name);

            _values = values;
            _index = index;

            dropdown.options = new List<Dropdown.OptionData>();
            for(int i = 0; i < values.Length; ++i)
            {
                dropdown.options.Add(new Dropdown.OptionData(Utils.TextFormat.ToDisplayName(values[i])));
            }

            dropdown.onValueChanged.AddListener(UpdateValue);
            dropdown.value = _index;
        }

        private void UpdateValue(int index)
        {
            _index = index;
            OnValueChanged?.Invoke(_index);
        }
    }
}