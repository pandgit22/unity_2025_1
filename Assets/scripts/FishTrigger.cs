using UnityEngine;

public class FishTrigger : MonoBehaviour
{
    [Header("Sound Settings")]
    [SerializeField] private AudioClip collectSound; // The sound clip to play when collected

    private bool hasBeenCollected = false;
    private AudioSource audioSource; // Reference to the AudioSource component on this GameObject

    void Awake()
    {
        // Get the AudioSource component when the GameObject wakes up
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogWarning("AudioSource component not found on " + gameObject.name + ". Sound will not play.", this);
        }
    }

    // This method is called when another collider enters this trigger
    private void OnTriggerEnter(Collider other)
    {
        // Check if the entering collider belongs to the Player and hasn't been collected
        if (other.CompareTag("Player") && !hasBeenCollected)
        {
            hasBeenCollected = true; // Mark as collected

            // Play the sound effect if an AudioSource and AudioClip are assigned
            if (audioSource != null && collectSound != null)
            {
                
                audioSource.PlayOneShot(collectSound);
            }
            else if (audioSource == null)
            {
                Debug.LogWarning("FishTrigger: No AudioSource found to play sound!", this);
            }
            else if (collectSound == null)
            {
                Debug.LogWarning("FishTrigger: No Collect Sound assigned to play!", this);
            }

            gameObject.SetActive(false); // Make the fish disappear

            // Notify the GameManager that a fish has been collected
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CollectFish();
            }
            else
            {
                Debug.LogWarning("GameManager instance not found! Fish collection not registered.");
            }
        }
    }

    // Call this method when the game restarts to make the fish collectible again
    public void ResetFish()
    {
        hasBeenCollected = false;
    }

}