using System;
using System.Collections.Generic;
using DungeonAutomata._Project.Scripts._Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonAutomata._Project.Scripts.DialogueSystem
{
    public class BarkPanel : MonoBehaviour, IPanel
    {

        private Queue<string> _dialogueLines;
        [SerializeField] private TMP_Text _text;
        private bool _isOpen = false;
        public static event Action OnDialogueEnd;

        private void Awake()
        {
            _dialogueLines = new Queue<string>();
            _text = GetComponentInChildren<TMP_Text>();
        }

        public void OpenPanel(string dialogueString)
        {
            _text.text = dialogueString;
            _isOpen = true;
        }

        public void OpenPanel(DialogueData dialogueData)
        {
            foreach (var line in dialogueData.dialogue)
            {
                _dialogueLines.Enqueue(line);
            }

            if (_dialogueLines.Count > 0)
                _text.text = _dialogueLines.Dequeue();
            _isOpen = true;
        }

        public void ShowNextLine()
        {
            if (_dialogueLines.Count == 0)
            {
                _isOpen = false;
                OnDialogueEnd?.Invoke();
                return;
            }
            _text.text = _dialogueLines.Dequeue();
        }
    }
}