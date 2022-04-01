﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KirinController : MonoBehaviour
{

    public bool useCharacterForward = false;
    public float turnSpeed = 10f;
    public KeyCode walkToggle = KeyCode.LeftShift;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode slashKey = KeyCode.E;
    public KeyCode fireWeapon = KeyCode.Mouse0;

    private float turnSpeedMultiplier;
    private float speed = 0f;
    private float direction = 0f;
    private float groundCheckDistance = 0.25f;
    private bool isWalking = true;
    //private bool isSprinting = false;
    private bool isGrounded;
    private Animator anim;
    private Vector3 targetDirection;
    private Vector2 input;
    Vector3 groundNormal;
    private Quaternion freeRotation;
    private Camera mainCamera;
    private float velocity;

    void Start()
    {
        anim = GetComponent<Animator>();
        mainCamera = Camera.main;
    }

    void FixedUpdate()
    {
        input.x = Input.GetAxis("Horizontal");
        input.y = Input.GetAxis("Vertical");

        // set speed to both vertical and horizontal inputs
        if (useCharacterForward)
            speed = Mathf.Abs(input.x) + input.y;
        else
            speed = Mathf.Abs(input.x) + Mathf.Abs(input.y);

        speed = Mathf.Clamp(speed, 0f, 1f);
        speed = Mathf.SmoothDamp(anim.GetFloat("Speed"), speed, ref velocity, 0.1f);
        //walkspeed
        if (isWalking == true)
            speed = Mathf.Clamp(speed, 0f, 0.2f); ;

        anim.SetFloat("Speed", speed);

        if (input.y < 0f && useCharacterForward)
            direction = input.y;
        else
            direction = 0f;

        anim.SetFloat("Direction", direction);
        CheckGroundStatus();
        anim.SetBool("OnGround", isGrounded);

        // walk toggle
        if (Input.GetKeyDown(walkToggle))
            isWalking = !isWalking;

        //Kirin Actions
        if (Input.GetKey(jumpKey) && isGrounded == true)
            anim.SetTrigger("Jump");
        else
            anim.ResetTrigger("Jump");

        if (Input.GetKey(slashKey) && isGrounded == true)
            anim.SetTrigger("AttackSlash");
        else
            anim.ResetTrigger("AttackSlash");

        if (Input.GetKey(fireWeapon) && isGrounded == true)
            anim.SetTrigger("AttackShoot");
        else
            anim.ResetTrigger("AttackShoot");

        //// set sprinting
        //isSprinting = ((Input.GetKey(sprintJoystick) || Input.GetKey(sprintKeyboard)) && input != Vector2.zero && direction >= 0f);
        //anim.SetBool("isSprinting", isSprinting);

        // Update target direction relative to the camera view (or not if the Keep Direction option is checked)
        UpdateTargetDirection();
        if (input != Vector2.zero && targetDirection.magnitude > 0.1f)
        {
            Vector3 lookDirection = targetDirection.normalized;
            freeRotation = Quaternion.LookRotation(lookDirection, transform.up);
            var diferenceRotation = freeRotation.eulerAngles.y - transform.eulerAngles.y;
            var eulerY = transform.eulerAngles.y;

            if (diferenceRotation < 0 || diferenceRotation > 0) eulerY = freeRotation.eulerAngles.y;
            var euler = new Vector3(0, eulerY, 0);

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(euler), turnSpeed * turnSpeedMultiplier * Time.deltaTime);
        }
    }

    public virtual void UpdateTargetDirection()
    {
        if (!useCharacterForward)
        {
            turnSpeedMultiplier = 1f;
            var forward = mainCamera.transform.TransformDirection(Vector3.forward);
            forward.y = 0;

            //get the right-facing direction of the referenceTransform
            var right = mainCamera.transform.TransformDirection(Vector3.right);

            // determine the direction the player will face based on input and the referenceTransform's right and forward directions
            targetDirection = input.x * right + input.y * forward;
        }
        else
        {
            turnSpeedMultiplier = 0.2f;
            var forward = transform.TransformDirection(Vector3.forward);
            forward.y = 0;

            //get the right-facing direction of the referenceTransform
            var right = transform.TransformDirection(Vector3.right);
            targetDirection = input.x * right + Mathf.Abs(input.y) * forward;
        }
    }

    void CheckGroundStatus()
    {
        RaycastHit hitInfo;

#if UNITY_EDITOR
        // helper to visualise the ground check ray in the scene view
        Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * groundCheckDistance));
#endif
        // 0.1f is a small offset to start the ray from inside the character
        // it is also good to note that the transform position in the sample assets is at the base of the character
        if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, groundCheckDistance))
        {
            groundNormal = hitInfo.normal;
            isGrounded = true;
            //anim.applyRootMotion = true;
        }
        else
        {
            isGrounded = false;
            groundNormal = Vector3.up;

        }
    }
}
