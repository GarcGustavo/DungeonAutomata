using System;
using UnityEngine;

namespace DungeonAutomata._Project.Scripts.DialogueSystem
{
	public class DialogueManager : MonoBehaviour
	{
		public static DialogueManager Instance { get; private set; }
		private DialoguePanel _dialoguePanel;

		private void Awake()
		{
			//Singleton initialization
			if (Instance == null)
			{
				Instance = this;
				DontDestroyOnLoad(gameObject);
			}
			else
			{
				Destroy(gameObject);
			}
			
			_dialoguePanel = GetComponentInChildren<DialoguePanel>();
			_dialoguePanel.gameObject.SetActive(false);
			DialogueActor.OnDialogueStart += OpenDialoguePanel;
			OnDialogueInput += ShowNextLine;
			DialoguePanel.OnDialogueEnd += CloseDialoguePanel;
		}
		
		private void OnDestroy()
		{
			DialogueActor.OnDialogueStart -= OpenDialoguePanel;
			OnDialogueInput -= ShowNextLine;
			DialoguePanel.OnDialogueEnd -= CloseDialoguePanel;
		}
		
		public static event Action OnDialogueInput;
		public void TriggerNextLine()
		{
			OnDialogueInput?.Invoke();
		}
		
		private void OpenDialoguePanel(DialogueData dialogueData)
		{
			if(_dialoguePanel.gameObject.activeInHierarchy)
				return;
			_dialoguePanel.gameObject.SetActive(true);
			_dialoguePanel.OpenPanel(dialogueData);
		}
		
		private void CloseDialoguePanel()
		{
			_dialoguePanel.gameObject.SetActive(false);
		}
		
		private void ShowNextLine()
		{
			if (_dialoguePanel.gameObject.activeInHierarchy)
				_dialoguePanel.ShowNextLine();
		}
		
	}
}