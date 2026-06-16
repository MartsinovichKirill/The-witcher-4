Shader "WitcherRightVersion/VertexColorLit"
{
    // Built-in pipeline lit shader that renders the mesh's baked VERTEX COLORS.
    // Quaternius / OpenGameArt characters store their colours in vertices (no textures);
    // the Standard shader ignores them and renders white, so this shader is used to make
    // those models show their real skin/cloth/armour colours. _Color is a subtle tint.
    Properties
    {
        _Color ("Tint", Color) = (1,1,1,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.12
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert
        #pragma target 3.0

        struct Input
        {
            float4 vertexColor : COLOR;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.vertexColor = v.color;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = IN.vertexColor * _Color;
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = 1.0;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
