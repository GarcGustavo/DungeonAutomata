using System;
using System.Collections.Generic;
using DungeonAutomata._Project.Scripts.DialogueSystem.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonAutomata._Project.Scripts.DialogueSystem
{
    public class DialoguePanel : MonoBehaviour, IPanel
    {
        private Queue<string> _dialogueLines;
        private Queue<Sprite> _dialogueSprites;
        [SerializeField] private Image _portrait;
        [SerializeField] private TMP_Text _text;
        private bool _isOpen = false;
        public static event Action OnDialogueEnd;

        private void Awake()
        {
            _dialogueLines = new Queue<string>();
            _dialogueSprites = new Queue<Sprite>();
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

            foreach (var portrait in dialogueData.sprites)
            {
                _dialogueSprites.Enqueue(portrait);
            }
        
            if(_dialogueLines.Count > 0)
                _text.text = _dialogueLines.Dequeue();
            if(_dialogueSprites.Count > 0)
                _portrait.sprite = _dialogueSprites.Dequeue();
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
            if(_dialogueSprites.Count > 0)
                _portrait.sprite = _dialogueSprites.Dequeue();
        }
    
    }
}
