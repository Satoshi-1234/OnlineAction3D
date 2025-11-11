using UnityEngine;

public class WaterAnimationSettings : MonoBehaviour
{
    [SerializeField, Range(1.0f, 100.0f)] private float m_tilling = 12.0f;
    [SerializeField, Range(0.0f, 100.0f)] private float m_waterSpeed = 0.17f;
    [SerializeField, Range(1.0f, 100.0f)] private float m_waterSpeedFactor = 20.0f;
    [SerializeField] private Vector2 m_direction = new Vector2(-0.25f, 1.5f);
    [SerializeField] private float m_secondaryDirection = -0.1f;
    [SerializeField] private MeshRenderer m_waterMesh;
    private string m_tillingProperty = "_Tilling";
    private string m_speedProperty = "_Speed";
    private string m_speedFactorProperty = "_Speed_Factor";
    private string m_directionProperty = "_Direction";
    private string m_secondaryDirectionProperty = "_Second_Direction_Factor";
    private Material m_material;

    void Start()
    {
        if (m_waterMesh == null)
        {
            m_waterMesh = GetComponent<MeshRenderer>();
        }

        m_material = m_waterMesh.material;

        if (HasProperty(m_tillingProperty))
        {
            m_material.SetFloat(m_tillingProperty, m_tilling);
        }
        if (HasProperty(m_speedProperty))
        {
            m_material.SetFloat(m_speedProperty, m_waterSpeed);
        }
        if (HasProperty(m_speedFactorProperty))
        {
            m_material.SetFloat(m_speedFactorProperty, m_waterSpeedFactor);
        }
        if (HasProperty(m_directionProperty))
        {
            m_material.SetVector(m_directionProperty, m_direction);
        }
        if (HasProperty(m_secondaryDirectionProperty))
        {
            m_material.SetFloat(m_secondaryDirectionProperty, m_secondaryDirection);
        }
    }

    bool HasProperty(string propertyName)
    {
        bool result = m_material.HasProperty(propertyName);

        if (result == false)
        {
            Debug.LogError("指定されたマテリアルプロパティ名は存在しません。: " + propertyName);
        }
        return result;
    }
}
