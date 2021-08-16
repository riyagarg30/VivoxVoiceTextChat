using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;


namespace VivoxUnity
{
    public static class ExtensionMethods
    {
        public static string Get_Selected(this TMP_Dropdown dropDown)
        {
            int index = dropDown.value;
            string result;
            if (index >= 0 && index < dropDown.options.Count)
            {
                result = dropDown.options[index].text;
                return result;
            }
            return null;
        }

        public static void Add_Value(this TMP_Dropdown dropDown, string valueToAdd)
        {
            dropDown.options.Add(new TMP_Dropdown.OptionData() { text = valueToAdd });
            dropDown.RefreshShownValue();
        }

        public static void Remove_Value(this TMP_Dropdown dropDown, string valueToRemove)
        {
            TMP_Dropdown.OptionData remove = dropDown.options.Find((x) => x.text == valueToRemove);
            if (dropDown.options.Contains(remove))
            {
                dropDown.options.Remove(remove);
                dropDown.RefreshShownValue();
            }
        }

    }
}