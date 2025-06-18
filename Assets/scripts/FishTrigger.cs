using UnityEngine;

public class FishTrigger : MonoBehaviour
{
    private bool hasBeenCollected = false; // To prevent multiple triggers

    private void OnTriggerEnter(Collider other)
    {
        // Check if the entering collider belongs to the Player
        // And ensure the GameManager instance exists
        if (other.CompareTag("Player") && !hasBeenCollected && GameManager.Instance != null)
        {
            Debug.Log("Cat (Player) stepped on the fish! Trigger activated!");

            hasBeenCollected = true; // Mark as collected

            // Tell the GameManager that a fish has been collected
            GameManager.Instance.CollectFish();

            // Optionally, disable the renderer and collider immediately
            // so the fish visually disappears, even if it's not destroyed yet.
            GetComponent<Collider>().enabled = false;
            // If your fish has a MeshRenderer, disable it:
            if (GetComponent<MeshRenderer>() != null)
            {
                GetComponent<MeshRenderer>().enabled = false;
            }
            // If it has children with renderers (e.g., a complex model)
            foreach (Transform child in transform)
            {
                if (child.GetComponent<MeshRenderer>() != null)
                {
                    child.GetComponent<MeshRenderer>().enabled = false;
                }
            }

            // Destroy the GameObject after a short delay (e.g., 0.1 seconds)
            // This allows the GameManager to process the collection before destruction.
            Destroy(gameObject, 0.1f);

            // If you prefer to just make it disappear instantly without destroying
            // gameObject.SetActive(false);
        }
    }
}