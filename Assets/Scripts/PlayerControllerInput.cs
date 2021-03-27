using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerControllerInput : MonoBehaviour
{
    [Header("Movement Information")]
    public float InputX; // Range between -1 and 1 for horizontal axis (sides).
    public float InputZ; // Range between -1 and 1 for vertical axis (forward).
    public Vector3 desiredMoveDirection;

    [Header("Thresholds of movement")]
    public float allowPlayerRotation; // When we want the player to start rotating.
    public float Speed;

    [Header("Rotation Information")]
    public bool blockRotationPlayer; // For static animation forward.
    public float desiredRotationSpeed; 
    [Range(1, 10)]
    public float increaseSpeed = 1.0f;
    public float maxSpeed = 1.5f;

    [Header("Activate autonomous agent")]
    public bool enableSmartAgent = false;
    public bool walkMotion = false;
    public bool runMotion = false;
    private Vector2 autTargetPos = Vector2.zero;
    private Vector2 direction = Vector2.zero;

    private Camera cam;
    private Animator anim;
    private CustomTerrain terrain;
    private Vector3 lastpos;
    private PlayerControllerIKFeetPlacementTask ikcomp;
    private float wmagn = 1f;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        cam = Camera.main;
        terrain = FindObjectOfType<CustomTerrain>();
        lastpos = transform.localPosition;
        ikcomp = anim.gameObject.GetComponent<PlayerControllerIKFeetPlacementTask>();

    }

    // Update is called once per frame
    void Update()
    {
        InputMagnitude();
    }

    /// <summary>
    /// Method to calculate input vectors.
    /// </summary>
    void InputMagnitude()
    {
        InputX = Input.GetAxis("Horizontal");
        InputZ = Input.GetAxis("Vertical");
        Vector2 d = checkAutonomousAgent();
        if(d != Vector2.zero){
            InputX = d.x;
            InputZ = d.y;
        } else{
            // Calculate Input Vectors

            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                anim.SetBool("Walk",true);
            else
                anim.SetBool("Walk",false);
            
            if (Input.GetKey(KeyCode.Space))
                anim.SetBool("Jump",true);
            else
                anim.SetBool("Jump",false);

            if (Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift)){
                increaseSpeed += 0.01f;
                if(increaseSpeed>maxSpeed)
                    increaseSpeed = maxSpeed;
                anim.SetFloat("IntensitySpeed",increaseSpeed);
            }
            if (Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.LeftAlt)){
                increaseSpeed -= 0.01f;
                if(increaseSpeed<1f)
                    increaseSpeed = 1f;
                anim.SetFloat("IntensitySpeed",increaseSpeed);
            }
            
        }
    if(wmagn <= 0.03f){
            InputX *= -1f;
            InputZ *= -1f;
            wmagn +=0.2f;
    }
        Speed = new Vector2(InputX, InputZ).normalized.sqrMagnitude * wmagn;

        // Set Floats into the animator equal to the inputs.
        anim.SetFloat("InputZ", InputZ, 0.0f, Time.deltaTime);
        anim.SetFloat("InputX", InputX, 0.0f, Time.deltaTime);

        // Calculate InputMagnitude (Blend (Speed)) - Better for controller axis - How much you are pressing the key or joystick.
        // Calculate the squared magnitude instead of the magnitude is much faster (it doesn´t need to do the square root).
        anim.SetFloat("InputMagnitude", Speed, 0.0f, Time.deltaTime);
        Speed = Speed * anim.GetFloat("IntensitySpeed");

        
        
        // Physically move player w/ animations
        if (Speed > allowPlayerRotation)
        {
            PlayerMoveAndRotation(ref d);
        }
        lastpos = transform.localPosition;
    }

    /// <summary>
    /// Method in charge of moving the player.
    /// </summary>
    void PlayerMoveAndRotation(ref Vector2 autv)
    {
        InputX = Input.GetAxis("Horizontal");
        InputZ = Input.GetAxis("Vertical");
        if(autv != Vector2.zero){
            InputX = autv.x;
            InputZ = autv.y;
        }

        // Set normalized unit vectors for the camera - var could replace Vector3
        // Vector3.right is a vector facing the world right. It will always be (1, 0, 0)
        // transform.right is a vector facing the local-space right, meaning it is a vector that faces to the right of your object.
        // This vector will be different depending on which way your object with the transform is facing.
        Vector3 forward = cam.transform.forward; // Z direction with respect to the global axis.
        Vector3 right = cam.transform.right; // X direction with respect to the global axis.

        // Convert to 2D planar coordinates and normalize.
        
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        // forward and right are normalized vectors.
        desiredMoveDirection = forward * InputZ + right * InputX;

        // Rotates the character between the current angle (transform.rotation) to the forward direction of the vector in which you are moving according to InputX and InputZ.
        // It can be modified to certain time or speed.
        if(blockRotationPlayer == false)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desiredMoveDirection), desiredRotationSpeed);
        }
    }

    protected Vector2 checkAutonomousAgent(){
        if(enableSmartAgent && (walkMotion || runMotion)){
            bool w = walkMotion && !runMotion;
            System.Random rand = new System.Random();
            w = walkMotion && !runMotion;
            walkMotion = w;
            runMotion = !w;
                Vector2 diff = autTargetPos - new Vector2(anim.transform.localPosition.x,anim.transform.localPosition.z);
                if(diff.sqrMagnitude < 1f){ 
                     diff = new Vector2((float)rand.NextDouble()*2f-1f,(float)rand.NextDouble()*2f-1f)*5f;
                    autTargetPos = new Vector2(anim.transform.localPosition.x,anim.transform.localPosition.z) + diff;
                }
                diff.Normalize();
                float rate = 0f;
                if(diff == Vector2.zero){
                    direction = Vector2.zero;
                    return Vector2.zero;
                }
                if(diff.x!=0f){
                    rate = diff.y/diff.x;
                    direction = new Vector2(diff.y/rate,diff.x*rate);
                }else{
                    rate = diff.x/diff.y;
                    direction = new Vector2(diff.y*rate,diff.x/rate);
                }
            print(" "+direction + " "+anim.transform.localPosition+" "+autTargetPos);
            return w ? direction*0.6f  : direction;
        }
        autTargetPos = new Vector2(anim.transform.localPosition.x,anim.transform.localPosition.z);
        direction = Vector2.zero;
        return Vector2.zero;
    }
    void OnControllerColliderHit(ControllerColliderHit hit) {
        float angle = Vector3.Angle(Vector3.up, hit.normal);
        if(angle >= 45){
            angle =45f;
        }
        angle = (angle / 45f);
        wmagn =  (1f-angle);
        RaycastHit hit2;
        float slope = 0f;
        slope = Vector3.Dot(anim.transform.right, (Vector3.Cross(Vector3.up, hit.normal)));
        if(slope >=0f){
            if(wmagn >= 0.8)
                wmagn =1f;
            else
                wmagn+=0.2f;
        }
    }
    /*void OnCollisionEnter2D(Collision2D other)
    {
        bool rightHit = Physics2D.OverlapCircle(transformer.position, 2f, wallLayer);
        bool leftHit = Physics2D.OverlapCircle(transformer.position, 2f, wallLayer);
        
        if(rightHit) Debug.Log("Hit on right");
        if(leftHit) Debug.Log("Hit on right");
    }*/

}
