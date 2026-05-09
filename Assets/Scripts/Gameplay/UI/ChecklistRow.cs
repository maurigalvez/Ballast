using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ballast.Gameplay
{
    public class ChecklistRow : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private GameObject checkmark;

        public void Bind(string itemName, bool checkedState)
        {
            if (label != null) label.text = itemName;
            if (checkmark != null) checkmark.SetActive(checkedState);
        }
    }
}
