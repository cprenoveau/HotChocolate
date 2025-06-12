using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HotChocolate.UI.Widgets
{
    public class TabContainer : MonoBehaviour
    {
        public delegate void TabSelected(int index);
        public event TabSelected OnTabSelected;

        [Tooltip("The tab to be selected by default")]
        public int initialIndex = 0;

        [Tooltip("Add all tabs found in children at awake time")]
        public bool initOnAwake = true;

        public int LastTabIndex { get; private set; } = -1;
        public int CurrentTabIndex { get; private set; } = -1;
        public Tab CurrentTab { get { return _tabs[CurrentTabIndex]; } }
        public int TabCount { get { return _tabs.Count; } }

        private List<Tab> _tabs = new List<Tab>();

        private void Awake()
        {
            if (initOnAwake)
                Init();
        }

        public void Init()
        {
            LastTabIndex = CurrentTabIndex = initialIndex;

            _tabs = transform.GetComponentsInChildren<Tab>(true).ToList();

            for (int i = 0; i < _tabs.Count; ++i)
            {
                int index = i;
                _tabs[i].IsOn = index == CurrentTabIndex;
                _tabs[i].OnSelected += () => { TabChanged(index); };
            }
        }

        public void AddTab(Tab tab)
        {
            _tabs.Add(tab);

            int index = _tabs.Count - 1;
            tab.IsOn = index == CurrentTabIndex;

            tab.OnSelected += () => { TabChanged(index); };
        }

        public Tab GetTab(int index)
        {
            return (index >= 0 && index < _tabs.Count) ? _tabs[index] : null;
        }

        public void SelectTab(int index)
        {
            if (index != CurrentTabIndex)
            {
                if (CurrentTabIndex >= 0 && CurrentTabIndex < _tabs.Count)
                {
                    _tabs[CurrentTabIndex].IsOn = false;
                }

                LastTabIndex = CurrentTabIndex;
                CurrentTabIndex = index;

                if (CurrentTabIndex >= 0 && CurrentTabIndex < _tabs.Count)
                {
                    _tabs[CurrentTabIndex].IsOn = true;
                }
            }
        }

        private void TabChanged(int index)
        {
            if (index != CurrentTabIndex)
            {
                _tabs[CurrentTabIndex].IsOn = false;

                LastTabIndex = CurrentTabIndex;
                CurrentTabIndex = index;

                _tabs[CurrentTabIndex].IsOn = true;

                OnTabSelected?.Invoke(index);
            }
        }
    }
}