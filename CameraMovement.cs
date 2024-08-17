using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwipe : MonoBehaviour
{
    public float dragSpeed = 2;
    public Vector2 minBoundary;
    public Vector2 maxBoundary;
    public float boundaryResistance = 0.5f;
    public float fixedHeightY; // Fixed height for the isometric camera on the Y-axis
    public float brakeSpeed = 0.9f; // Smooth braking speed
    public float maxSpeed = 0.01f;
    public float directionChangeSmoothing = 0.1f; // Smoothing factor for direction change

    private Vector3 dragOrigin;
    private Vector3 currentVelocity;
    private Camera cam;
    private bool isDragging = false;
    private Vector2 lastTouch = Vector2.zero;

    void Start()
    {
        cam = Camera.main;
        fixedHeightY = cam.transform.position.y; // Set the fixed height to the camera's initial y position
    }

    void Update()
    {
        Vector3 currentScreenPoint = Vector3.zero;

        // Handle touch input
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            currentScreenPoint = new Vector3(touch.position.x, touch.position.y, 0);

            if (touch.phase == TouchPhase.Began)
            {
                dragOrigin = cam.ScreenToViewportPoint(currentScreenPoint);
                isDragging = true;
                currentVelocity = Vector3.zero; // Reset velocity on new touch
                Debug.Log("Touch began at: " + dragOrigin);
            }
            else if (touch.phase == TouchPhase.Moved && isDragging)
            {
                Vector3 currentViewportPoint = cam.ScreenToViewportPoint(currentScreenPoint);
                Vector3 difference = dragOrigin - currentViewportPoint;
                Vector3 worldDifference = new Vector3(difference.x * dragSpeed, 0, difference.y * dragSpeed);

                // Smoothly transition to the new direction
                currentVelocity = Vector3.Lerp(currentVelocity, worldDifference, directionChangeSmoothing);

                MoveCamera(currentVelocity);
                dragOrigin = currentViewportPoint; // Update drag origin for smooth transition
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                isDragging = false;
            }
        }

        // Apply smooth braking if not dragging
        if (!isDragging)
        {
            currentVelocity *= brakeSpeed;
            if (currentVelocity.magnitude > 0.000001f) // Small threshold to stop completely
            {
                MoveCamera(currentVelocity);
            }
        }
    }

    private void MoveCamera(Vector3 difference)
    {
        difference.z *= 0.5f;
        if (difference == Vector3.zero) return;

        Vector3 newPosition = cam.transform.position + difference;

        Debug.Log("New camera position before boundaries: " + newPosition);

        // Apply boundary resistance
        if (newPosition.x < minBoundary.x)
        {
            float resistance = Mathf.Lerp(1, 0, Mathf.InverseLerp(minBoundary.x, minBoundary.x - boundaryResistance, newPosition.x));
            newPosition.x = Mathf.Lerp(newPosition.x, minBoundary.x, 1 - resistance);
        }
        if (newPosition.x > maxBoundary.x)
        {
            float resistance = Mathf.Lerp(1, 0, Mathf.InverseLerp(maxBoundary.x, maxBoundary.x + boundaryResistance, newPosition.x));
            newPosition.x = Mathf.Lerp(newPosition.x, maxBoundary.x, 1 - resistance);
        }
        if (newPosition.z < minBoundary.y)
        {
            float resistance = Mathf.Lerp(1, 0, Mathf.InverseLerp(minBoundary.y, minBoundary.y - boundaryResistance, newPosition.z));
            newPosition.z = Mathf.Lerp(newPosition.z, minBoundary.y, 1 - resistance);
        }
        if (newPosition.z > maxBoundary.y)
        {
            float resistance = Mathf.Lerp(1, 0, Mathf.InverseLerp(maxBoundary.y, maxBoundary.y + boundaryResistance, newPosition.z));
            newPosition.z = Mathf.Lerp(newPosition.z, maxBoundary.y, 1 - resistance);
        }

        // Ensure we are not assigning invalid positions
        if (float.IsNaN(newPosition.x) || float.IsInfinity(newPosition.x) ||
            float.IsNaN(newPosition.y) || float.IsInfinity(newPosition.y) ||
            float.IsNaN(newPosition.z) || float.IsInfinity(newPosition.z))
        {
            Debug.LogError("Invalid camera position calculated: " + newPosition);
            return;
        }

        newPosition.y = fixedHeightY; // Keep the camera at the fixed height on the Y-axis

        Debug.Log("New camera position after boundaries: " + newPosition);
        cam.transform.position = newPosition;
    }
}

