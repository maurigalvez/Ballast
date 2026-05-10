using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ballast.Gameplay
{
    public class ChecklistRow : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private Image iconImage;
        [SerializeField] private GameObject checkmark;

        public void Bind(ItemData data, bool checkedState)
        {
            if (data == null)
            {
                if (label != null) label.text = string.Empty;
                if (iconImage != null) iconImage.enabled = false;
                if (checkmark != null) checkmark.SetActive(false);
                return;
            }

            if (label != null) label.text = data.ItemName;
            if (iconImage != null)
            {
                iconImage.sprite = data.Icon;
                iconImage.enabled = data.Icon != null;
            }
            if (checkmark != null) checkmark.SetActive(checkedState);
        }
    }
}
