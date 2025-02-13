using UnityEngine;
using UnityEngine.UI;

namespace HotChocolate.UI.Cheats
{
    public class CheatSlider : MonoBehaviour
    {
        public Text label;
        public Slider slider;

        public delegate void ValueChanged(float value);
        public event ValueChanged OnValueChanged;

        public void Init(string name, float value, float minValue, float maxValue, bool wholeNumbers)
        {
            label.text = Utils.TextFormat.ToDisplayName(name);

            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.wholeNumbers = wholeNumbers;
            slider.value = value;

            slider.onValueChanged.AddListener((float val) => { OnValueChanged(val); });
        }
    }
}
