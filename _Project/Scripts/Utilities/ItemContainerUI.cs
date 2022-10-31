using DungeonAutomata._Project.Scripts._Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonAutomata._Project.Scripts.Utilities
{
	public class ItemContainerUI : MonoBehaviour
	{
		[SerializeField] private Image itemIcon;

		public void SetItem(IItem item)
		{
			itemIcon.sprite = item.Icon;
			itemIcon.SetNativeSize();
		}
	}
}