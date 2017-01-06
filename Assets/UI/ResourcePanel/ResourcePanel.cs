using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class ResourcePanel : MonoBehaviour
{
    public Text nickLabel;
    public Text popLabel;
    public Text energyLabel;
    public Text copperLabel;
    public Text foodLabel;
    public Text ironLabel;
    public Text garageLabel;
    public Text infectionLabel;

    private Dictionary<Text, bool> labelsRegisteredForFlashingFeedbackSupportDict = new Dictionary<Text, bool>();
    private PlayerCityController localPlayerCity;

    public static ResourcePanel instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;

        else if (instance != this)
        {
            Debug.LogError("Tried to create another instance of " + GetType() + ". Destroying.");
            Destroy(gameObject);
        }
    }

    // Use this for initialization
    private void Start()
    {
        RegisterResourceLabelsForFlashingFeedbackSupport();
        InvokeRepeating("UpdateResourceLabels", 0, 0.3f); // Dont update it to often, so we use a slow updater
    }

    private void RegisterResourceLabelsForFlashingFeedbackSupport()
    {
        labelsRegisteredForFlashingFeedbackSupportDict.Add(copperLabel, false);
        labelsRegisteredForFlashingFeedbackSupportDict.Add(ironLabel, false);
        labelsRegisteredForFlashingFeedbackSupportDict.Add(foodLabel, false);
    }

    public void FlashMissingResource(string type)
    {
        if (type == CopperItem.SerializedType)
            FlashMissingResource(copperLabel);
        else if (type == IronItem.SerializedType)
            FlashMissingResource(ironLabel);
        else if (type == FoodItem.SerializedType)
            FlashMissingResource(foodLabel);
        else
            Debug.LogError(string.Format("ResourceType {0} not registered for flashing feedback support.", type));
    }

    private void FlashMissingResource(Text label)
    {
        if (labelsRegisteredForFlashingFeedbackSupportDict[label] == false)
            StartCoroutine(AddFlashEffectToTextField(label, 4));
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void RegisterLocalPlayerCity(PlayerCityController playerCity)
    {
        localPlayerCity = playerCity;
    }

    private void UpdateResourceLabels()
    {
        if (localPlayerCity != null)
        {
            nickLabel.text = localPlayerCity.Nick;
            popLabel.text = string.Format("Pop: {0} ({1:0}%)", localPlayerCity.PopulationManager.Population, Math.Round(localPlayerCity.PopulationManager.GrowthProgress * 100, 0));
            energyLabel.text = string.Format("Energy: {0}/{1}", localPlayerCity.Energy, PlayerCityController.Settings_MaxEnergyStorage);
            copperLabel.text = "Copper: " + localPlayerCity.GetItemCount<CopperItem>();
            ironLabel.text = "Iron: " + localPlayerCity.GetItemCount<IronItem>();
            foodLabel.text = "Food: " + localPlayerCity.GetItemCount<FoodItem>();
            garageLabel.text = "Garage: " + localPlayerCity.Garage.Count();
            infectionLabel.text = string.Format("Infection: {0:0.0}%", localPlayerCity.GetRelativeInfection());
        }
    }

    /// <summary>
    /// Will flash the selected text field 'numberOfBlinks' times. 
    /// Any text field you intend to use this function on should be added to the dictionary in RegisterResourceLabelsForFlashingFeedbackSupport(). 
    /// This is done to prevent multiple flashing effects on the one label at the same time.
    /// </summary>
    private IEnumerator AddFlashEffectToTextField(Text textField, int numberOfBlinks)
    {
        if (!labelsRegisteredForFlashingFeedbackSupportDict.ContainsKey(textField))
        {
            Debug.LogError("AddBlinkEffectToTextField called on textfield which has not been added to isTextBlinkingDict in RegisterResourceLabelsForBlinkingFeedbackSupport(). Plz fix.");
            yield break;
        }

        if (labelsRegisteredForFlashingFeedbackSupportDict[textField] == true)
            yield break;
        else
            labelsRegisteredForFlashingFeedbackSupportDict[textField] = true;

        Color originalColor = textField.color;

        for (int i = 0; i < numberOfBlinks; i++)
        {
            textField.color = Color.red;
            yield return new WaitForSeconds(.1f);
            textField.color = originalColor;
            yield return new WaitForSeconds(.1f);
        }

        labelsRegisteredForFlashingFeedbackSupportDict[textField] = false;
    }
}