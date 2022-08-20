using UnityEngine;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    [SerializeField]
    private List<Vector3> _movePositions;

    [SerializeField]
    private float _moveTime;

    private float currentMovePosition;
    private float moveSpeed;

    private bool canMove;
    private bool canShoot;

    [SerializeField]
    private AudioClip _moveClip, _pointClip, _scoreClip, _loseClip;

    [SerializeField]
    private GameObject _explosionPrefab;

    private void Awake()
    {
        currentMovePosition = 0f;
        canMove = false;
        canShoot = false;
        moveSpeed = 1 / _moveTime;
    }

    private void OnEnable()
    {
        GameManager.Instance.GameStarted += GameStarted;
    }

    private void OnDisable()
    {
        GameManager.Instance.GameStarted -= GameStarted;
    }

    private void GameStarted()
    {
        canShoot = true;
        canMove = true;
    }

    private void Update()
    {
        if(canShoot && Input.GetMouseButtonDown(0))
        {
            moveSpeed *= -1f;
            AudioManager.Instance.PlaySound(_moveClip);
        }
    }

    private int startIndex, endIndex;
    private float moveDistance;
    private Vector3 startPos, endPos;

    private void FixedUpdate()
    {
        if (!canMove) return;

        currentMovePosition += moveSpeed * Time.fixedDeltaTime;

        if(currentMovePosition < 0f)
        {
            currentMovePosition = _movePositions.Count;
        }

        if(currentMovePosition > _movePositions.Count)
        {
            currentMovePosition = 0f;
        }

        startIndex = Mathf.FloorToInt(currentMovePosition) % _movePositions.Count;
        endIndex = (startIndex + 1) % _movePositions.Count;
        moveDistance = (currentMovePosition % _movePositions.Count) - startIndex;

        startPos = _movePositions[startIndex];
        endPos = _movePositions[endIndex];

        transform.position = startPos + moveDistance * (endPos - startPos);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag(Constants.Tags.SCORE))
        {
            if(collision.gameObject.GetComponent<Score>().currentId == 
                GameManager.Instance.currentTargetIndex)
            {
                GameManager.Instance.UpdateScore();
                AudioManager.Instance.PlaySound(_scoreClip);
            }
            else
            {
                AudioManager.Instance.PlaySound(_pointClip);
            }
        }

        if(collision.CompareTag(Constants.Tags.OBSTACLE))
        {
            Destroy(Instantiate(_explosionPrefab, transform.position, Quaternion.identity), 3f);
            AudioManager.Instance.PlaySound(_loseClip);
            GameManager.Instance.EndGame();
            Destroy(gameObject);
        }
    }
}