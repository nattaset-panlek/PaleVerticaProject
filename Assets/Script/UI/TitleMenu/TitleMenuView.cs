using CoreSystem.Data.Save;
using CoreSystem.UI.SystemMenu;
using CoreSystem.World;
using UnityEngine;
using UnityEngine.UI;

namespace CoreSystem.UI.TitleMenu
{
    public sealed class TitleMenuView : MonoBehaviour
    {
        #region [Variables/Fields]
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _exitButton;
        #endregion

        #region [Unity Methods]
        private void Awake()
        {
            _continueButton.onClick.AddListener(HandleContinue);
            _newGameButton.onClick.AddListener(GameCoreLoader.StartNewGame);
            _exitButton.onClick.AddListener(HandleExit);
        }

        private void Start()
        {
            _continueButton.interactable = SaveStorage.SlotExists(SystemMenuWindow.QuickSaveSlot);
        }

        private void OnDestroy()
        {
            _continueButton.onClick.RemoveListener(HandleContinue);
            _newGameButton.onClick.RemoveListener(GameCoreLoader.StartNewGame);
            _exitButton.onClick.RemoveListener(HandleExit);
        }
        #endregion

        #region [Private Methods]
        private static void HandleContinue()
        {
            GameCoreLoader.ContinueGame(SaveStorage.LoadSlot(SystemMenuWindow.QuickSaveSlot));
        }

        private static void HandleExit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        #endregion
    }
}
