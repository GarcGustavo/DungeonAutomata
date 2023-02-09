using System;
using System.Collections.Generic;
using DungeonAutomata._Project.Scripts._Managers;
using DungeonAutomata._Project.Scripts.DialogueSystem.Interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace DungeonAutomata._Project.Scripts.DialogueSystem
{
	public class DialogueActor : MonoBehaviour, IActor
	{
		//Holds dialogue text object/data and is responsible for triggering dialogue events
		[SerializeField] private DialogueData[] _dialogueData;
		[SerializeField] private DialogueData _barkData;
		[SerializeField] private DialogueData _activeDialogue;
		private Queue<DialogueData> _dialogueQueue;
		private DialogueManager _dialogueManager;
		public static event Action<DialogueData> OnDialogueStart;
		public static event Action<DialogueData> OnBarkStart;

		private void Awake()
		{
			_dialogueManager = DialogueManager.Instance;
			_dialogueQueue = new Queue<DialogueData>();
			foreach (var data in _dialogueData)
			{
				_dialogueQueue.Enqueue(data);
			}
		}
		
		public void TriggerDialogue()
		{
			if (_dialogueQueue.Count > 0)
			{
				_activeDialogue = _dialogueQueue.Dequeue();
			}
			OnDialogueStart?.Invoke(_activeDialogue);
		}

		public void TriggerDialogueBark()
		{
			OnBarkStart?.Invoke(_barkData);
		}
		
		public void SetNextDialogue()
		{
			if (_dialogueQueue.Count == 0)
			{
				_dialogueQueue.Enqueue(_activeDialogue);
				return;
			}
			TriggerDialogue();
		}
		
	}
}