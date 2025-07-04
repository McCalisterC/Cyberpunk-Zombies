// Assets/Scripts/Player/Weapons/GrappleHook.cs
using System.Collections;
using UnityEngine;

public class GrappleHook : MonoBehaviour
{
    [Header("Grapple Settings")]
    [SerializeField] private float grappleRange = 50f;
    [SerializeField] private float grappleSpeed = 20f;
    [SerializeField] private float pullForce = 15f;
    [SerializeField] private LayerMask grappleableLayers = -1;
    [SerializeField] private float cooldownTime = 5f;
    
    [Header("Visual Effects")]
    [SerializeField] private LineRenderer grappleLine;
    [SerializeField] private GameObject grappleHookPrefab; // Optional: for visual hook object
    
    private Camera playerCamera;
    private CharacterController characterController;
    private StarterAssets.FirstPersonController fpsController;
    private Input_Controller inputController;
    private Transform playerTransform;
    
    private bool isGrappling = false;
    private bool isPullingPlayer = false;
    private bool isPullingZombie = false;
    private bool cooldown = false;
    private Vector3 grapplePoint;
    private GameObject grappleTarget;
    private GameObject visualHook;
    
    private Coroutine grappleCoroutine;

    private void Start()
    {
        playerCamera = Camera.main;
        characterController = GetComponentInParent<CharacterController>();
        fpsController = GetComponentInParent<StarterAssets.FirstPersonController>();
        inputController = GetComponentInParent<Input_Controller>();
        playerTransform = transform.parent;
        
        // Setup line renderer if not assigned
        if (grappleLine == null)
        {
            grappleLine = gameObject.AddComponent<LineRenderer>();
            grappleLine.material = new Material(Shader.Find("Sprites/Default"));
            grappleLine.startColor = Color.blue;
            grappleLine.endColor = Color.green;
            grappleLine.startWidth = 0.1f;
            grappleLine.endWidth = 0.1f;
            grappleLine.enabled = false;
        }
    }

    private void Update()
    {
        HandleGrappleInput();
        UpdateGrappleLine();
        
        // Cancel grapple on jump
        if (inputController.jump && isGrappling)
        {
            CancelGrapple();
        }
    }

    private void HandleGrappleInput()
    {
        if (inputController.grapple && !isGrappling && !cooldown)
        {
            StartGrapple();
        }

        // Reset input
        inputController.grapple = false;
    }

