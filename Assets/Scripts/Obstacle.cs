using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [SerializeField]
    private float _moveSpeed, _maxOffset;

    private void FixedUpdate()
    {
        transform.position += _moveSpeed * Time.fixedDeltaTime * Vector3.left;

        if(transform.position.x < _maxOffset)
        {
            Destroy(gameObject);
        }
    }
}
