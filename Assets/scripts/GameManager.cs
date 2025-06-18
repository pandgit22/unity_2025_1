using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; // For reloading scenes
using TMPro;                     // For TextMeshPro UI elements
using UnityEngine.UI;              // For Image (hunger bar fill)

public class GameManager : MonoBehaviour
{
    // Singleton instance for easy access from other scripts
    public static GameManager Instance { get; private set; }

    [Header("Hunger Settings")]
    public float maxHunger = 100f;
    public float hungerDrainRate = 5f; // Hunger drained per second
    public float hungerReplenishAmount = 25f; // Hunger gained per fish
    public float lowHungerThreshold = 20f; // Threshold for "low hunger" state

    [Header("UI Elements")]
    public TextMeshProUGUI hungerText;       // Displays current hunger value (e.g., "Hunger: 75/100")
    public Image hungerFillImage;            // The 'Filled' Image component for the hunger bar itself
    public TextMeshProUGUI fishCountText;    // Displays fish collected count (e.g., "Fish: 3/10")

    public GameObject winPanel;              // The UI panel to show when the player wins
    public TextMeshProUGUI winText;          // Text on the win panel (e.g., "YOU WIN!")
    public GameObject gameOverPanel;         // The UI panel to show when the game is over (lost)
    public TextMeshProUGUI gameOverText;     // Text on the game over panel (e.g., "GAME OVER!")

    [Header("Game State")]
    [Tooltip("The cat's current hunger level.")]
    public float currentHunger;
    [Tooltip("The total number of fish (Food tag) initially in the scene.")]
    public int totalFishInScene;
    [Tooltip("The number of fish the cat has collected.")]
    public int collectedFishCount;
    [Tooltip("Is the game currently in a win or lose state?")]
    public bool gameOver = false;
    private bool hungerDrainEnabled = true; // Control for pausing/resuming hunger drain

    // --- Singleton Implementation ---
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // If another GameManager already exists, destroy this duplicate
            Destroy(gameObject);
        }
        else
        {
            // Set this instance as the singleton
            Instance = this;
            // Optionally, keep GameManager alive across scene loads.
            // If used, ensure RestartGame properly resets all game elements.
            // For now, reloading the scene simplifies resets.
            // DontDestroyOnLoad(gameObject);
        }
    }

    // --- Game Initialization ---
    void Start()
    {
        InitializeGameState();
        StartCoroutine(HungerDrainRoutine()); // Start the continuous hunger drain
    }

    private void InitializeGameState()
    {
        currentHunger = maxHunger;
        collectedFishCount = 0;
        gameOver = false;
        hungerDrainEnabled = true;

        // Find all active fish in the scene by their "Food" tag
        // Note: This only counts objects currently active in the scene when Start() runs.
        // If fish are spawned later, totalFishInScene would need to be updated.
        totalFishInScene = GameObject.FindGameObjectsWithTag("Food").Length;

        // Hide win/game over panels at the start of the game
        if (winPanel != null) winPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        // Ensure cursor is locked and hidden at start
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f; // Ensure game is not paused from a previous game over screen

        // Update UI with initial values
        UpdateHungerUI();
        UpdateFishCountUI();
    }

    // --- Debugging/Testing Input ---
    void Update()
    {
        // For debugging: Press 'R' to restart the game
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }

        // For debugging: press 'Q' to quit the application
        if (Input.GetKeyDown(KeyCode.Q))
        {
            QuitGame();
        }
    }

    // --- Hunger Drain Logic (Coroutine) ---
    IEnumerator HungerDrainRoutine()
    {
        // Continue draining hunger as long as the game is not over
        while (!gameOver)
        {
            if (hungerDrainEnabled)
            {
                currentHunger -= hungerDrainRate * Time.deltaTime;
                currentHunger = Mathf.Max(0, currentHunger); // Ensure hunger doesn't go below 0

                UpdateHungerUI(); // Update hunger bar and text

                // Check for game over due to starvation
                if (currentHunger <= 0)
                {
                    LoseGame();
                }
            }
            yield return null; // Wait for the next frame before continuing the loop
        }
    }

    // --- Fish Collection ---
    // This method is called by the FishTrigger script when a fish is collected
    public void CollectFish()
    {
        if (gameOver) return; // Prevent collecting if game is already over

        collectedFishCount++;
        // Replenish hunger, ensuring it doesn't exceed maxHunger
        currentHunger = Mathf.Min(maxHunger, currentHunger + hungerReplenishAmount);

        UpdateHungerUI();    // Update hunger display
        UpdateFishCountUI(); // Update fish count display

        Debug.Log($"Fish collected! {collectedFishCount}/{totalFishInScene}. Current Hunger: {Mathf.CeilToInt(currentHunger)}");

        // Check for win condition
        if (collectedFishCount >= totalFishInScene)
        {
            WinGame();
        }
    }

    // --- Game Win Condition ---
    void WinGame()
    {
        if (gameOver) return; // Ensure win logic only runs once
        gameOver = true;
        hungerDrainEnabled = false; // Stop hunger drain

        Debug.Log("You Win!");

        // Show the win panel
        if (winPanel != null) winPanel.SetActive(true);
        // Customize the win message on the panel
        if (winText != null) winText.text = "YOU WIN!";

        PauseGameForUI(); // Pause game time and show cursor
    }

    // --- Game Lose Condition (Starvation) ---
    void LoseGame()
    {
        if (gameOver) return; // Ensure lose logic only runs once
        gameOver = true;
        hungerDrainEnabled = false; // Stop hunger drain

        Debug.Log("Game Over! You starved!");

        // Show the game over panel
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        // Customize the game over message on the panel
        if (gameOverText != null) gameOverText.text = "YOU STARVED!";

        PauseGameForUI(); // Pause game time and show cursor
    }

    // --- Game Control Methods ---
    public void RestartGame()
    {
        // Unpause game, re-lock/hide cursor before reloading
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Reload the current scene to reset everything (simplest approach for now)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit(); // This function only works when the game is built.

        // If running in the Unity editor, stop play mode
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // --- UI Update Helper Methods ---
    void UpdateHungerUI()
    {
        if (hungerText != null)
        {
            // Display hunger as a rounded integer
            hungerText.text = $"Hunger: {Mathf.CeilToInt(currentHunger)} / {maxHunger}";
        }

        if (hungerFillImage != null)
        {
            // Update the fill amount of the Image component (0 to 1)
            hungerFillImage.fillAmount = currentHunger / maxHunger;

            // Optional: Change the color of the hunger bar based on current hunger
            if (currentHunger <= lowHungerThreshold)
            {
                hungerFillImage.color = Color.Lerp(Color.red, Color.yellow, currentHunger / lowHungerThreshold);
            }
            else
            {
                // Interpolate from yellow (or green) to green as hunger increases
                hungerFillImage.color = Color.Lerp(Color.yellow, Color.green, (currentHunger - lowHungerThreshold) / (maxHunger - lowHungerThreshold));
            }
        }
    }

    void UpdateFishCountUI()
    {
        if (fishCountText != null)
        {
            fishCountText.text = $"Fish: {collectedFishCount} / {totalFishInScene}";
        }
    }

    // Helper to pause game and unlock cursor for UI interaction
    private void PauseGameForUI()
    {
        Time.timeScale = 0f; // Pause all game-time dependent operations
        Cursor.lockState = CursorLockMode.None; // Unlock cursor
        Cursor.visible = true; // Make cursor visible
    }
}