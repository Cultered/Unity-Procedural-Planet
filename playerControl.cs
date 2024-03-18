using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class playerControl : MonoBehaviour
{
    Vector3 velocityCtrl = Vector3.zero;
    Vector3 gravityCenter=new Vector3(0,0,0);
    bool isCollision=false;
    Rigidbody r;
    // Start is called before the first frame update
    void Start()
    {
        r = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision){
        isCollision = true;
    }

    void OnCollisionExit(Collision collision){
        isCollision = false;
    }

    void Update()
    {

        //recalculate player's rotation
        transform.up = (transform.position-gravityCenter).normalized;

        //player motion
        if (Input.GetKeyDown("space")&&isCollision)
        {
            r.AddForce(transform.up * 1000);
        }
        if (Input.GetKey("w"))
        {
            r.AddForce(transform.forward);
        }
        if (Input.GetKey("s"))
        {
            r.AddForce(-transform.forward);
        }if (Input.GetKey("d"))
        {
            r.AddForce(transform.right);
        }if (Input.GetKey("a"))
        {
            r.AddForce(-transform.right);
        }
        //gravity
        if (!isCollision)
        {   
            r.AddForce(-transform.up);
        }
    }
}
