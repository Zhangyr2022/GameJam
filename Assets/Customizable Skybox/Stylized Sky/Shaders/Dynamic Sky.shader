Shader "Stylized/DynamicSky"
{
    Properties
    {
        [Header(Control)]
        _Factor ("Factor", Range(0, 1)) = 0

        [Header(Sun Disc)]
        _SunDiscColor1 ("Color", Color) = (1, 1, 1, 1)
        _SunDiscMultiplier1 ("Multiplier", float) = 100000
        _SunDiscExponent1 ("Exponent", float) = 500000

        [Header(Sun Halo)]
        _SunHaloColor1 ("Color", Color) = (0.8970588, 0.7760561, 0.6661981, 1)
        _SunHaloExponent1 ("Exponent", float) = 500
        _SunHaloContribution1 ("Contribution", Range(0, 1)) = 0.5

        [Header(Horizon Line)]
        _HorizonLineColor1 ("Color", Color) = (0.9044118, 0.8872592, 0.7913603, 1)
        _HorizonLineExponent1 ("Exponent", float) = 5.75
        _HorizonLineContribution1 ("Contribution", Range(0, 1)) = 0.3
        
        [Header(Sky Gradient)]
        _SkyGradientTop1 ("Top", Color) = (0.172549, 0.5686274, 0.6941177, 1)
        _SkyGradientBottom1 ("Bottom", Color) = (0.764706, 0.8156863, 0.8509805)
        _SkyGradientExponent1 ("Exponent", float) = 0.1
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Background"
            "Queue" = "Background"
        }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            
            float _Factor;

            float3 _SunDiscColor1;
            float _SunDiscExponent1;
            float _SunDiscMultiplier1;

            float3 _SunHaloColor1;
            float _SunHaloExponent1;
            float _SunHaloContribution1;

            float3 _HorizonLineColor1;
            float _HorizonLineExponent1;
            float _HorizonLineContribution1;

            float3 _SkyGradientTop1;
            float3 _SkyGradientBottom1;
            float _SkyGradientExponent1;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPosition : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPosition = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 _SunDiscColor2 = {1,1,1};
                float _SunDiscExponent2 = 100000;
                float _SunDiscMultiplier2 = 100000;

                float3 _SunHaloColor2 = {0.7735849f,0.69112f,0.6239765f};
                float _SunHaloExponent2 = 100;
                float _SunHaloContribution2 = 0.5;

                float3 _HorizonLineColor2 = {0,0,0};
                float _HorizonLineExponent2 = 1;
                float _HorizonLineContribution2 = 1;

                float3 _SkyGradientTop2 = {0.9019608,0.7882354,0.5843138};
                float3 _SkyGradientBottom2 = {0.9607844,0.9019608,0.7725491};
                float _SkyGradientExponent2 = 1;

                float3 _SunDiscColor = lerp(_SunDiscColor1,_SunDiscColor2,_Factor);
                float _SunDiscExponent = lerp(_SunDiscExponent1,_SunDiscExponent2,_Factor);
                float _SunDiscMultiplier = lerp(_SunDiscMultiplier1,_SunDiscMultiplier2,_Factor);

                float3 _SunHaloColor = lerp(_SunHaloColor1,_SunHaloColor2,_Factor);
                float _SunHaloExponent = lerp(_SunHaloExponent1,_SunHaloExponent2,_Factor);
                float _SunHaloContribution = lerp(_SunHaloContribution1,_SunHaloContribution2,_Factor);

                float3 _HorizonLineColor = lerp(_HorizonLineColor1,_HorizonLineColor2,_Factor);
                float _HorizonLineExponent = lerp(_HorizonLineExponent1,_HorizonLineExponent2,_Factor);
                float _HorizonLineContribution = lerp(_HorizonLineContribution1,_HorizonLineContribution2,_Factor);

                float3 _SkyGradientTop = lerp(_SkyGradientTop1,_SkyGradientTop2,_Factor);
                float3 _SkyGradientBottom = lerp(_SkyGradientBottom1,_SkyGradientBottom2,_Factor);
                float _SkyGradientExponent = lerp(_SkyGradientExponent1,_SkyGradientExponent2,_Factor);

                // Masks.
                float maskHorizon = dot(normalize(i.worldPosition), float3(0, 1, 0));
                float maskSunDir = dot(normalize(i.worldPosition), _WorldSpaceLightPos0.xyz);

                // Sun disc.
                float maskSun = pow(saturate(maskSunDir), _SunDiscExponent);
                maskSun = saturate(maskSun * _SunDiscMultiplier);

                // Sun halo.
                float3 sunHaloColor = _SunHaloColor * _SunHaloContribution;
                float bellCurve = pow(saturate(maskSunDir), _SunHaloExponent * saturate(abs(maskHorizon)));
                float horizonSoften = 1 - pow(1 - saturate(maskHorizon), 50);
                sunHaloColor *= saturate(bellCurve * horizonSoften);

                // Horizon line.
                float3 horizonLineColor = _HorizonLineColor * saturate(pow(1 - abs(maskHorizon), _HorizonLineExponent));
                horizonLineColor = lerp(0, horizonLineColor, _HorizonLineContribution);

                // Sky gradient.
                float3 skyGradientColor = lerp(_SkyGradientTop, _SkyGradientBottom, pow(1 - saturate(maskHorizon), _SkyGradientExponent));

                float3 finalColor = lerp(saturate(sunHaloColor + horizonLineColor + skyGradientColor), _SunDiscColor, maskSun);
                return float4(finalColor, 1);
            }
            ENDCG
        }
    }
}
