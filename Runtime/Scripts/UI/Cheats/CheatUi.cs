using HotChocolate.Cheats;
using HotChocolate.UI.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HotChocolate.UI.Cheats
{
    public interface ICheatUiRefresher
    {
        event Action<ScriptableObject> NeedsRefresh;
    }

    public class CheatUi : MonoBehaviour
    {
        public Canvas canvas;
        public CheatTab tabPrefab;
        public CheatSlider sliderPrefab;
        public CheatTextfield textfieldPrefab;
        public CheatComboBox comboBoxPrefab;
        public CheatButton buttonPrefab;
        public CheatToggle togglePrefab;
        public TabContainer tabsContainer;
        public RectTransform cheatContent;
        public GridLayoutGroup gridLayout;
        public Button closeButton;
        public List<ScriptableObject> cheats = new List<ScriptableObject>();

        public event Action OnOpen;
        public event Action OnClose;

        public bool IsOpened => canvas.gameObject.activeSelf;

        private Vector2 defaultCellSize;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            defaultCellSize = gridLayout.cellSize;

            foreach (Transform child in cheatContent)
            {
                Destroy(child.gameObject);
            }

            closeButton.onClick.AddListener(Close);
            Close();
        }

        public void Init()
        {
            tabsContainer.Init();
            tabsContainer.OnTabSelected += TabSelected;

            for (int i = 0; i < cheats.Count; ++i)
            {
                var tab = Instantiate(tabPrefab, tabsContainer.transform);

                tab.Init(i, cheats[i].name, i == 0);
                tabsContainer.AddTab(tab.tab);

                if (cheats[i] is ICheatUiRefresher uiRefresher)
                {
                    uiRefresher.NeedsRefresh -= Refresh;
                    uiRefresher.NeedsRefresh += Refresh;
                }
            }

            InitShortcutKeys();
            TabSelected(0);
        }

        private void OnDestroy()
        {
            tabsContainer.OnTabSelected -= TabSelected;

            for (int i = 0; i < cheats.Count; ++i)
            {
                if (cheats[i] is ICheatUiRefresher uiRefresher)
                {
                    uiRefresher.NeedsRefresh -= Refresh;
                }
            }
        }

        private List<InputBinding> shortcutKeys = new List<InputBinding>();

        private class InputBinding
        {
            public KeyCode key;
            public Action method;
        }

        private void InitShortcutKeys()
        {
            foreach (var cheat in cheats)
            {
                foreach (var method in cheat.GetType().GetMethods())
                {
                    CheatMethod methodAttr = (CheatMethod)method.GetCustomAttributes(typeof(CheatMethod), false).FirstOrDefault();

                    if (methodAttr != null && methodAttr.ShortcutKeys != null)
                    {
                        foreach (var key in methodAttr.ShortcutKeys)
                        {
                            var binding = new InputBinding
                            {
                                key = key,
                                method = () => { method.Invoke(cheat, null); }
                            };

                            shortcutKeys.Add(binding);
                        }
                    }
                }

                foreach(var prop in cheat.GetType().GetProperties())
                {
                    if (prop.PropertyType != typeof(bool))
                        continue;

                    CheatProperty propertyAttr = (CheatProperty)prop.GetCustomAttributes(typeof(CheatProperty), false).FirstOrDefault();
                    if (propertyAttr != null && propertyAttr.ShortcutKeys != null)
                    {
                        foreach (var key in propertyAttr.ShortcutKeys)
                        {
                            var binding = new InputBinding
                            {
                                key = key,
                                method = () => { prop.SetValue(cheat, !(bool)prop.GetValue(cheat)); Refresh(cheat); }
                            };

                            shortcutKeys.Add(binding);
                        }
                    }
                }
            }
        }

        private void Update()
        {
            foreach(var shortcut in shortcutKeys)
            {
                if(Input.GetKeyDown(shortcut.key))
                {
                    shortcut.method();
                }
            }
        }

        private void Refresh(ScriptableObject cheat)
        {
            if (cheats.IndexOf(cheat) == tabsContainer.CurrentTabIndex)
            {
                TabSelected(tabsContainer.CurrentTabIndex);
            }
        }

        public void Toggle()
        {
            if (IsOpened) Close();
            else Open();
        }

        private GameObject lastSelected;
        public void Open()
        {
            if (!IsOpened)
            {
                if(cheats.Count > tabsContainer.CurrentTabIndex)
                    Refresh(cheats[tabsContainer.CurrentTabIndex]);

                canvas.gameObject.SetActive(true);
                OnOpen?.Invoke();

                var currentTab = tabsContainer.CurrentTab;
                if (currentTab != null)
                {
                    lastSelected = EventSystem.current.currentSelectedGameObject;
                    EventSystem.current.SetSelectedGameObject(currentTab.buttons[0].gameObject);
                }
            }
        }

        public void Close()
        {
            if (IsOpened)
            {
                EventSystem.current.SetSelectedGameObject(lastSelected);

                canvas.gameObject.SetActive(false);
                OnClose?.Invoke();
            }
        }

        private void TabSelected(int index)
        {
            foreach (Transform child in cheatContent)
            {
                Destroy(child.gameObject);
            }

            var cheatScriptableObject = cheats[index];
            gridLayout.cellSize = defaultCellSize;

            var cheatsMemberInfos = new List<(MemberInfo info, string order)>();
            void AddCheat<TInfo, TCheat>(TInfo property)
                where TInfo : MemberInfo
                where TCheat : CheatAttribute
            {
                var attribute = (TCheat)property.GetCustomAttributes(typeof(TCheat), false).FirstOrDefault();
                if (attribute != null && attribute.IsValid())
                    cheatsMemberInfos.Add((property, attribute.DeclarationOrder));
            }

            // Retrieve all cheats
            foreach (var property in cheatScriptableObject.GetType().GetProperties())
                AddCheat<MemberInfo, CheatProperty>(property);
            foreach (var method in cheatScriptableObject.GetType().GetMethods())
                AddCheat<MethodInfo, CheatMethod>(method);

            // Instantiate cheats in declaration order
            foreach (var (info, order) in cheatsMemberInfos.OrderBy(x => x.order))
            {
                _Instantiate(info);
            }

            void _Instantiate(MemberInfo info)
            {
                if (info is PropertyInfo propertyInfo) _InstantiateProperty(propertyInfo);
                else if (info is MethodInfo methodInfo) _InstantiateMethod(methodInfo);
            }

            void _InstantiateProperty(PropertyInfo property)
            {
                CheatProperty attribute = (CheatProperty)property.GetCustomAttributes(typeof(CheatProperty), false).FirstOrDefault();

                if(!string.IsNullOrEmpty(attribute.CustomWidgetAddress))
                {
                    var obj = InstantiateCustomWidget(attribute.CustomWidgetAddress);
                    if(obj != null && obj.GetComponent<ICustomWidget>() != null)
                    {
                        gridLayout.cellSize = obj.GetComponent<RectTransform>().sizeDelta;

                        obj.GetComponent<ICustomWidget>().Init(property.Name, property.GetValue(cheatScriptableObject));
                        obj.GetComponent<ICustomWidget>().OnValueChanged += (object value) => { property.SetValue(cheatScriptableObject, value); };
                    }
                }
                else if (property.PropertyType == typeof(bool))
                {
                    var toggle = Instantiate(togglePrefab, cheatContent);
                    toggle.Init(property.Name, attribute.ShortcutKeys, (bool)property.GetValue(cheatScriptableObject));
                    toggle.OnValueChanged += (bool value) => { property.SetValue(cheatScriptableObject, value); };
                }
                else if(property.PropertyType.IsEnum)
                {
                    var comboBox = Instantiate(comboBoxPrefab, cheatContent);
                    comboBox.Init(property.Name, (int)property.GetValue(cheatScriptableObject), Enum.GetNames(property.PropertyType));
                    comboBox.OnValueChanged += (int value) => { property.SetValue(cheatScriptableObject, value) ; };
                }
                else if (property.PropertyType == typeof(float) || property.PropertyType == typeof(int))
                {
                    if(!string.IsNullOrEmpty(attribute.StringArrayProperty))
                    {
                        var stringArrayProperty = cheatScriptableObject.GetType().GetProperty(attribute.StringArrayProperty);

                        var comboBox = Instantiate(comboBoxPrefab, cheatContent);
                        comboBox.Init(property.Name, (int)property.GetValue(cheatScriptableObject), (string[])stringArrayProperty.GetValue(cheatScriptableObject));
                        comboBox.OnValueChanged += (int value) => { property.SetValue(cheatScriptableObject, value); };
                    }
                    else if (attribute.MinValue != -1 && attribute.MaxValue != -1)
                    {
                        var slider = Instantiate(sliderPrefab, cheatContent);

                        if (property.PropertyType == typeof(int))
                        {
                            slider.Init(property.Name, (int)property.GetValue(cheatScriptableObject), attribute.MinValue, attribute.MaxValue, true);
                            slider.OnValueChanged += (float value) => { property.SetValue(cheatScriptableObject, Convert.ChangeType(value, property.PropertyType)); };
                        }
                        else
                        {
                            slider.Init(property.Name, (float)property.GetValue(cheatScriptableObject), attribute.MinValue, attribute.MaxValue, false);
                            slider.OnValueChanged += (float value) => { property.SetValue(cheatScriptableObject, value); };
                        }
                    }
                    else
                    {
                        var textfield = Instantiate(textfieldPrefab, cheatContent);

                        if (property.PropertyType == typeof(int))
                        {
                            textfield.Init(property.Name, ((int)property.GetValue(cheatScriptableObject)).ToString(), InputField.CharacterValidation.Integer);
                            textfield.OnValueChanged += (string value) => { property.SetValue(cheatScriptableObject, int.Parse(value)); };
                        }
                        else
                        {
                            textfield.Init(property.Name, ((float)property.GetValue(cheatScriptableObject)).ToString(), InputField.CharacterValidation.Decimal);
                            textfield.OnValueChanged += (string value) => { property.SetValue(cheatScriptableObject, float.Parse(value)); };
                        }
                    }
                }
                else if (property.PropertyType == typeof(string))
                {
                    var textfield = Instantiate(textfieldPrefab, cheatContent);
                    textfield.Init(property.Name, (string)property.GetValue(cheatScriptableObject), InputField.CharacterValidation.None);
                    textfield.OnValueChanged += (string value) => { property.SetValue(cheatScriptableObject, value); };
                }
            }

            void _InstantiateMethod(MethodInfo method)
            {
                CheatMethod attribute = (CheatMethod)method.GetCustomAttributes(typeof(CheatMethod), false).FirstOrDefault();

                var button = Instantiate(buttonPrefab, cheatContent);
                button.Init(method.Name, attribute.ShortcutKeys);
                button.OnButtonClicked += () => { method.Invoke(cheatScriptableObject, null); };
            }
        }

        private GameObject InstantiateCustomWidget(string address)
        {
            try
            {
                var op = Addressables.InstantiateAsync(address, cheatContent);
                op.WaitForCompletion();
                return op.Result;
            }
            catch (Exception e)
            {
                Debug.LogError("Could not instantiate " + address + " Exception: " + e.Message);
            }

            return null;
        }
    }
}