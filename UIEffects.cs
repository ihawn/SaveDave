using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIEffects : MonoBehaviour
{
    public bool randomStartScale, randomStartLoc, randomMoveSpeed, randomScaleSpeed;
    public float startScaleMin, startScaleMax;
    public Vector3 startOffsetMin, startOffsetMax;
    public float scaleSpeedMin, scaleSpeedMax;
    public float moveSpeedMin, moveSpeedMax;

    float startScale, moveSpeed, scaleSpeed;
    Vector2 startOffset;
    Vector3 endPos, endScale;

    void OnEnable()
    {

        endPos = transform.position;

        if (randomStartScale)
            startScale = Random.Range(startScaleMin, startScaleMax);
        else
            startScale = startScaleMax;

        if (randomStartLoc)
            transform.position += new Vector3(Random.Range(startOffsetMin.x, startOffsetMax.x), Random.Range(startOffsetMin.y, startOffsetMax.y), 0f);
        else
            transform.position += startOffsetMax;

        if (randomMoveSpeed)
            moveSpeed = Random.Range(moveSpeedMin, moveSpeedMax);
        else
            moveSpeed = moveSpeedMax;

        if (randomScaleSpeed)
            scaleSpeed = Random.Range(scaleSpeedMin, scaleSpeedMax);
        else
            scaleSpeed = scaleSpeedMax;

        endScale = transform.localScale;
        transform.localScale = Vector3.one * startScale;

        StartCoroutine(Scale());
        StartCoroutine(Move());
    }

    IEnumerator Scale()
    {
        while(transform.localScale.x < 1)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, endScale, scaleSpeed * Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator Move()
    {
        while(Vector3.Distance(transform.position, endPos) > 0.0001f)
        {
            transform.position = Vector3.Lerp(transform.position, endPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }

}
