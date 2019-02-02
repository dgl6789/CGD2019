using UnityEngine;
using UnityEngine.UI;
using TMPro;
using App;

namespace App.UI {
    public enum ModalState { NONE, INVENTORY, SHOP, OPTIONS /* If a modal is added, put a state for it here and assign its animator and transform in the inspector */}

    public class UIManager : MonoBehaviour
    {
        // Singleton instance (reference this class' members via UIManager.Instance from any context that is 'using App.UI;')
        public static UIManager Instance;

        [SerializeField] bool DrawDebugText;
        [SerializeField] string version;

        [Header("UI Transforms")]
        [SerializeField] RectTransform[] stateTransforms;
        [SerializeField] RectTransform[] modalTransforms;

        [SerializeField] RectTransform playerInventoryItemArea;
        [SerializeField] RectTransform shopInventoryItemArea;

        [SerializeField] RectTransform inventoryEquippedArea;
        [SerializeField] RectTransform ingameEquipmentArea;

        [SerializeField] RectTransform activeToolBorder;

        [Header("UI Animators")]
        [SerializeField] Animator[] modalAnimators;

        [Header("Text")]
        [SerializeField] TextMeshProUGUI InventoryInfoboxFlavorText;
        [SerializeField] TextMeshProUGUI InventoryInfoboxNameText;
        [SerializeField] TextMeshProUGUI shopCurrencyText;
        [SerializeField] TextMeshProUGUI debugText;

        [Header("Sprites")]
        [SerializeField] Sprite inventoryOpenImage;
        [SerializeField] Sprite inventoryClosedImage;

        [Header("Images")]
        [SerializeField] Image inventoryImage;

        [Header("Buttons")]
        public Button newRockButton;

        [Header("Objects")]
        [SerializeField] RectTransform inventoryUIObject;
        [SerializeField] RectTransform shopUIObject;
        [SerializeField] RectTransform equippedToolUIObject;

        [Header("Miscellaneous")]
        [SerializeField] string defaultInventoryPanelText;

        // Use this for initialization
        void Awake()
        {
            // Singleton intitialization.
            if (Instance == null) Instance = this;
            else Destroy(this);
        }

        private void Start() {
            debugText.gameObject.SetActive(DrawDebugText);

            WriteDebug("Version: " + version);
        }

        /// <summary>
        /// Open a modal by ModalState.
        /// </summary>
        /// <param name="modal">Modal to open.</param>
        public void OpenModal(ModalState modal) {
            OpenModal((int)modal);
        }

        /// <summary>
        /// Open a modal by state, closing all others.
        /// </summary>
        /// <param name="modal">Modal state to open.</param>
        public void OpenModal(int modal) {
            DoModalSetup((ModalState)modal);

            modalAnimators[modal].SetBool("Open", true);
        }

        /// <summary>
        /// Close an open modal, or open a closed modal.
        /// </summary>
        /// <param name="modal">Modal index to open or close.</param>
        public void ToggleModal(int modal) {
            bool opened = !modalAnimators[modal].GetBool("Open");

            if (opened) OpenModal(modal);
            else CloseModal(modal);

            modalAnimators[modal].SetBool("Open", opened);
        }

        void DoModalSetup(ModalState modal) {
            // Setup the modal before opening it.
            switch (modal) {
                case ModalState.INVENTORY:
                    SetInventoryButtonImage(true);
                    LoadInventoryToInventoryModal(InventoryType.PLAYER);
                    ResetInventoryPanelText();
                    break;
                case ModalState.SHOP:
                    LoadInventoryToInventoryModal(InventoryType.SHOP);
                    if (!ModalIsOpen(ModalState.INVENTORY)) OpenModal(ModalState.INVENTORY);
                    break;
            }
        }

        void DoModalTeardown(ModalState modal) {
            // Clean up the modal before closing it.
            switch (modal) {
                case ModalState.INVENTORY:
                    SetInventoryButtonImage(false);
                    break;
                case ModalState.SHOP:
                    if (ModalIsOpen(ModalState.INVENTORY)) CloseModal((int)ModalState.INVENTORY);
                    break;
            }
        }

        /// <summary>
        /// Close a modal by state. If none is given, close all open modals.
        /// </summary>
        /// <param name="modal">Modal to close.</param>
        public void CloseModal(int modal = 0) {
            if(modal == 0) {
                for (int i = 0; i < modalTransforms.Length; i++) {
                    modalAnimators[i].SetBool("Open", false);
                    DoModalTeardown((ModalState)i);
                }

                return;
            }
            
            modalAnimators[modal].SetBool("Open", false);
            DoModalTeardown((ModalState)modal);
        }

        public bool ModalIsOpen(ModalState modal) {
            return modalAnimators[(int)modal].GetBool("Open");
        }

        /// <summary>
        /// Swap the UI from one game state to another.
        /// </summary>
        /// <param name="state">Game state to which the UI should be set.</param>
        public void SwapState(GameState state) {
            for(int i = 0; i < stateTransforms.Length; i++) { if(stateTransforms[i]) stateTransforms[i].gameObject.SetActive((int)state == i); }
        }

