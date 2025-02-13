using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HotChocolate.UI.Widgets
{
    public class Tab : MonoBehaviour
    {
        public List<Button> buttons = new List<Button>();
        public GameObject unselected;
        public GameObject selected;

        public delegate void Selected();
        public event Selected OnSelected;

        public bool IsOn
        {
            get { return _isOn; }
            set { _isOn = value; UpdateState(); }
        }

        private bool _isOn = false;

        private void Awake()
        {
            foreach(var button in buttons)
                button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            if(!IsOn)
            {
                IsOn = true;
                OnSelected?.Invoke();
            }
        }

        private void UpdateState()
        {
            if(selected != null)
                selected.SetActive(_isOn);

            if (unselected != null)
                unselected.SetActive(!_isOn);
        }
    }
}
