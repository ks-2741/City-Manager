using UnityEngine;

public class PlacementGhost : MonoBehaviour
{
    [SerializeField] private Color _validColor = new Color(0f, 1f, 0f, 0.5f);
    [SerializeField] private Color _invalidColor = new Color(1f, 0f, 0f, 0.5f);

    private Renderer[] _renderers;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>();
    }

    public void SetValid(bool isValid)
    {
        Color c = isValid ? _validColor : _invalidColor;
        foreach (var r in _renderers)
            r.material.color = c;
    }
}