using UnityEngine;
using UnityEngine.UI;
using TMPro;
using App;

namespace App.UI {
    public enum ModalState { NONE, INVENTORY, OPTIONS /* If a modal is added, put a state for it here and assign its animator and transform in the inspector */}

    public class UIManager : MonoBehaviour
    {
        // Singleton instance (reference this class' members via UIManager.Instance from any context that is 'using App.UI;')
        public static UIManager Instance;

        [SerializeField] bool DrawDebugText;
        [SerializeField] string version;

        [Header("UI Transforms")]
        [SerializeField] RectTransform[] stateTransforms;
        [SerializeField] RectTransform[] modalTransforms;

        [SerializeField] RectTransform inventoryItemArea;
        [SerializeField] RectTransform inventoryEquippedArea;
        [SerializeField] RectTransform ingameEquipmentArea;

        [SerializeField] RectTransform activeToolBorder;

        [Header("UI Animators")]
        [SerializeField] Animator[] modalAnimators;

        [Header("Text")]
        [SerializeField] TextMeshProUGUI InventoryInfoboxFlavorText;
        [SerializeField] TextMeshProUGUI InventoryInfoboxNameText;
        [SerializeField] TextMeshProUGUI inventoryValueText;
        [SerializeField] TextMeshProUGUI debugText;

        [Header("Objects")]
        [SerializeField] RectTransform inventoryUIObject;
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

            for(int i = 0; i < modalTransforms.Length; i++) { modalAnimators[i].SetBool("Open", i == modal); }
        }

        /// <summary>
        /// Close an open modal, or open a closed modal.
        /// </summary>
        /// <param name="modal">Modal index to open or close.</param>
        public void ToggleModal(int modal) {
            bool opened = !modalAnimators[modal].GetBool("Open");

            DoModalSetup((ModalState)modal);

            modalAnimators[modal].SetBool("Open", opened);
        }

        void DoModalSetup(ModalState modal) {
            // Setup the modal before opening it.
            switch (modal) {
                case ModalState.INVENTORY:
                    LoadInventoryToInventoryModal();
                    ResetInventoryPanelText();
                    break;
            }
        }

        /// <summary>
        /// Close a modal by state. If none is given, close all open modals.
        /// </summary>
        /// <param name="modal">Modal to close.</param>
        public void CloseModal(int modal = 0) {
            if(modal == 0) {
                for (int i = 0; i < modalTransforms.Length; i++) { modalAnimators[i].SetBool("Open", false); }

                return;
            }
            
            modalAnimators[modal].SetBool("Open", false);
        }

        /// <summary>
        /// Swap the UI from one game state to another.
        /// </summary>
        /// <param name="state">Game state to which the UI should be set.</param>
        public void SwapState(GameState state) {
            for(int i = 0; i < stateTransforms.Length; i++) { if(stateTransforms[i]) stateTransforms[i].gameObject.SetActive((int)state == i); }
        }

        /// <summary>
        /// Load the item inventory from the inventory manager to the UI inventory panel.
        /// </summary>
        public void LoadInventoryToInventoryModal() {
            // Remove existing inventory items
            foreach (RectTransform r in inventoryItemArea.GetComponentsInChildren<RectTransform>()) { if(r != inventoryItemArea) Destroy(r.gameObject); }
            foreach (RectTransform r in inventoryEquippedArea.GetComponentsInChildren<RectTransform>()) { if (r != inventoryEquippedArea) Destroy(r.gameObject); }

            int it = 0;

            int numItemsPerRow = Mathf.FloorToInt(inventoryItemArea.rect.width / (inventoryUIObject.rect.width + 5));
            int numRows = Mathf.CeilToInt((float)InventoryManager.Instance.items.Count / numItemsPerRow);
            

            // Adjust the size of the scrollable area to accomodate the inventory UI items
            inventoryItemArea.offsetMax = new Vector2(0, inventoryItemArea.rect.yMin + (5 + (numRows * (inventoryUIObject.rect.height + 5))));

            // Inventory items
            for(int y = 0; y < numRows; y++) {
                for (int x = 0; x < numItemsPerRow; x++) {
                    if (it >= InventoryManager.Instance.items.Count) break;

                    RectTransform i = Instantiate(inventoryUIObject, inventoryItemArea.transform);

                    i.anchoredPosition = new Vector2(
                        5 + (x * (inventoryUIObject.rect.width + 5)) + inventoryUIObject.rect.width / 2,
                        5 - (y * (inventoryUIObject.rect.height + 5)) - inventoryUIObject.rect.height / 2
                    );

                    i.GetComponent<UIInventoryItem>().InitializeWithData(InventoryManager.Instance.items[it]);
                    it++;
                }

                if (it >= InventoryManager.Instance.items.Count) break;
            }

            // Equipped Items
            for(int i = 0; i < InventoryManager.Instance.equippedItems.Count; i++) {
                RectTransform t = Instantiate(inventoryUIObject, inventoryEquippedArea.transform);

                t.anchoredPosition = new Vector2(
                    5 + (i * (inventoryUIObject.rect.width + 5)) + inventoryUIObject.rect.width / 2,
                    inventoryEquippedArea.rect.y
                );

                t.GetComponent<UIInventoryItem>().InitializeWithData(InventoryManager.Instance.equippedItems[i]);
            }
        }
        
        /// <summary>
        /// Load the equipped tools from the inventory manager to the UI equipment bar.
        /// </summary>
        public void LoadInventoryToEquipmentBar() {
            // Remove existing equipment objects
            foreach (RectTransform r in ingameEquipmentArea.GetComponentsInChildren<RectTransform>()) { if (r != ingameEquipmentArea) Destroy(r.gameObject); }

            // Load the equipped items to the bar
            for (int i = 0; i < InventoryManager.Instance.equippedItems.Count; i++) {
                RectTransform t = Instantiate(equippedToolUIObject, ingameEquipmentArea.transform);

                t.anchoredPosition = new Vector2(
                    5 + (i * (equippedToolUIObject.rect.width + 5)) + equippedToolUIObject.rect.width / 2,
                    ingameEquipmentArea.rect.y
                );

                t.GetComponent<UIEquippedTool>().InitializeWithData(InventoryManager.Instance.equippedItems[i]);
            }

            SetActiveToolBorder(InventoryManager.Instance.ActiveTool);
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
            InventoryInfoboxNameText.text = item.ItemName;
            InventoryInfoboxFlavorText.text = item.ItemText;
        }

        /// <summary>
        /// Set the inventory panel text to the default.
        /// </summary>
        public void ResetInventoryPanelText() {
            InventoryInfoboxNameText.text = "???";
            InventoryInfoboxFlavorText.text = defaultInventoryPanelText;
        }

        public void WriteDebug(string text) {
            debugText.text = text;
        }
    }
}
