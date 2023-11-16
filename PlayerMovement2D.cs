// Code by MrDaveDev
// Feel free to use this, with credit.

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement2D : MonoBehaviour
{
    [Header("Basic Settings")] 
    [SerializeField] private float moveSpeed = 23f;
    
    [Space] [Tooltip("When toggled, the player will have better movement")] 
    [SerializeField] private bool advancedMovement;
    
    [Tooltip("The high this value is, the faster the player will come to a stop. (Advanced Movement Only)")]
    [SerializeField] private float moveDrag = 20f;

    
    
    
    [Header("Grid Settings")]
    [Tooltip("When toggled, the player will move based on a grid.")]
    [SerializeField] private bool gridBased;
    
    [Tooltip("When toggled, the player will be able to move diagonally. (Grid Based Movement Only")]
    [SerializeField] private bool diagonalMovement;
    
    [Space] [SerializeField] private Transform movePoint; // Set this as an empty game object prefab, at the center of the screen.
    private Transform movePointObj;

    [Tooltip("The layers selected here will stop the player from being able to move through those objects.")]
    [SerializeField] private LayerMask stopLayers;

    
    [Header("Dash Settings")] 
    [Tooltip("When toggled, the player will be able to dash. (Incompatible with Grid Based Movement)")] 
    [SerializeField] private bool canDash;

    private bool _isDashing;

    [Space] [SerializeField] private float dashPower = 125f;
    
    [Tooltip("How long the player dashes, in seconds.")]
    [SerializeField] private float dashTime = 0.2f;
    
    [Tooltip("How long until the player can dash again, in seconds.")]
    [SerializeField] private float dashCooldown = 0.3f;

    
    // Private Variables
    private Rigidbody2D _rb;
    private TrailRenderer _tr;
    
    private int _x;
    private int _y;
    
    
    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>(); // Set _rb variable to the player's Rigidbody2D.
        _tr = transform.Find("DashTrail").GetComponent<TrailRenderer>(); // Set _tr variable to the player's Trail Renderer.
    }

    
    // Update is called once per frame
    void FixedUpdate()
    {
        if (gridBased) // if gridBased is true, run code.
        {
            if (canDash) // if dashing is enabled, run code.
            {
                canDash = false;
                // Disable dashing.
            }
            
            gridMove();
        }
        else if (!gridBased) // if gridBased is false, run code.
        {
            normalMove();
            
            if (_x == 0 || _y == 0) // if player isn't moving, run code.
            {
                _rb.drag = moveDrag; // Slow down player.
            }
        }
    }

    
    public void Move(InputAction.CallbackContext context)
    {
        _x = (int)Mathf.Round(context.ReadValue<Vector2>().x); // Get value of _x, when x value keys are pressed.
        _y = (int)Mathf.Round(context.ReadValue<Vector2>().y); // Get value of _y, when y value keys are pressed.
    }

    
    public void DashInput(InputAction.CallbackContext context)
    {
        if (!_isDashing && canDash) // If dashing is allowed, and dashing is enabled, run code.
        {
            StartCoroutine(Dash()); // Start Dashing
        } else
        {
            return;
            // if the if-statement is false, stop code.
        }
    }

    
    IEnumerator Dash()
    {
        _isDashing = true; // Don't Allow Dashing

        _rb.velocity = new Vector2(_x, _y) * dashPower; // Make player dash in movement direction.
        _tr.emitting = true; // Turns on trail.

        yield return new WaitForSeconds(dashTime); // Wait to stop dash.
        _tr.emitting = false; // Turn off trail.

        yield return new WaitForSeconds(dashCooldown); // Wait to allow dashing.
        _isDashing = false; // Allow Dashing
    }

    
    void gridMove()
    {
        if (movePointObj == null)
        {
            movePointObj = Instantiate(movePoint, this.gameObject.transform);
            // Creates the movePoint Object, if there is none.
        }

        if (movePointObj.parent != null)
        {
            movePointObj.parent = null;
            // Sets the movePoint Object's parent to nothing, if it has a parent.
        }

        transform.position = Vector3.MoveTowards(transform.position, movePointObj.position, moveSpeed * Time.deltaTime); // Moves Player

        if (Vector3.Distance(transform.position, movePointObj.position) <= .05f) // If the distance is less than, or equal to, 0.05, run code.
        {
            if (!Physics2D.OverlapCircle(movePointObj.position + new Vector3(_x, _y, 0f), 0.2f, stopLayers)) // If the movePoint will not touch an obstacle, run code.
            {
                if (diagonalMovement) // If diagonal movement is true, run code.
                {
                    if (Mathf.Abs(_x) == 1f) // if the absolute of _x is one, run code.
                    {
                        movePointObj.position += new Vector3(_x, 0f, 0f);
                        // Move movePoint by the value of _x.
                    }
                    if (Mathf.Abs(_y) == 1f) // if the absolute of _y is one, run code.
                    {
                        movePointObj.position += new Vector3(0f, _y, 0f);
                        // Move movePoint by the value of _y.
                    }
                }
                else // Same as before, but no diagonal movement.
                {
                    if (Mathf.Abs(_x) == 1f)
                    {
                        movePointObj.position += new Vector3(_x, 0f, 0f);
                    }
                    else if (Mathf.Abs(_y) == 1f)
                    {
                        movePointObj.position += new Vector3(0f, _y, 0f);
                    }
                }
            }
        }
        else
        {
            return;
            // if the distance is greater than 0.05, stop code.
        }
    }

    
    void normalMove()
    {
        if (movePointObj != null) // If there is a movePoint Object, run code.
        {
            Destroy(movePointObj.gameObject);
            // Destroy movePoint Object.
        }

        if (advancedMovement)
        {
            _rb.AddForce(new Vector2(_x, _y) * (moveSpeed * 20), ForceMode2D.Force); // Move Player with advanced movement.
        }
        else if (!advancedMovement)
        {
            _rb.velocity += new Vector2(_x, _y) * (moveSpeed * 0.45f); // Move Player without advanced movement.
        }
    }
}
