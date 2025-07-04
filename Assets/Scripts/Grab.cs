using UnityEngine;

public class Grab : MonoBehaviour
{
    public Camera playerCamera; // Assign your camera in the Inspector
    public float grabDistance = 3f; // How far you can grab
    public float holdSmoothness = 10f; // How quickly the object moves to the hold position
    private GameObject heldObject;
    private Rigidbody heldRigidbody;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldObject == null)
            {
                RaycastHit hit;
                // Raycast from the camera forward
                if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, grabDistance))
                {
                    // Check if the object is grabbable (e.g., has a tag)
                    if (hit.collider.CompareTag("Grabbable"))
                    {
                        heldObject = hit.collider.gameObject;
                        heldRigidbody = heldObject.GetComponent<Rigidbody>();
                        if (heldRigidbody != null)
                        {
                            heldRigidbody.isKinematic = false; // Disable physics while holding
                            heldRigidbody.useGravity = false;
                            heldRigidbody.linearDamping = 10f; // Optional: add drag for smoother movement
                        }
                    }
                }
            }
            else
            {
                // Drop the object
                if (heldRigidbody != null)
                {
                    heldRigidbody.useGravity = true;
                    
                    heldRigidbody.linearDamping = 0f;
                }
                heldObject = null;
                heldRigidbody = null;
            }
        }

        // Move held object with Rigidbody
        if (heldObject != null && heldRigidbody != null)
        {
            Vector3 targetPosition = playerCamera.transform.position + playerCamera.transform.forward * grabDistance;
            Vector3 direction = targetPosition - heldObject.transform.position;
            heldRigidbody.linearVelocity = direction * holdSmoothness;
        }
    }
}
