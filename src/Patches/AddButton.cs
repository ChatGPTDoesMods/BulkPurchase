using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BuyAllButton.Patches
{
    [HarmonyPatch(typeof(PlayerNetwork), nameof(PlayerNetwork.OnStartClient))]
    public class AddButton
    {
        public static bool Prefix()
        {
            // Find the Buttons_Bar GameObject
            GameObject buttonsBar = GameObject.Find("Buttons_Bar");

            if (buttonsBar == null)
            {
                return true;
            }

            // Create the "Add All to Cart" button if it doesn't exist
            if (buttonsBar.transform.Find("AddAllToCartButton") == null)
            {
                GameObject addAllButton = CreateButton(buttonsBar, "AddAllToCartButton", -450, 110); // Full width
                AddButtonEvents(addAllButton.GetComponent<Button>(), addAllButton.GetComponent<Image>(), OnAddAllToCartButtonClick);
            }

            // Create the "Remove All from Cart" button if it doesn't exist
            if (buttonsBar.transform.Find("RemoveAllFromCartButton") == null)
            {
                GameObject removeAllButton = CreateButton(buttonsBar, "RemoveAllFromCartButton", 425, 110); // Shifted 800 units to the right
                AddButtonEvents(removeAllButton.GetComponent<Button>(), removeAllButton.GetComponent<Image>(), OnRemoveAllFromCartButtonClick);
            }

            // Create the new button if it doesn't exist
            if (buttonsBar.transform.Find("NeedsOnlyButton") == null)
            {
                // Position this button 50px to the right of the "AddAllToCartButton"
                GameObject newButton = CreateButton(buttonsBar, "NeedsOnlyButton", -325, 55); // Half width, 50px right
                AddButtonEvents(newButton.GetComponent<Button>(), newButton.GetComponent<Image>(), OnNeedsOnlyButtonClick);
            }

            return true;
        }

        private static GameObject CreateButton(GameObject parent, string name, float xOffset, float width)
        {
            // Create the button GameObject
            GameObject buttonObject = new GameObject(name);
            RectTransform rectTransform = buttonObject.AddComponent<RectTransform>();
            Button buttonComponent = buttonObject.AddComponent<Button>();
            Image buttonImage = buttonObject.AddComponent<Image>();

            // Set the button's parent to Buttons_Bar
            buttonObject.transform.SetParent(parent.transform, false);

            // Set up RectTransform properties
            rectTransform.sizeDelta = new Vector2(width, 35); // Adjust width here
            rectTransform.anchoredPosition = new Vector2(xOffset, 612);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            // Create and configure the text component
            GameObject textObject = new GameObject("ButtonText");
            textObject.transform.SetParent(buttonObject.transform, false);

            RectTransform textRectTransform = textObject.AddComponent<RectTransform>();
            Text textComponent = textObject.AddComponent<Text>();

            textRectTransform.sizeDelta = rectTransform.sizeDelta;
            textRectTransform.anchoredPosition = Vector2.zero;

            textComponent.text = name == "AddAllToCartButton" ? "Add All to Cart" :
                                  name == "RemoveAllFromCartButton" ? "Remove All from Cart" : "Needs Only Button";
            textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            textComponent.alignment = TextAnchor.MiddleCenter;
            textComponent.color = Color.black;
            textComponent.fontStyle = FontStyle.Bold;

            return buttonObject;
        }

        private static void AddButtonEvents(Button button, Image buttonImage, UnityEngine.Events.UnityAction onClickAction)
        {
            EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();

            // Hover enter event
            EventTrigger.Entry pointerEnter = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter
            };
            pointerEnter.callback.AddListener((data) => OnHoverEnter(buttonImage));
            trigger.triggers.Add(pointerEnter);

            // Hover exit event
            EventTrigger.Entry pointerExit = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerExit
            };
            pointerExit.callback.AddListener((data) => OnHoverExit(buttonImage));
            trigger.triggers.Add(pointerExit);

            // Click event (only triggers if hovered)
            button.onClick.AddListener(() =>
            {
                if (buttonImage.color != Color.white)
                {
                    onClickAction.Invoke();
                }
            });
        }

        private static void OnHoverEnter(Image buttonImage)
        {
            buttonImage.color = new Color(5f / 255f, 133f / 255f, 208f / 255f); // Light blue hover color
        }

        private static void OnHoverExit(Image buttonImage)
        {
            buttonImage.color = Color.white; // Revert color when not hovering
        }

        private static void OnAddAllToCartButtonClick()
        {
            ProductListing productListing = GameObject.FindFirstObjectByType<ProductListing>();
            ManagerBlackboard managerBlackboard = GameObject.FindFirstObjectByType<ManagerBlackboard>();

            if (productListing == null || managerBlackboard == null) return;

            foreach (var productPrefab in productListing.productPrefabs)
            {
                var productComponent = productPrefab.GetComponent<Data_Product>();
                if (productComponent != null && productListing.unlockedProductTiers[productComponent.productTier])
                {
                    float boxPrice = productComponent.basePricePerUnit * productComponent.maxItemsPerBox;
                    boxPrice *= productListing.tierInflation[productComponent.productTier];
                    float roundedBoxPrice = Mathf.Round(boxPrice * 100f) / 100f;
                    managerBlackboard.AddShoppingListProduct(productComponent.productID, roundedBoxPrice);
                }
            }
        }

        private static void OnRemoveAllFromCartButtonClick()
        {
            ManagerBlackboard managerBlackboard = GameObject.FindFirstObjectByType<ManagerBlackboard>();

            if (managerBlackboard == null) return;

            int itemCount = managerBlackboard.shoppingListParent.transform.childCount;
            for (int i = itemCount - 1; i >= 0; i--)
            {
                managerBlackboard.RemoveShoppingListProduct(i);
            }
        }

        private static void OnNeedsOnlyButtonClick()
        {
            ProductListing productListing = GameObject.FindFirstObjectByType<ProductListing>();
            ManagerBlackboard managerBlackboard = GameObject.FindFirstObjectByType<ManagerBlackboard>();

            if (productListing == null || managerBlackboard == null) return;

            // Define your threshold for product existence
            int threshold = 40; // Adjust this value as needed

            foreach (var productPrefab in productListing.productPrefabs)
            {
                var productComponent = productPrefab.GetComponent<Data_Product>();
                if (productComponent != null && productListing.unlockedProductTiers[productComponent.productTier])
                {
                    int productID = productComponent.productID;

                    // Get the count of existing products
                    int[] productExistences = managerBlackboard.GetProductsExistences(productID);
                    int totalExistence = 0;
                    foreach (int count in productExistences)
                    {
                        totalExistence += count;
                    }

                   
                    // Only order the product if the total existence meets the threshold
                    if (totalExistence <= threshold)
                    {
                        //Debug.Log($"Ordering Product ID: {productID} - Total Existence: {totalExistence} is under the threshold.");

                        float boxPrice = productComponent.basePricePerUnit * productComponent.maxItemsPerBox;
                        boxPrice *= productListing.tierInflation[productComponent.productTier];
                        float roundedBoxPrice = Mathf.Round(boxPrice * 100f) / 100f;
                        managerBlackboard.AddShoppingListProduct(productID, roundedBoxPrice);
                    }
                }
            }
        }


    }
}