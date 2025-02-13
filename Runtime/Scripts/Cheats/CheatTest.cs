using UnityEngine;

namespace HotChocolate.Cheats
{
    [CreateAssetMenu(fileName = "CheatTest", menuName = "HotChocolate/Cheats/Cheat Test", order = 1)]
    public class CheatTest : ScriptableObject
    {
        public float someFloat = 50f;
        [Range(0, 1)]
        public float someFloatWithRange = 0.5f;

        public int someInt = 100;
        public int someIndex = 0;

        [Range(1, 100)]
        public int someIntWithRange = 50;

        public string someString = "patate";

        public enum SomeEnumType
        {
            Value1,
            Value2,
            Value3
        }

        public SomeEnumType someEnum = SomeEnumType.Value1;

        public bool someToggle = false;

        [CheatProperty]
        public float SomeFloat
        {
            get { return someFloat; }
            set { someFloat = value; }
        }

        [CheatProperty(MinValue = 0, MaxValue = 1)]
        public float SomeFloatWithRange
        {
            get { return someFloatWithRange; }
            set { someFloatWithRange = value; }
        }

        [CheatProperty]
        public int SomeInt
        {
            get { return someInt; }
            set { someInt = value; }
        }

        [CheatProperty(MinValue = 1, MaxValue = 100)]
        public int SomeIntWithRange
        {
            get { return someIntWithRange; }
            set { someIntWithRange = value; }
        }

        [CheatProperty]
        public string SomeString
        {
            get { return someString; }
            set { someString = value; }
        }

        [CheatProperty]
        public SomeEnumType SomeEnum
        {
            get { return someEnum; }
            set { someEnum = value; }
        }

        [CheatProperty]
        public bool SomeToggle
        {
            get { return someToggle; }
            set { someToggle = value; }
        }

        [CheatMethod]
        public void SomeMethod()
        {
            Debug.Log("Invoking SomeMethod");
        }
    }
}