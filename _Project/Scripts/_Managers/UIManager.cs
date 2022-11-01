using DungeonAutomata._Project.Scripts._Interfaces;
using DungeonAutomata._Project.Scripts.GridComponents;
using DungeonAutomata._Project.Scripts.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonAutomata._Project.Scripts._Managers
{
	public class UIManager : MonoBehaviour
	{
		[SerializeField] private Transform HealthUI;
		[SerializeField] private Image HealthContainer;
		[SerializeField] private Transform EnergyUI;
		[SerializeField] private Image EnergyContainer;
		[SerializeField] private TMP_Text _turnText;
		[SerializeField] private TMP_Text _energyText;
		[SerializeField] private TMP_Text _inspectText;
		[SerializeField] private TMP_Text _unitInfoText;
		[SerializeField] private GameObject _infoPanel;
		[SerializeField] private RectTransform inventoryPanel;
		[SerializeField] private ItemContainerUI inventoryItem;
		private EventManager _eventManager;
		private GameManager _gameManager;
		private InventoryManager _inventoryManager;
		private PlayerUnit _player;
		private int _turnCount;
		public static UIManager Instance { get; private set; }

		private void Awake()
		{
			//Singleton initialization
			if (Instance == null)
				Instance = this;
			else
				Destroy(gameObject);
		}

		// Start is called before the first frame update
		private void Start()
		{
			_eventManager = EventManager.Instance;
			_gameManager = GameManager.Instance;
			_inventoryManager = InventoryManager.Instance;
			_eventManager.OnUpdateInventory += UpdateInventory;
			//_player = _gameManager.GetPlayer();
		}

		// Update is called once per frame
		private void Update()
		{
			/*
			if (_player != null)
			{
				_player.GetPlayerInput();
				_energyText.text = "Energy: " + _player.CurrentEnergy;
			}
			*/
		}

		private void UpdateTurn()
		{
			//_energyText.text = "Energy: " + _player.CurrentEnergy;
			/*
			if (_player.CurrentEnergy <= 0)
			{
				_turnCount++;
				_turnText.text = "Turn: " + _turnCount;
			}
			*/
		}

		public void SetUnitInfo(IUnit target)
		{
			if(target != null)
			{
				_infoPanel.SetActive(true);
				if (target.GetType() == typeof(EnemyUnit)
				    || target.GetType() == typeof(PlayerUnit))
				{
					var combatUnit = (ICombatUnit)target;
					_unitInfoText.text = target.UnitName + "\n"
					                                     + "Health: "
					                                     + combatUnit.CurrentHP + "/"
					                                     + combatUnit.MaxHP + "\n";
				}
				else
				{
					_unitInfoText.text = target.UnitName + "\n";
				}
			}
			else
			{
				_infoPanel.SetActive(false);
				_unitInfoText.text = "";
			}
		}

		public void SetHoverText(string text)
		{
			_inspectText.text = text;
		}

		private void UpdateInventory()
		{
			var items = _inventoryManager.Items;
			foreach (Transform child in inventoryPanel)
			{
				Destroy(child.gameObject);
			}

			foreach (IItem item in items)
			{
				var itemContainer = Instantiate(inventoryItem, inventoryPanel);
				itemContainer.SetItem(item);
				// itemIcon.GetComponent<Button>().onClick.AddListener(() =>
				// {
				// 	_eventManager.InvokeItemSelect(item);
				// });
			}
		}
	}
}