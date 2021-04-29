using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DaveController : MonoBehaviour
{
    private AudioManager theAudioManager;
    public float speedToThud, rbSpeed;
    Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        theAudioManager = FindObjectOfType<AudioManager>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

        rbSpeed = rb.velocity.y;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(rbSpeed <= -speedToThud && (collision.gameObject.tag == "ground" || collision.gameObject.tag == "stackPiece"))
        {
            if (!theAudioManager.thud.isPlaying)
            {
                theAudioManager.thud.Play();
                theAudioManager.thud.volume = theAudioManager.globalEffectVolume;
                theAudioManager.thud.pitch = Random.Range(0.85f, 1.15f);
            }
        }


    }
}
