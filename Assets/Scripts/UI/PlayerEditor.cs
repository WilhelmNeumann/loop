using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PlayerEditor : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _playerNameInput;
        [SerializeField] private Image _colorPreview;
        private Player Player { get; set; }

        public void Initialize(Player player)
        {
            Player = player;
            _colorPreview.color = Player.Color;
        }

        private IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();
            _playerNameInput.text = Player.Name;
            _playerNameInput.onValueChanged.AddListener((input) => Player.Name = input);
            yield return null;
        }
    }
}