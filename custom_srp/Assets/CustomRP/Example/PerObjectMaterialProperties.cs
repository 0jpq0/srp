using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerObjectMaterialProperties : MonoBehaviour
{
    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    static MaterialPropertyBlock s_block;

    [SerializeField]
    Color m_baseColor = Color.white;

    void Awake()
    {
        OnValidate();
    }

    void OnValidate()
    {
        if (s_block == null)
            s_block = new MaterialPropertyBlock();

        s_block.SetColor(BaseColorId,m_baseColor);
        GetComponent<Renderer>().SetPropertyBlock(s_block);
    }

}
