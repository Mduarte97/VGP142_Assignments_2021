using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class Character : MonoBehaviour
{
    CharacterController controller;
    Animator animator;

    // Attributes will help organize the Inspector
    [Header("Player Settings")]
    [Space(2)]
    [Tooltip("Speed value between 1 and 6.")]
    [Range(1.0f, 6.0f)]
    public float speed;
    public float jumpSpeed;
    public float rotationSpeed;
    public float gravity;

    Vector3 moveDirection;

    enum ControllerType { SimpleMove, Move };
    [SerializeField] ControllerType type;

    [Header("Weapon Settings")]
    // Handle weapon shooting
    public float projectileForce;
    public Rigidbody projectilePrefab;
    public Transform projectileSpawnPoint;

    [Header("Raycast Settings")]
    // Raycast variables
    public Transform thingToLookFrom;
    public float lookAtDistance;

    [Header("Health Settings")]
    public float health = 100f;
    public Text healthText;
    public bool isDamagePlayer;
    // Start is called before the first frame update
    void Start()
    {
        isDamagePlayer = false;
        try
        {
            controller = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();

            controller.minMoveDistance = 0.0f;

            animator.applyRootMotion = false;

            name = "Character";

            if (speed <= 0)
            {
                speed = 6.0f;

                Debug.Log("Speed not set on " + name + " defaulting to " + speed);
            }

            if (jumpSpeed <= 0)
            {
                jumpSpeed = 6.0f;

                Debug.Log("JumpSpeed not set on " + name + " defaulting to " + jumpSpeed);
            }

            if (rotationSpeed <= 0)
            {
                rotationSpeed = 10.0f;

                Debug.Log("RotationSpeed not set on " + name + " defaulting to " + rotationSpeed);
            }

            if (gravity <= 0)
            {
                gravity = 9.81f;

                Debug.Log("Gravity not set on " + name + " defaulting to " + gravity);
            }

            moveDirection = Vector3.zero;

            if (projectileForce <= 0)
            {
                projectileForce = 10.0f;

                //Debug.Log("ProjectileForce not set on " + name + " defaulting to " + projectileForce);
            }

            if (!projectilePrefab)
                Debug.LogWarning("Missing projectilePrefab on " + name);

            if (!projectileSpawnPoint)
                Debug.LogWarning("Missing projectileSpawnPoint on " + name);

            if (lookAtDistance <= 0)
            {
                lookAtDistance = 10.0f;
               // Debug.Log("LookAtDistance not set on " + name + " defaulting to " + lookAtDistance);
            }
        }
        catch (NullReferenceException e)
        {
            Debug.LogWarning(e.Message);
        }
        catch (UnassignedReferenceException e)
        {
            Debug.LogWarning(e.Message);
        }
        finally
        {
            Debug.LogWarning("Always get called");
        }

      if (health <= 0)
        {
            health = 100f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch(type)
        {
            case ControllerType.SimpleMove:

                //transform.Rotate(0, Input.GetAxis("Horizontal") * rotationSpeed, 0);

                controller.SimpleMove(transform.forward * Input.GetAxis("Vertical") * speed);

                break;

            case ControllerType.Move:

                if(controller.isGrounded)
                {
                    moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

                    moveDirection *= speed;

                    moveDirection = transform.TransformDirection(moveDirection);

                    if (Input.GetButtonDown("Jump"))
                        moveDirection.y = jumpSpeed;
                }

                moveDirection.y -= gravity * Time.deltaTime;

                controller.Move(moveDirection * Time.deltaTime);

                break;
        }

        if (Input.GetButtonDown("Fire1")) // Set in Edit | Project Settings | Input Manager
        {
         
            animator.SetTrigger("Attack");
        }

        else
        {
            animator.ResetTrigger("Attack");
        }

        if (Input.GetButton("Fire2"))
        {
            animator.SetTrigger("Kick");
           
        }

        else 
        {
            animator.ResetTrigger("Kick");
        }

        if(Input.GetButton("Fire3"))
        {
            animator.SetTrigger("IsFiring");
        }

        // Usage Raycast
        // - GameObject needs a Collider
        RaycastHit hit;

        if (!thingToLookFrom)
        {
            Debug.DrawRay(transform.position, transform.forward * lookAtDistance, Color.red);

            if (Physics.Raycast(transform.position, transform.forward, out hit, lookAtDistance))
            {
               // Debug.Log("Raycast hit: " + hit.transform.name);
            }
        }
        else
        {
            Debug.DrawRay(thingToLookFrom.transform.position, thingToLookFrom.transform.forward * lookAtDistance, Color.yellow);

            if (Physics.Raycast(thingToLookFrom.transform.position, thingToLookFrom.transform.forward, out hit, lookAtDistance))
            {
               // Debug.Log("Raycast hit: " + hit.transform.name);
            }
        }

        animator.SetFloat("Speed", transform.InverseTransformDirection(controller.velocity).z);
        animator.SetBool("IsGrounded", controller.isGrounded);

        if (health <= 0)
        {
            SceneManager.LoadScene("GameOver");
            health = 0;
        }

        if (healthText)
        {
            healthText.text = health.ToString();
        }

        if (health >= 100)
        {
            health = 100;
        }
    }

    public void fire()
    {
        Debug.Log("Pew Pew");

        animator.ResetTrigger("IsFiring");
       

        if(projectileSpawnPoint && projectilePrefab)
        {
            // Make projectile
            Rigidbody temp = Instantiate(projectilePrefab, projectileSpawnPoint.position,
                projectileSpawnPoint.rotation);

            // Shoot projectile
            temp.AddForce(projectileSpawnPoint.forward * projectileForce, ForceMode.Impulse);

            // Destroy projectile after 2.0 seconds
            Destroy(temp.gameObject, 2.0f);
        }
    }

    private void OnTriggerStay (Collider other)
    {
        if (other.gameObject.CompareTag("Water"))
        {
            if (isDamagePlayer == false)
            StartCoroutine(DamagePlayer(1, 100));
        }

       else if (other.gameObject.CompareTag("Gas"))
        {
                if (isDamagePlayer == false)
                StartCoroutine(DamagePlayer(1, 5));
        }

        else if (other.gameObject.CompareTag("SandPit"))
        {
            speed = 3f;
            jumpSpeed = 3f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("SandPit"))
        {
            speed = 6f;
            jumpSpeed = 5f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("End Point"))
        {
            SceneManager.LoadScene("GameOver");
        }

        else if (other.gameObject.CompareTag("HealthPotion"))
        {
            health += 20f;
          
        }
    }

    IEnumerator DamagePlayer(float wait, float _health)
    {
        isDamagePlayer = true;
        health -= _health;
        yield return new WaitForSeconds(wait);

        isDamagePlayer = false;
    }


    // Adds a menu option to reset stats
    [ContextMenu("Reset Stats")]
    void ResetStats()
    {
        //Debug.Log("Perform operation");
        speed = 6.0f;
    }
}
