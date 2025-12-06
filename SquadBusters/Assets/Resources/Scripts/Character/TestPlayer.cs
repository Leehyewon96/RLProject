using DG.Tweening;
using System.Collections;
using UnityEngine;

public class TestPlayer : MonoBehaviour
{
    private Animator animator = null;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
    }

    private void Jump()
    {
        animator.SetTrigger("Jump");
        StartCoroutine(CoMove());
    }

    IEnumerator CoMove()
    {
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Jump"));
        transform.DOMove(transform.position + transform.forward.normalized * 5f, 0.5f);
    }
}
