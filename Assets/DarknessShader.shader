//This shader was inspired by this video: https://www.youtube.com/shorts/WacXbEP6iv8 and a lot of shader documentation
//https://docs.unity3d.com/Manual/SL-Reference.html
//https://docs.unity3d.com/Manual/SL-Shader.html
//http://docs.unity3d.com/Manual/SL-Properties.html
//I also got debugging help from my wonderful boyfriend Lawrence Feng

Shader "Custom/DarknessOverlay"
{
    Properties
    {
        _PlayerPos ("Player Position", Vector) = (0, 0, 0, 0)
        _LightRadius ("Light Radius", Float) = 5.0
        _DarknessColor ("Darkness Color", Color) = (0, 0, 0, 1)
        _EdgeSoftness ("Edge Softness", Range(0, 2)) = 0.5
    }
    
    SubShader
    {
        Tags 
        { 
            "Queue"="Overlay" 
            "RenderType"="Transparent" 
            "IgnoreProjector"="True"
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };
            
            float3 _PlayerPos;
            float _LightRadius;
            fixed4 _DarknessColor;
            float _EdgeSoftness;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Calculate distance from pixel to player
                float2 pixelPos = i.worldPos.xy;
                float2 playerPos = _PlayerPos.xy;
                float distance = length(pixelPos - playerPos);
                
                // falloff
                float alpha = clamp(distance / _LightRadius, 0.0, 1.0);
                
                // Return darkness color with calculated alpha
                return fixed4(_DarknessColor.rgb, alpha * _DarknessColor.a);
            }
            ENDCG
        }
    }
}