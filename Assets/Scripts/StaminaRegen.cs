using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StaminaRegen : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Drag your UI Text here")]
    public Text staminaText;

    [Header("Stamina Settings")]
    [Tooltip("Maximum stamina value")]
    public int maxStamina = 15;
    [Tooltip("Seconds between each +1")]
    public float regenInterval = 2f;

    public int currentStamina = 0;

    private float sprintDrainTimer = 0f;

    void Start()
    {
        UpdateStaminaDisplay();
        StartCoroutine(RegenerateStamina());
    }

    void Update()
    {
        UpdateStaminaDisplay();
        if (Input.GetKey(KeyCode.LeftShift) && currentStamina > 0)
        {
            sprintDrainTimer += Time.deltaTime;
            if (sprintDrainTimer >= 1f)
            {
                DrainStamina(1);
                sprintDrainTimer = 0f;
            }
        }
        else
        {
            sprintDrainTimer = 0f;
        }
    }

    IEnumerator RegenerateStamina()
    {
        while (true)
        {
            // Only regenerate if not holding shift and stamina is below max
            if (!Input.GetKey(KeyCode.LeftShift) && currentStamina < maxStamina)
            {
                yield return new WaitForSeconds(regenInterval);
                currentStamina++;
                UpdateStaminaDisplay();
            }
            else
            {
                yield return null;
            }
        }
    }

    void UpdateStaminaDisplay()
    {
        if (staminaText != null)
            staminaText.text = "Stamina: " + currentStamina.ToString();
    }

    public void DrainStamina(int amount)
    {
        currentStamina = Mathf.Max(0, currentStamina - amount);
        UpdateStaminaDisplay();
    }
}
