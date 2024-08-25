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

            if (buttonsBar != null)
            {
                
                // Create a new GameObject for the button
                GameObject buttonObject = new GameObject("MyButton");

                // Add RectTransform, Button, and Image components
                RectTransform rectTransform = buttonObject.AddComponent<RectTransform>();
                Button buttonComponent = buttonObject.AddComponent<Button>();
                Image buttonImage = buttonObject.AddComponent<Image>();

                // Set the button's parent to Buttons_Bar
                buttonObject.transform.SetParent(buttonsBar.transform, false);

                // Set up RectTransform properties
                rectTransform.sizeDelta = new Vector2(110, 35);
                rectTransform.anchoredPosition = new Vector2(-450, 612);
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);

                
                // Create a Text GameObject as a child of the button
                GameObject textObject = new GameObject("ButtonText");
                textObject.transform.SetParent(buttonObject.transform, false);

                // Add RectTransform and Text components
                RectTransform textRectTransform = textObject.AddComponent<RectTransform>();
                Text textComponent = textObject.AddComponent<Text>();

                // Set up RectTransform properties for the text
                textRectTransform.sizeDelta = new Vector2(100, 30); // Match button size
                textRectTransform.anchoredPosition = Vector2.zero; // Center text

                // Configure Text component
                textComponent.text = "Add All to Cart"; // Set button text
                textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); // Use built-in Arial font
                textComponent.alignment = TextAnchor.MiddleCenter;
                textComponent.color = Color.black; // Set text color
                textComponent.fontStyle = FontStyle.Bold; // Set text style

                // Add hover effect
                EventTrigger trigger = buttonObject.AddComponent<EventTrigger>();

                // Pointer enter event
                EventTrigger.Entry pointerEnter = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerEnter
                };
                pointerEnter.callback.AddListener((data) => OnHoverEnter(buttonImage));
                trigger.triggers.Add(pointerEnter);

                // Pointer exit event
                EventTrigger.Entry pointerExit = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerExit
                };
                pointerExit.callback.AddListener((data) => OnHoverExit(buttonImage));
                trigger.triggers.Add(pointerExit);

                // Add click functionality
                buttonComponent.onClick.AddListener(OnButtonClick);

                
            }
            

           return true;
        }

        private static void OnHoverEnter(Image buttonImage)
        {
            // Define RGB values (0-255) and convert them to the 0-1 range
            float r = 5f / 255f; // Light red
            float g = 133f / 255f; // Light green
            float b = 208f / 255f; // Light blue

            // Create a Color object from RGB values
            Color hoverColor = new Color(r, g, b);

            // Change color on hover
            buttonImage.color = hoverColor;
        }


        private static void OnHoverExit(Image buttonImage)
        {
            buttonImage.color = Color.white; // Revert color when not hovering
        }

        private static void OnButtonClick()
        {
            // Get the ManagerBlackboard instance
            ProductListing productListing = GameObject.FindFirstObjectByType<ProductListing>();
            ManagerBlackboard managerBlackboard = GameObject.FindFirstObjectByType<ManagerBlackboard>();

            if (managerBlackboard != null)
            {
                
                // Loop through all products
                for (int productID = 0; productID < productListing.productPrefabs.Length; productID++)
                {
                    var productPrefab = productListing.productPrefabs[productID];

                    // Get the Data_Product component to access price and other relevant data
                    var productComponent = productPrefab.GetComponent<Data_Product>();
                    if (productComponent != null)
                    {
                        // Check if the product's tier is unlocked
                        if (productListing.unlockedProductTiers[productComponent.productTier])
                        {
                            // Calculate boxPrice based on basePricePerUnit and maxItemsPerBox
                            float boxPrice = productComponent.basePricePerUnit * productComponent.maxItemsPerBox;
                            float[] inflationMultiplier = productListing.tierInflation;
                            boxPrice *= inflationMultiplier[productComponent.productTier];
                            float newBoxPrice = Mathf.Round(boxPrice * 100f) / 100f;

                            // Add the product to the shopping list
                            managerBlackboard.AddShoppingListProduct(productID, newBoxPrice);
                        }
                        
                    }
                }
            }
            
        }




    }
}
