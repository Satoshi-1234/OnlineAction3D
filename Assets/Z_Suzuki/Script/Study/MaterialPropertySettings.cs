using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class MaterialPropertySettings : MonoBehaviour
{
    [SerializeField] private MeshRenderer m_waterMesh;
    [SerializeField] private Dictionary<string, float> m_materialPropertys;
    private Material m_material;

    void Start()
    {
        if (m_waterMesh == null)
        {
            m_waterMesh = GetComponent<MeshRenderer>();
        }

        m_material = m_waterMesh.material;

        //foreach (KeyValuePair<string, float> in m_materialPropertys)
        //{
        //    if (HasProperty(pair.Key))
        //    {
        //        m_material.SetFloat(pair.Key, pair.Value);
        //    }
        //}

        //if (HasProperty(m_tillingProperty))
        //{
        //    m_material.SetFloat(m_tillingProperty, m_tilling);
        //}
        //if (HasProperty(m_speedProperty))
        //{
        //    m_material.SetFloat(m_speedProperty, m_waterSpeed);
        //}
        //if (HasProperty(m_speedFactorProperty))
        //{
        //    m_material.SetFloat(m_speedFactorProperty, m_waterSpeedFactor);
        //}
        //if (HasProperty(m_directionProperty))
        //{
        //    m_material.SetVector(m_directionProperty, m_direction);
        //}
        //if (HasProperty(m_secondaryDirectionProperty))
        //{
        //    m_material.SetFloat(m_secondaryDirectionProperty, m_secondaryDirection);
        //}
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