    private void StartGrapple()
    {
        Vector3 rayOrigin = playerCamera.transform.position;
        Vector3 rayDirection = playerCamera.transform.forward;
        
        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, rayDirection, out hit, grappleRange, grappleableLayers))
        {
            grapplePoint = hit.point;
            grappleTarget = hit.collider.gameObject;
            cooldown = true;
            
            // Check if we hit a zombie
            Script_BasicEnemy zombie = hit.collider.GetComponentInParent<Script_BasicEnemy>();
            if (zombie != null)
            {
                // Pull zombie to player
                isPullingZombie = true;
                isPullingPlayer = false;
                Debug.Log("Grappling zombie: " + zombie.name);
            }
            else
            {
                // Pull player to location
                isPullingPlayer = true;
                isPullingZombie = false;
                Debug.Log("Grappling to location: " + grapplePoint);
            }
            
            isGrappling = true;
            grappleLine.enabled = true;
            
            // Create visual hook if prefab exists
            if (grappleHookPrefab != null)
            {
                visualHook = Instantiate(grappleHookPrefab, grapplePoint, Quaternion.identity);
            }
            
            // Start the grapple movement
            if (grappleCoroutine != null)
                StopCoroutine(grappleCoroutine);
            
            if (isPullingPlayer)
                grappleCoroutine = StartCoroutine(PullPlayerToPoint());
            else if (isPullingZombie)
                grappleCoroutine = StartCoroutine(PullZombieToPlayer());
        }
        else
        {
            Debug.Log("Grapple missed - no target in range");
        }
    }

    private IEnumerator PullPlayerToPoint()
    {
        Vector3 startPosition = playerTransform.position;
        float journeyLength = Vector3.Distance(startPosition, grapplePoint);
        float journeyTime = journeyLength / grappleSpeed;
        float elapsedTime = 0;
        
        // Disable normal movement during grapple
        if (fpsController != null)
            fpsController.enabled = false;
        
        while (elapsedTime < journeyTime && isGrappling)
        {
            elapsedTime += Time.deltaTime;
            float fractionOfJourney = elapsedTime / journeyTime;
            
            Vector3 currentPosition = Vector3.Lerp(startPosition, grapplePoint, fractionOfJourney);
            Vector3 movement = currentPosition - playerTransform.position;
            
            // Use CharacterController to move
            if (characterController != null && characterController.enabled)
            {
                characterController.Move(movement);
            }
            
            yield return null;
        }
        
        // Re-enable normal movement
        if (fpsController != null)
            fpsController.enabled = true;
        
        EndGrapple();
    }

    private IEnumerator PullZombieToPlayer()
    {
        if (grappleTarget == null)
        {
            EndGrapple();
            yield break;
        }
        
        Script_BasicEnemy zombie = grappleTarget.GetComponentInParent<Script_BasicEnemy>();
        if (zombie == null)
        {
            EndGrapple();
            yield break;
        }

        grappleTarget = zombie.gameObject;

        Vector3 startPosition = grappleTarget.transform.position;
        Vector3 targetPosition = playerCamera.transform.position + playerCamera.transform.forward * 1f; // Stop 1 units in front of player

        targetPosition.y = startPosition.y;
        
        float journeyLength = Vector3.Distance(startPosition, targetPosition);
        float journeyTime = journeyLength / pullForce;
        float elapsedTime = 0;
        
        while (elapsedTime < journeyTime && isGrappling && grappleTarget != null)
        {
            elapsedTime += Time.deltaTime;
            float fractionOfJourney = elapsedTime / journeyTime;
            
            Vector3 currentPosition = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);
            
            // Move the zombie (you might need to adjust this based on your zombie movement system)
            if (grappleTarget != null)
            {
                grappleTarget.transform.position = currentPosition;
                grapplePoint = currentPosition; // Update grapple point for line renderer
            }
            
            yield return null;
        }
        
        EndGrapple();
    }

    private void UpdateGrappleLine()
    {
        if (isGrappling && grappleLine.enabled)
        {
            grappleLine.positionCount = 2;
            grappleLine.SetPosition(0, transform.position);
            
            if (isPullingZombie && grappleTarget != null)
            {
                grappleLine.SetPosition(1, grappleTarget.transform.position);
            }
            else
            {
                grappleLine.SetPosition(1, grapplePoint);
            }
        }
    }

    private void CancelGrapple()
    {
        if (grappleCoroutine != null)
        {
            StopCoroutine(grappleCoroutine);
        }
        
        // Re-enable normal movement if it was disabled
        if (fpsController != null && !fpsController.enabled)
            fpsController.enabled = true;
        
        EndGrapple();
        Debug.Log("Grapple cancelled by jump");
    }

    private void EndGrapple()
    {
        isGrappling = false;
        isPullingPlayer = false;
        isPullingZombie = false;
        grappleLine.enabled = false;
        grappleTarget = null;
        
        // Destroy visual hook
        if (visualHook != null)
        {
            Destroy(visualHook);
            visualHook = null;
        }

        // Start Cooldown
        StartCoroutine(GrappleCooldown());
    }

    private IEnumerator GrappleCooldown()
    {
        yield return new WaitForSeconds(cooldownTime);

        cooldown = false;
    }

    // Public methods for external control
    public bool IsGrappling() => isGrappling;
    public void ForceEndGrapple() => EndGrapple();
}