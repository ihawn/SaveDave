using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipController : MonoBehaviour
{
    private LevelController theLevelController;
    public float shipOffset;

    public bool flyAway;
    public float speed;

    // Start is called before the first frame update
    void Start()
    {
        theLevelController = FindObjectOfType<LevelController>();

        transform.position = new Vector3(transform.position.x, theLevelController.levelRequirements[theLevelController.levelRequirements.Length - 1]/2 + shipOffset, transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        if(flyAway)
        {
            transform.Translate(speed * Time.deltaTime * new Vector3(0, 0, 1));
        }
    }
}