        /// <summary>
        /// Load the item inventory from the inventory manager to a UI inventory panel.
        /// </summary>
        public void LoadInventoryToInventoryModal(InventoryType inventory) {
            RectTransform itemArea = inventory.Equals(InventoryType.PLAYER) ? playerInventoryItemArea : shopInventoryItemArea;
            int itemCount = inventory.Equals(InventoryType.PLAYER) ? InventoryManager.Instance.playerItems.Count : InventoryManager.Instance.shopItems.Count;
            RectTransform uiItemObject = inventory.Equals(InventoryType.PLAYER) ? inventoryUIObject : shopUIObject;

            // Remove existing inventory items
            foreach (RectTransform r in itemArea.GetComponentsInChildren<RectTransform>()) { if(r != itemArea) Destroy(r.gameObject); }


            if (inventory.Equals(InventoryType.PLAYER)) {
                foreach (InventoryItem item in InventoryManager.Instance.playerItems) {
                    RectTransform i = Instantiate(uiItemObject, itemArea.transform);

                    i.GetComponent<UIInventoryItem>().InitializeWithData(item);
                }

            } else {
                foreach (InventoryItem item in InventoryManager.Instance.shopItems) {
                    RectTransform i = Instantiate(uiItemObject, itemArea.transform);

                    i.GetComponent<UIInventoryItem>().InitializeWithData(item);
                }
            }

            // Equipped Items (if necessary)
            if (inventory.Equals(InventoryType.PLAYER)) {
                foreach (RectTransform r in inventoryEquippedArea.GetComponentsInChildren<RectTransform>()) { if (r != inventoryEquippedArea) Destroy(r.gameObject); }

                // Load the equipped items to the bar
                foreach (ToolItem i in InventoryManager.Instance.equippedItems) {
                    RectTransform t = Instantiate(equippedToolUIObject, inventoryEquippedArea.transform);
                    t.GetComponent<UIEquippedTool>().InitializeWithData(i);
                }
            }
        }
        
        /// <summary>
        /// Load the equipped tools from the inventory manager to the UI equipment bar.
        /// </summary>
        public void LoadInventoryToEquipmentBar() {
            // Remove existing equipment objects
            foreach (RectTransform r in ingameEquipmentArea.GetComponentsInChildren<RectTransform>()) { if (r != ingameEquipmentArea) Destroy(r.gameObject); }

            // Load the equipped items to the bar
            foreach (ToolItem i in InventoryManager.Instance.equippedItems) {
                RectTransform t = Instantiate(equippedToolUIObject, ingameEquipmentArea.transform);
                t.GetComponent<UIEquippedTool>().InitializeWithData(i);
            }

            SetActiveToolBorder(InventoryManager.Instance.ActiveTool);
        }

        /// <summary>
        /// Indicate to all active shop ui items (and inventory ui items if both the shop and inventory modals are open).
        /// </summary>
        /// <param name="item">Item that was tapped.</param>
        public void OnItemTapped(UIInventoryItem item) {

            foreach(RectTransform r in shopInventoryItemArea.GetComponentsInChildren<RectTransform>()) {
                UIInventoryItem i = r.GetComponent<UIInventoryItem>();

                if(r != shopInventoryItemArea && i != null && i != item) {
                    i.UntapItem();
                }
            }

            foreach (RectTransform r in playerInventoryItemArea.GetComponentsInChildren<RectTransform>()) {
                UIInventoryItem i = r.GetComponent<UIInventoryItem>();

                if (r != playerInventoryItemArea && i != null && i != item) {
                    i.UntapItem();
                }
            }
            
            if(ModalIsOpen(ModalState.SHOP)) InventoryInfoboxFlavorText.text += " Tap again to " + (InventoryManager.Instance.playerItems.Contains(item.Item) ? "sell." : "buy.");
        }

        /// <summary>
        /// Set the border around the active tool to indicate that it is active.
        /// </summary>
        /// <param name="item">The equipped item that should be indicated as active.</param>
        public void SetActiveToolBorder(ToolItem item) {
            
            bool foundActiveItem = false;

            // Find the tool item that should be highlighted.
            foreach (RectTransform r in ingameEquipmentArea.GetComponentsInChildren<RectTransform>()) {
                if(r.GetComponent<UIEquippedTool>() && r.GetComponent<UIEquippedTool>().Item == item) {
                    activeToolBorder.position = r.position;

                    foundActiveItem = true;
                    break;
                }
            }

            // Set the cursor inactive if no tool item is selected.
            activeToolBorder.gameObject.SetActive(foundActiveItem);
        }

        /// <summary>
        /// Set the text shown in the inventory text box.
        /// </summary>
        /// <param name="item">Item to show text for.</param>
        public void SetInventoryPanelText(InventoryItem item) {
            

            InventoryInfoboxFlavorText.text = item.ItemText;
            if (item is MineralItem) {
                InventoryInfoboxNameText.text = item.ItemName + string.Format(" (${0:n0})", item.Value);
            } else {
                InventoryInfoboxNameText.text = item.ItemName;
            }
        }

        /// <summary>
        /// Set the inventory panel text to the default.
        /// </summary>
        public void ResetInventoryPanelText() {
            InventoryInfoboxNameText.text = "";
            InventoryInfoboxFlavorText.text = defaultInventoryPanelText;
        }

        public void SetCurrencyText() {
            shopCurrencyText.text = string.Format("${0:n0}", InventoryManager.Instance.PlayerCurrency);
        }

        public void WriteDebug(string text) {
            debugText.text = text;
        }

        public void SetButtonEnabled(Button button, bool enabled = true) { button.interactable = enabled; }

        public void SetInventoryButtonImage(bool open) { inventoryImage.sprite = open ? inventoryOpenImage : inventoryClosedImage; }
    }
}
