using System;
using DungeonAutomata._Project.Scripts._Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonAutomata._Project.Scripts.GridComponents
{
	public class ItemContainerUI : MonoBehaviour
	{
		[SerializeField] private Image itemIcon;
		private IUnit itemUnit;

		public void SetItem(IItem item, IUnit unit)
		{
			itemIcon.sprite = item.Icon;
			itemUnit = unit;
			itemIcon.SetNativeSize();
		}
	}
}