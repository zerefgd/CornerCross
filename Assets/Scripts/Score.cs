using UnityEngine;

public class Score : MonoBehaviour
{
    public int currentId;

    [SerializeField]
    private Color _activeColor, _inactiveColor;

    [SerializeField]
    private SpriteRenderer _sr,_sr1,_sr2;

    private Color currentColor;

    private void OnEnable()
    {
        GameManager.UpdateScoreColor += OnTargetSet;
    }

    private void OnDisable()
    {
        GameManager.UpdateScoreColor -= OnTargetSet;
    }

    private void OnTargetSet(int targetId)
    {
        currentColor = targetId == currentId ? _activeColor : _inactiveColor;
        _sr.color = _sr1.color = _sr2.color = currentColor;
    }
}
