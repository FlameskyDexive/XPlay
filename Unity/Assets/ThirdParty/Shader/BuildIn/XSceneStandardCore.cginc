// Upgrade NOTE: replaced 'defined FOG_COMBINED_WITH_WORLD_POS' with 'defined (FOG_COMBINED_WITH_WORLD_POS)'
// Upgrade NOTE: excluded shader from DX11 because it uses wrong array syntax (type[size] name)
#if defined(SCENESTANDARDCORE)
#pragma exclude_renderers d3d11

#ifndef XSCENESTANDARDCORE_INCLUDED
#define XSCENESTANDARDCORE_INCLUDED

#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"
#include "UnityShaderVariables.cginc"
#include "UnityShaderUtilities.cginc"
#include "TerrainEngine.cginc"
// Upgrade NOTE: replaced 'defined FOG_COMBINED_WITH_WORLD_POS' with 'defined (FOG_COMBINED_WITH_WORLD_POS)'

#ifndef XRLINEFOG_INCLUDED
#define XRLINEFOG_INCLUDED

#define XRLINEFOG_PROPS	\
uniform fixed _GodFogEnabled;\
uniform float4 _GodFogColor;\
uniform sampler2D _GodTexture;\
uniform float4x4 _GodMatrixVP;
uniform float4 _GodFogTragetPos;\
uniform float4 _GodFogTargetIntensity;\
uniform float _GodFogIntensity;


#define XRLINEFOG_COLOR(col)	\
float4 worldpos = i.worldPos;	\
float4 computeGrabScreenPos9 = ComputeGrabScreenPos( mul( _GodMatrixVP, worldpos ) );\
float4 temp_output_20_0 = ( computeGrabScreenPos9 / (computeGrabScreenPos9).w );\
float4 appendResult33 = (float4((temp_output_20_0).x , ( 1.0 - (temp_output_20_0).y ) , 0.0 , 0.0));\
float fogfac51 = ( 1.0 - (tex2D( _GodTexture, appendResult33.xy )).r );\
float raidLerp87 = ( max( 0.0 , ( _GodFogTragetPos.w - distance( worldpos , float4( (_GodFogTragetPos).xyz , 0.0 ) ) ) ) / _GodFogTragetPos.w );\
col.rgb = lerp(col.rgb, _GodFogColor.rgb , _GodFogEnabled * ( saturate( ( fogfac51 * ( 1.0 - ( saturate( ( raidLerp87 * _GodFogTargetIntensity.x ) ) * _GodFogTargetIntensity.y ) ) ) ) * _GodFogIntensity ));
				


#endif // XSCENESTANDARDCORE_INCLUDED


#define X_GLOSS_NORMAL_B

struct appdata
{
	float4 vertex       : POSITION;
	float4 tangent      : TANGENT;
	float3 normal       : NORMAL;
	float2 texcoord     : TEXCOORD0;
	float2 texcoord1    : TEXCOORD1;
	float2 texcoord2    : TEXCOORD2;
	fixed4 color : COLOR;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
	UNITY_POSITION(pos);
	float4 texcoord             : TEXCOORD0;
	float4 tSpace0              : TEXCOORD1;
	float4 tSpace1              : TEXCOORD2;
	float4 tSpace2              : TEXCOORD3;
	float4 ambientOrLightmapUV  : TEXCOORD4;
	float  fogCoordH : TEXCOORD5;
	float4 worldPos : TEXCOORD6;
#if !defined (UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS)
	UNITY_SHADOW_COORDS(7)
	UNITY_FOG_COORDS(8)

#if defined(XWEATHER_WATER) || defined(XWEATHER_SNOW)
		float4  weather_parameter  : TEXCOORD12;
#endif
#else
	UNITY_LIGHTING_COORDS(7, 8)
	UNITY_FOG_COORDS(9)
#if defined(XWEATHER_WATER) || defined(XWEATHER_SNOW)
		float4  weather_parameter  : TEXCOORD12;
#endif
#endif
	float3 test					: TEXCOORD13;

	UNITY_VERTEX_INPUT_INSTANCE_ID
};


fixed _AlphaCtl;
sampler2D _MainTex;
float4 _MainTex_ST;
sampler2D _BumpMap;
float4 _BumpMap_ST;
sampler2D _BumpMapRGloss;
float4 _BumpMapRGloss_ST;
sampler2D _GlossMap;
sampler2D _ReflectMap;
samplerCUBE _ReflectCubeMap;
#ifdef  x_LIGHT_MODE_TEXTURE
fixed4 _TextureModeFilter;
#endif

fixed4 _Color;
fixed4 _ReflectColor;
half _Shininess;
fixed4 _LightDir;
fixed3 _LightSpeed;
fixed _Fog;
fixed _lFogPower;
#ifdef  XHIGHT_FOG
fixed4  _HFogColor;
float   _HFogStart;
float   _HFogEnd;
#endif

#ifdef XENV
fixed4 _XEnvColor;
#endif

#ifdef XEMISSION
fixed4 _EmissionColor;
#endif



#if defined(XWEATHER_WATER)
float4 _WaterSpeed;
sampler2D _WaterBump;
float4 _WaterBump_ST;
half _WaterNormalPower;
fixed4 _WaterColor;
half _WaterMask;

sampler2D _DropletsMap;
float4 _DropletsMap_ST;
half4 _DropletsPar;
samplerCUBE _WaterReflectCubeMap;
#endif

#if defined(XWEATHER_WATER) ||  defined(XWEATHER_SNOW)
half X_GLOBAL_XWEATHER_FADE = 0;
#endif

#if defined(XWEATHER_SNOW)
sampler2D _SnowAlbedo;
float4 _SnowAlbedo_ST;
sampler2D _SnowNormal;
half _SnowNormalPower;
half  _SnowAmount;
half _SnowColorPower;
half _SnowMask;
fixed _SnowThick;
#endif

#if defined(XWIND)
// float4  _Wind;
float   _WindEdgeFlutter;
float   _WindEdgeFlutterFreqScale;
#endif

#if defined(BLOOM)
sampler2D _BloomTex;
half _BloomIntensity;
fixed4 _BloomColor;
#endif


sampler2D _AlphaTex;
// [Enum(MainTex(A),0,Channel(R),1,Channel(G),2,Channel(B),3)] 
half _AlphaChannel;
fixed _EnabledAmbient;


float4 _XPOINT_LIGHT_POS0;
fixed4 _InteractiveDirection;
fixed3 _XPOINT_LIGHT_COLOR0;
fixed _DisabledXEnvColor;



XRLINEFOG_PROPS


UNITY_INSTANCING_BUFFER_START(Props)
//UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
UNITY_INSTANCING_BUFFER_END(Props)

#define PI UNITY_PI
inline float EaseOutQuad(float x)
{
	return 1 - (1 - x) * (1 - x);
}
inline float EaseInQuad(float x)
{
	return x * x;
}
inline float EaseOutQuint(float x)
{
	return 1 - pow(1 - x, 5);
}
inline float EaseInSine(float x)
{
	return 1 - cos((x * PI) / 2);
}

uniform float4 _InteractivePositions[6];
uniform float4 _InteractivePosition;
uniform float _InteractiveHeight;
uniform float _InteractiveUVPower;


inline float4 InteractiveVertex(float4 pos, float2 texcoord,float3 normal)
{
	float3 worldPos = mul(unity_ObjectToWorld, pos).xyz;
	worldPos.y = 0;

#if defined(INTERACTIVE)
	float posYAddition = 0;
	float interableValue = 0;
	for (int i = 0; i < 6; i++)
	{
		float3 interableWPos = _InteractivePositions[i].xyz;
		interableWPos.y = 0;
		float range = _InteractivePositions[i].w;
		float value = saturate(1 - (distance(worldPos, interableWPos) / range));
		interableValue += value;
	}
	interableValue = saturate(interableValue);
	interableValue = EaseInSine(interableValue);
	posYAddition = (EaseOutQuad(texcoord.y)) * interableValue;
	posYAddition = clamp(posYAddition, 0, _InteractiveHeight * pow(texcoord.y, _InteractiveUVPower));
	//pos.z -= posYAddition;
	pos.xyz -= _InteractiveDirection.xyz * posYAddition * _InteractiveDirection.w;
	return pos;
#else
	return pos; 
#endif
}

inline float4 InteractiveVertexTest(float4 pos, float2 texcoord, float3 normal)
{
	float3 worldPos = mul(unity_ObjectToWorld, pos).xyz;
	worldPos.y = 0;

#if defined(INTERACTIVE)
	float posYAddition = 0;
	float interableValue = 0;
	for (int i = 0; i < 6; i++)
	{
		float3 interableWPos = _InteractivePositions[i].xyz;
		interableWPos.y = 0;
		float range = _InteractivePositions[i].w;
		float value = saturate(1 - (distance(worldPos, interableWPos) / range));
		interableValue += value;
	}
	interableValue = saturate(interableValue);
	interableValue = EaseInSine(interableValue);
	posYAddition = (EaseOutQuad(texcoord.y)) * interableValue;
	return texcoord.y;
#else
	return 1;
#endif
}

inline float4 AnimateVertex_Wind(float4 pos, float3 normal, fixed4 color)
{

#if defined(XWIND)
	float4  wind;
	float           bendingFact = color.a;
	wind.xyz = mul((float3x3)unity_WorldToObject, _Wind.xyz);
	wind.w = _Wind.w  * bendingFact;
	float4  animParams = float4(0, _WindEdgeFlutter, bendingFact.xx);
	float   time = _Time.y * float2(_WindEdgeFlutterFreqScale, 1);

	// animParams stored in color
	// animParams.x = branch phase
	// animParams.y = edge flutter factor
	// animParams.z = primary factor
	// animParams.w = secondary factor

	float fDetailAmp = 0.1f;
	float fBranchAmp = 0.3f;

	// Phases (object, vertex, branch)
	float fObjPhase = dot(unity_ObjectToWorld[3].xyz, 1);
	float fBranchPhase = fObjPhase + animParams.x;

	float fVtxPhase = dot(pos.xyz, animParams.y + fBranchPhase);

	// x is used for edges; y is used for branches
	float2 vWavesIn = time + float2(fVtxPhase, fBranchPhase);

	// 1.975, 0.793, 0.375, 0.193 are good frequencies
	float4 vWaves = (frac(vWavesIn.xxyy * float4(1.975, 0.793, 0.375, 0.193)) * 2.0 - 1.0);

	vWaves = SmoothTriangleWave(vWaves);
	float2 vWavesSum = vWaves.xz + vWaves.yw;

	// Edge (xz) and branch bending (y)
	float3 bend = animParams.y * fDetailAmp * normal.xyz;
	bend.y = animParams.w * fBranchAmp;
	pos.xyz += ((vWavesSum.xyx * bend) + (wind.xyz * vWavesSum.y * animParams.w)) * wind.w;

	// Primary bending
	// Displace position
	pos.xyz += animParams.z * wind.xyz;

	return pos;
#else
	return pos;
#endif

}


inline fixed X_Alpha(fixed a, float2 uv)
{
	fixed3 alpahpkg = tex2D(_AlphaTex, uv);
	return lerp(lerp(lerp(a, alpahpkg.r, step(1, _AlphaChannel)), alpahpkg.g, step(2, _AlphaChannel)), alpahpkg.b, step(3, _AlphaChannel));
}


inline half3 X_UnpackScaleNormal(half4 packednormal, half bumpScale)
{
#if defined(UNITY_NO_DXT5nm)
	half3 normal;
	normal.xy = (packednormal.xy * 2 - 1);
#if (SHADER_TARGET >= 30)
	normal.xy *= bumpScale;
#endif
	normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));
	return normal;
#else
	packednormal.x *= packednormal.w;

	half3 normal;
	normal.xy = (packednormal.xy * 2 - 1);
#if (SHADER_TARGET >= 30)
	normal.xy *= bumpScale;
#endif
	normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));
	return normal;
#endif
}

inline float2 X_Ripplefunc(float2 uv)
{
	float2 droplets_refraction_offset = float2(0, 0);

#if defined(XWEATHER_WATER)
	float2 ripuv = uv * _DropletsPar.y;
	fixed4 Ripple = tex2D(_DropletsMap, ripuv);
	Ripple.xy = Ripple.xy * 2 - 1;
	float DropFrac = frac(Ripple.w + _Time.x*_DropletsPar.z);
	float TimeFrac = DropFrac - 1.0f + Ripple.z;
	float DropFactor = saturate(_DropletsPar.x - DropFrac);
	float FinalFactor = DropFactor * Ripple.z * sin(clamp(TimeFrac * 9.0f, 0.0f, 3.0f) * 3.1415);
	droplets_refraction_offset = Ripple.xy * FinalFactor;
#endif

	return droplets_refraction_offset;
}



inline fixed snowFac(half worldNormal_y)
{
	fixed fac = 0;
#if defined(XWEATHER_SNOW)
	// tex2D(_SnowNormal,)
	fac = saturate(worldNormal_y * _SnowAmount);
#endif
	return fac;
}


inline fixed4 XBlinnPhongLight(fixed4 Albedo, half3 worldNormal, half Specular, half Gloss, half3 worldViewDir, UnityGI gi)
{
	UnityLight light = gi.light;

	// return fixed4(light.color,1);

	light.dir = lerp(light.dir, _LightDir, _LightDir.w);
	light.dir += (_LightSpeed * (_SinTime.w *  1.5));

	half3 h = normalize(light.dir + worldViewDir);
	fixed diff = max(0, dot(worldNormal, light.dir));
	float nh = max(0, dot(worldNormal, h));
	float spec = pow(nh, Specular*128.0) * Gloss;
	fixed4 c;
	c.rgb = Albedo.rgb * light.color * diff + light.color * _SpecColor.rgb * spec;
	c.a = Albedo.a;
	return c;
}



inline float3 XPointLight(float3 lightpos, fixed3 lightColor0, float4 lightAttenSq, float3 pos, float3 normal)
{

	float3 tolight = lightpos - pos;

	float dis = length(tolight);

	float atten = (lightAttenSq - dis) / lightAttenSq;

	float ndotl = max(0, dot(normal, tolight));

	float diff = ndotl * atten;

	float3 col = 0;

	col += lightColor0 * diff;
	return col;
}

inline UnityGI XFragmentGI(fixed3 lightDir, float3 worldPos, float3 worldViewDir, float4 ambientOrLightmapUV, half3 worldNormal, half atten)
{

	UnityGI gi;
	UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
#if defined(x_LIGHT_NONE_TEXTURE)
	return gi;
#endif

	gi.indirect.diffuse = 0;
	gi.indirect.specular = 0;
	gi.light.color = _LightColor0.rgb;

	gi.light.dir = lightDir;

	UnityGIInput d;
	UNITY_INITIALIZE_OUTPUT(UnityGIInput, d);
	d.light = gi.light;
	d.worldPos = worldPos;
	d.atten = atten;
#if !defined(x_LIGHT_MODE_TEXTURE)
	d.worldViewDir = worldViewDir;
#endif
#if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
	d.ambient = 0;
	d.lightmapUV = ambientOrLightmapUV;
#else
	d.ambient = ambientOrLightmapUV.rgb;
	d.lightmapUV = 0;
#endif
	d.probeHDR[0] = unity_SpecCube0_HDR;
	d.probeHDR[1] = unity_SpecCube1_HDR;
#if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
	d.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
#endif
#ifdef UNITY_SPECCUBE_BOX_PROJECTION
	d.boxMax[0] = unity_SpecCube0_BoxMax;
	d.probePosition[0] = unity_SpecCube0_ProbePosition;
	d.boxMax[1] = unity_SpecCube1_BoxMax;
	d.boxMin[1] = unity_SpecCube1_BoxMin;
	d.probePosition[1] = unity_SpecCube1_ProbePosition;
#endif

	gi = UnityGlobalIllumination(d, 1.0, worldNormal);

	return gi;
}


inline fixed4 XFragmentfunc(float2 uv, fixed4 Albedo, half Specular, fixed gloss, float3 worldViewDir, half3 worldNormal, UnityGI gi, float2 snow_uv)
{

	fixed4 tex = Albedo;

	fixed snowV = 0;
	fixed wmask = 0;
#if defined(XWEATHER_SNOW)
	snowV = snowFac(worldNormal.y);
	fixed mask = tex2D(_SnowNormal, snow_uv).b;
	mask = saturate(mask * _SnowMask * X_GLOBAL_XWEATHER_FADE);

	tex.rgb = lerp(tex.rgb, tex2D(_SnowAlbedo, snow_uv) * _SnowColorPower, snowV * mask);

	fixed3 ogloss = gloss;
	gloss = lerp(ogloss, tex2D(_SnowAlbedo, snow_uv).a, _SnowMask);
	snowV = mask;
#elif defined(XWEATHER_WATER)
	fixed mask = tex2D(_WaterBump, uv).b;
	wmask = saturate(mask * _WaterMask * X_GLOBAL_XWEATHER_FADE);
	gloss = lerp(0, gloss, wmask);
#endif


	fixed4 c = 0;
#if defined(XTEXTURE_MULTIPLE)
	tex.rgb *= 2;
#endif

	fixed4 plight = 0;
#if defined(X_LIGHT_MODE_BLINNPHONG)
	plight = XBlinnPhongLight(tex, worldNormal, Specular, gloss, worldViewDir, gi);
	c += plight;
#elif defined(x_LIGHT_MODE_TEXTURE)
	fixed diff = max(0, dot(worldNormal, gi.light.dir));
	c.rgb += tex.rgb * gi.light.color * diff * _TextureModeFilter.rgb;
	c.a = tex.a;
#if !defined(LIGHTMAP_ON) && !defined(DYNAMICLIGHTMAP_ON)
	c.rgb *= 2;
#endif
#elif defined(x_LIGHT_NONE_TEXTURE)
	c += tex;
#endif

#if defined(UNITY_LIGHT_FUNCTION_APPLY_INDIRECT) && !defined(x_LIGHT_NONE_TEXTURE)
	//是否启用环境颜色
	c.rgb += lerp(tex.rgb * gi.indirect.diffuse, fixed3(0, 0, 0), _EnabledAmbient);
#endif

#if defined(XEMISSION)
	c.rgb += tex.rgb * _EmissionColor.rgb;
#endif

#if defined(XWEATHER_WATER)
	c.rgb *= lerp(fixed3(1, 1, 1), _WaterColor.rgb + plight, X_GLOBAL_XWEATHER_FADE);
#endif


#if defined(REFLECTMATCAP)
	fixed3 reflectcolor = tex2D(_ReflectMap, ((mul(UNITY_MATRIX_V, fixed4(worldNormal, 0.0)) * 0.5) + 0.5).xy).xyz;
#if defined(XWEATHER_WATER)
	c.rgb += reflectcolor * _ReflectColor.rgb * gloss * wmask;
#else
	c.rgb += reflectcolor * _ReflectColor.rgb * gloss;//lerp(c.rgb,c.rgb * reflectcolor * _ReflectColor.rgb * gloss ,_ReflectColor.a);
#endif
#endif

#if defined(REFLECTCUBEMAP)
	half3 worldRefl = reflect(-worldViewDir, worldNormal);
	fixed4 reflcol = 0;
#if defined(XWEATHER_WATER)
	reflcol = texCUBE(_WaterReflectCubeMap, worldRefl);
	c.rgb += reflcol.rgb * _ReflectColor.rgb * X_GLOBAL_XWEATHER_FADE;// * gloss;
#else
	reflcol = texCUBE(_ReflectCubeMap, worldRefl);
	c.rgb += reflcol.rgb * _ReflectColor.rgb * gloss;
#endif
#endif   

#ifdef XENV
	c.rgb *= lerp(lerp(fixed3(1, 1, 1), _XEnvColor.rgb, _XEnvColor.a), fixed3(1, 1, 1), _DisabledXEnvColor);
#endif

#ifdef BLOOM
	c.rgb = lerp(c.rgb, c.rgb * _BloomIntensity * _BloomColor.rgb, tex2D(_BloomTex, uv).r);
#endif   


#if defined(XPOINTLIGHT)
	c.rgb = tex.rgb + XPointLight(_XPOINT_LIGHT_POS0.xyz, _XPOINT_LIGHT_COLOR0.rgb, _XPOINT_LIGHT_POS0.w, worldPos, worldNormal);
#endif  

	//  #if defined(XWEATHER_WATER)
	//      half fresnel = dot( worldViewDir, worldNormal );
	//      half4 water = tex2D( _WaterColorControl, float2(fresnel,fresnel) );
	//      c.rgb += lerp( water.rgb, _WaterColor.rgb, water.a ) * _WaterColor.a * wmask;
	// #endif

	c.a = X_Alpha(c, uv);
	c.a *= _Color.a * _AlphaCtl;

	return c;
}

float4 _XTime;
//_XTime dimension definition
//x = realtimeSinceStartup / 20
//y = realtimeSinceStartup
//z = realtimeSinceStartup * 2
//w = realtimeSinceStartup * 3
float4 _FadeOutParam;  
//_FadeOutParam dimension definition
//x = realtimeSinceStartup
//y = x + animeTime
//z = origin alpha
//w = target alpha
inline float TransparentAnime()
{
    float param  = saturate((_FadeOutParam.y - _XTime.y)/(_FadeOutParam.y - _FadeOutParam.x));
    float targetAlpha = lerp(_FadeOutParam.z,_FadeOutParam.w,1 - param);
    
    return lerp(1,targetAlpha,step(0.001,_FadeOutParam.x));
}

v2f vert(appdata v)
{
	v2f o;
	UNITY_INITIALIZE_OUTPUT(v2f, o);
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_TRANSFER_INSTANCE_ID(v, o);

	//风
#if defined(XWIND)
	v.vertex = AnimateVertex_Wind(v.vertex, v.normal, v.color);
#endif


	o.texcoord.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
#if defined(x_NOUVTS)
	o.texcoord.xy = v.texcoord;
#else
	o.texcoord.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
#endif

	//交互(草,植物)
#if defined(INTERACTIVE)
	v.vertex = InteractiveVertex(v.vertex, o.texcoord.xy, v.normal);
	//o.test = InteractiveVertexTest(v.vertex, o.texcoord.xy, v.normal);
#endif

	o.pos = UnityObjectToClipPos(v.vertex);

	float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
	float3 worldNormal = UnityObjectToWorldNormal(v.normal);
	o.worldPos = float4(worldPos,1);


#ifdef NORMALMAP 
	o.texcoord.zw = TRANSFORM_TEX(v.texcoord, _BumpMapRGloss);
	fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
	fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
	fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
	o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
	o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
	o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
#else
	o.tSpace0 = float4(0, 0, worldNormal.x, worldPos.x);
	o.tSpace1 = float4(0, 0, worldNormal.y, worldPos.y);
	o.tSpace2 = float4(0, 0, worldNormal.z, worldPos.z);
#endif

	half4 ambientOrLightmapUV = 0;
#ifdef LIGHTMAP_ON
	ambientOrLightmapUV.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
#elif UNITY_SHOULD_SAMPLE_SH
	ambientOrLightmapUV.rgb = ShadeSHPerVertex(worldNormal, ambientOrLightmapUV.rgb);
#endif

#ifdef DYNAMICLIGHTMAP_ON
	ambientOrLightmapUV.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#endif
	o.ambientOrLightmapUV = ambientOrLightmapUV;

#ifdef XHIGHT_FOG
	o.fogCoordH = saturate(((_HFogEnd - worldPos.y) / (_HFogEnd - _HFogStart)));
#endif

#if defined(XWEATHER_WATER)
	o.weather_parameter.xy = TRANSFORM_TEX((v.texcoord + (_Time.y * _WaterSpeed.xy * 0.01)), _WaterBump);
	o.weather_parameter.zw = TRANSFORM_TEX((v.texcoord + (_Time.y * _WaterSpeed.zw * 0.01)), _WaterBump);
#endif

#if defined(XWEATHER_SNOW)
	o.weather_parameter.xy = TRANSFORM_TEX(v.texcoord, _SnowAlbedo);
#endif


	UNITY_TRANSFER_LIGHTING(o, v.texcoord1);
	UNITY_TRANSFER_FOG(o, o.pos);
	return o;
}

fixed4 frag(v2f i) : SV_Target
{
	UNITY_APPLY_DITHER_CROSSFADE(i.pos.xy);
	UNITY_SETUP_INSTANCE_ID(i);

	//float4 debug = 1; debug.rgb = i.texcoord.y; debug.a = 1; return debug;
	//float4 debug = i.test.x; debug.a = 1; return debug;
	fixed4 tex = tex2D(_MainTex, i.texcoord.xy);
	tex.rgb *= _Color.rgb;
	#if defined(_ALPHATEST_ON)
		fixed pa = X_Alpha(tex.a, i.texcoord.xy);
		clip(pa - (1 - _Color.a));
	#endif

	fixed4 c = 0;
	float3 worldPos = float3(i.tSpace0.w, i.tSpace1.w, i.tSpace2.w);
	fixed3 lightDir = 0;

	#if !defined(x_LIGHT_NONE_TEXTURE) 
		lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
	#endif


	float3 worldViewDir = 0;
	#if !defined(x_LIGHT_MODE_TEXTURE) 
		worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
	#endif

	half3 worldNormal = 0;
	fixed gloss = 0;
	#if defined(NORMALMAP) && !defined(x_LIGHT_MODE_TEXTURE)
		#if defined(X_GLOSS_NORMAL_B)
			fixed4 bumpTex = tex2D(_BumpMapRGloss, i.texcoord.zw);
			fixed3 normalTangent = X_UnpackScaleNormal(bumpTex,1);
			gloss = bumpTex.b;
		#else
			fixed3 normalTangent = UnpackNormal(tex2D(_BumpMapRGloss, i.texcoord.zw));
		#endif
		#if defined(XWEATHER_WATER)
			fixed waterMask = tex2D(_WaterBump, i.texcoord.xy).b;
			waterMask = (waterMask - 0.3) * _WaterMask * X_GLOBAL_XWEATHER_FADE;
			fixed3 waterBump1 = lerp(fixed3(0,0,0),X_UnpackScaleNormal(tex2D(_WaterBump, i.weather_parameter.xy),_WaterNormalPower),waterMask);
			fixed3 waterBump2 = lerp(fixed3(0,0,0),X_UnpackScaleNormal(tex2D(_WaterBump, i.weather_parameter.zw),_WaterNormalPower),waterMask);
			fixed3 waterBump = (waterBump1 + waterBump2) * 0.5;

			normalTangent = waterBump + normalTangent;
			normalTangent.xy += X_Ripplefunc(i.texcoord.xy) * waterMask;
		#elif defined(XWEATHER_SNOW)
			float4 snowNormal = tex2D(_SnowNormal, i.weather_parameter.xy);
			fixed3 oldNormal = normalTangent;
			normalTangent = lerp(normalTangent, (normalTangent * _SnowThick) + X_UnpackScaleNormal(snowNormal,_SnowNormalPower),snowFac(i.tSpace1.z));
			normalTangent = lerp(oldNormal,normalTangent,_SnowMask * X_GLOBAL_XWEATHER_FADE);
		#endif


		worldNormal.x = dot(i.tSpace0, normalTangent);
		worldNormal.y = dot(i.tSpace1, normalTangent);
		worldNormal.z = dot(i.tSpace2, normalTangent);
		worldNormal = normalize(worldNormal);

	#elif defined(x_LIGHT_MODE_TEXTURE)     
		worldNormal = half3(i.tSpace0.z, i.tSpace1.z, i.tSpace2.z);
	#else
		worldNormal = normalize(half3(i.tSpace0.z, i.tSpace1.z, i.tSpace2.z));

		#if defined(XWEATHER_WATER)
		fixed waterMask = tex2D(_WaterBump, i.texcoord.xy).b;
		waterMask = (waterMask - 0.3) * _WaterMask * X_GLOBAL_XWEATHER_FADE;
		worldNormal.xy += X_Ripplefunc(i.texcoord.xy) * waterMask;
		#endif
	#endif


	UNITY_LIGHT_ATTENUATION(atten, i, worldPos);

	#if defined(XWEATHER_SNOW)
	#elif defined(XWEATHER_WATER)
		atten = max(atten,0.15);
	#endif

	UnityGI gi = XFragmentGI(lightDir,worldPos,worldViewDir,i.ambientOrLightmapUV,worldNormal,atten);

	#if defined(X_LIGHT_MODE_BLINNPHONG) || defined(REFLECTMATCAP) || defined(REFLECTCUBEMAP)
		#if !defined(X_GLOSS_NORMAL_B)
			gloss = tex2D(_GlossMap, i.texcoord.xy).r;
		#endif 
	#endif 

	#if defined(XWEATHER_SNOW)    
		c = XFragmentfunc(i.texcoord.xy,tex,_Shininess,gloss,worldViewDir,worldNormal,gi,i.weather_parameter.xy);
	#else
		c = XFragmentfunc(i.texcoord.xy,tex,_Shininess,gloss,worldViewDir,worldNormal,gi,float2(0,0));
	#endif

	#if defined(XHIGHT_FOG)
		// i.fogCoordH = saturate( ( _HFogEnd - worldPos.y ) / ( _HFogEnd - _HFogStart ));
		c.rgb = lerp(c.rgb,lerp(c.rgb ,_HFogColor,i.fogCoordH * _HFogColor.a),_Fog);
	#endif    

	fixed3 fogColor = lerp(c.rgb,unity_FogColor,_Fog * (1 - _lFogPower));

	XRLINEFOG_COLOR(c.rgb);
	UNITY_APPLY_FOG_COLOR(i.fogCoord, c, fogColor);
	
	float alpha = TransparentAnime();
	c.a *= alpha;

	return c;
}

//================================================================= 地形 ======================================================================

struct v2fterrain
{
	UNITY_POSITION(pos);
	float4 packdata0            : TEXCOORD0;
	float4 tSpace0              : TEXCOORD1;
	float4 tSpace1              : TEXCOORD2;
	float4 tSpace2              : TEXCOORD3;
	float4 ambientOrLightmapUV  : TEXCOORD4;
	float  fogCoordH : TEXCOORD5;
	float4 worldPos : TEXCOORD6;
#if !defined (UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS)
	UNITY_SHADOW_COORDS(7)
		UNITY_FOG_COORDS(8)
#else
	UNITY_LIGHTING_COORDS(7, 8)
		UNITY_FOG_COORDS(9)
#endif

		float4 packdata1  : TEXCOORD10;
	float4 packdata2  : TEXCOORD11;

#if defined(XWEATHER_WATER) || defined(XWEATHER_SNOW)
	float4  weather_parameter  : TEXCOORD12;
#endif
};



sampler2D _Control;
sampler2D _Splat0;
sampler2D _Splat1;
sampler2D _Splat2;
sampler2D _Splat3;

sampler2D _Normal0;
sampler2D _Normal1;
sampler2D _Normal2;
sampler2D _Normal3;

float4 _Control_ST;
float4 _Splat0_ST;
float4 _Splat1_ST;
float4 _Splat2_ST;
float4 _Splat3_ST;


fixed _Shininess0;
fixed _Shininess1;
fixed _Shininess2;
fixed _Shininess3;


v2fterrain verterrain(appdata v)
{
	v2fterrain o;
	UNITY_INITIALIZE_OUTPUT(v2fterrain, o);
	o.pos = UnityObjectToClipPos(v.vertex);
	o.packdata0.xy = TRANSFORM_TEX(v.texcoord, _Control);
	o.packdata1.xy = TRANSFORM_TEX(v.texcoord, _Splat0);
	o.packdata1.zw = TRANSFORM_TEX(v.texcoord, _Splat1);
	o.packdata2.xy = TRANSFORM_TEX(v.texcoord, _Splat2);
	o.packdata2.zw = TRANSFORM_TEX(v.texcoord, _Splat3);


	float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
	o.worldPos = float4(worldPos,1);
	float3 worldNormal = UnityObjectToWorldNormal(v.normal);
#ifdef NORMALMAP 
	fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
	fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
	fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
	o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
	o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
	o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
#else
	o.tSpace0 = float4(0, 0, worldNormal.x, worldPos.x);
	o.tSpace1 = float4(0, 0, worldNormal.y, worldPos.y);
	o.tSpace2 = float4(0, 0, worldNormal.z, worldPos.z);
#endif

	half4 ambientOrLightmapUV = 0;
#ifdef LIGHTMAP_ON
	ambientOrLightmapUV.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
#elif UNITY_SHOULD_SAMPLE_SH
	ambientOrLightmapUV.rgb = ShadeSHPerVertex(worldNormal, ambientOrLightmapUV.rgb);
#endif

#ifdef DYNAMICLIGHTMAP_ON
	ambientOrLightmapUV.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#endif
	o.ambientOrLightmapUV = ambientOrLightmapUV;

#ifdef XHIGHT_FOG
	o.fogCoordH = saturate(((_HFogEnd - worldPos.y) / (_HFogEnd - _HFogStart)));
#endif

#if defined(XWEATHER_WATER)
	o.weather_parameter.xy = TRANSFORM_TEX((v.texcoord + (_Time.y * _WaterSpeed.xy * 0.01)), _WaterBump);
	o.weather_parameter.zw = TRANSFORM_TEX((v.texcoord + (_Time.y * _WaterSpeed.zw * 0.01)), _WaterBump);
#endif

#if defined(XWEATHER_SNOW)
	o.weather_parameter.xy = TRANSFORM_TEX(v.texcoord, _SnowAlbedo);
#endif



	UNITY_TRANSFER_LIGHTING(o, v.texcoord1);
	UNITY_TRANSFER_FOG(o, o.pos);
	return o;
}

fixed4 fragterrain(v2fterrain i) : SV_Target
{

	fixed4 c = 0;
	float3 worldPos = float3(i.tSpace0.w, i.tSpace1.w, i.tSpace2.w);
	fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
	float3 worldViewDir = 0;
	#if !defined(x_LIGHT_MODE_TEXTURE) 
		worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
	#endif

	fixed4 splat_control = tex2D(_Control, i.packdata0.xy);

	fixed4 mixedDiffuse = 0;
	mixedDiffuse += splat_control.r * tex2D(_Splat0, i.packdata1.xy);
	mixedDiffuse += splat_control.g * tex2D(_Splat1, i.packdata1.zw);
	mixedDiffuse += splat_control.b * tex2D(_Splat2, i.packdata2.xy);
	mixedDiffuse += splat_control.a * tex2D(_Splat3, i.packdata2.zw);
	mixedDiffuse *= _Color;

	half3 worldNormal = 0;
	half Shininess = 0;
	#if defined(NORMALMAP) && !defined(x_LIGHT_MODE_TEXTURE) 
		fixed4 normalTangent = splat_control.r * tex2D(_Normal0, i.packdata1.xy);
		normalTangent += splat_control.g * tex2D(_Normal1, i.packdata1.zw);
		normalTangent += splat_control.b * tex2D(_Normal2, i.packdata2.xy);
		normalTangent += splat_control.a * tex2D(_Normal3, i.packdata2.zw);


		Shininess = normalTangent.b;
		fixed3 mixedNormal = X_UnpackScaleNormal(normalTangent,1);

		#if defined(XWEATHER_WATER)
			fixed waterMask = tex2D(_WaterBump, i.packdata0.xy).b;
			waterMask = (waterMask - 0.3) * _WaterMask * X_GLOBAL_XWEATHER_FADE;
			fixed3 waterBump1 = lerp(fixed3(0,0,0),X_UnpackScaleNormal(tex2D(_WaterBump, i.weather_parameter.xy),_WaterNormalPower),waterMask);
			fixed3 waterBump2 = lerp(fixed3(0,0,0),X_UnpackScaleNormal(tex2D(_WaterBump, i.weather_parameter.zw),_WaterNormalPower),waterMask);
			fixed3 waterBump = (waterBump1 + waterBump2) * 0.5;

			mixedNormal = waterBump + mixedNormal;
			mixedNormal.xy += X_Ripplefunc(i.packdata0.xy) * waterMask;
		#elif defined(XWEATHER_SNOW)
			float4 snowNormal = tex2D(_SnowNormal, i.weather_parameter.xy);
			fixed3 oldNormal = mixedNormal;
			mixedNormal = lerp(mixedNormal, (mixedNormal * _SnowThick) + X_UnpackScaleNormal(snowNormal,_SnowNormalPower),snowFac(i.tSpace1.z));
			mixedNormal = lerp(oldNormal,mixedNormal,_SnowMask * X_GLOBAL_XWEATHER_FADE);
		#endif


		worldNormal.x = dot(i.tSpace0, mixedNormal);
		worldNormal.y = dot(i.tSpace1, mixedNormal);
		worldNormal.z = dot(i.tSpace2, mixedNormal);
		worldNormal = normalize(worldNormal);
	#elif defined(x_LIGHT_MODE_TEXTURE)     
		worldNormal = half3(i.tSpace0.z, i.tSpace1.z, i.tSpace2.z);
	#else
		worldNormal = normalize(half3(i.tSpace0.z, i.tSpace1.z, i.tSpace2.z));
	#endif


	UNITY_LIGHT_ATTENUATION(atten, i, worldPos);
	UnityGI gi = XFragmentGI(lightDir,worldPos,worldViewDir,i.ambientOrLightmapUV,worldNormal,atten);

	// fixed4 gloss    = 0;

	#if defined(X_LIGHT_MODE_BLINNPHONG) || defined(REFLECTMATCAP) || defined(REFLECTCUBEMAP)

	#endif

	c = XFragmentfunc(i.packdata0.xy,mixedDiffuse,Shininess,mixedDiffuse.a,worldViewDir,worldNormal,gi,float2(0,0));

	#if defined(XWEATHER_SNOW)    
		 c = XFragmentfunc(i.packdata0.xy,mixedDiffuse,Shininess,mixedDiffuse.a,worldViewDir,worldNormal,gi,i.weather_parameter.xy);
	#else
		 c = XFragmentfunc(i.packdata0.xy,mixedDiffuse,Shininess,mixedDiffuse.a,worldViewDir,worldNormal,gi,float2(0,0));
	#endif



	#if defined(XHIGHT_FOG)
		c.rgb = lerp(c.rgb ,_HFogColor,i.fogCoordH * _HFogColor.a);
	#endif    

		//UNITY_APPLY_FOG(i.fogCoord,c);

		fixed3 fogColor = lerp(c.rgb,unity_FogColor,_Fog * (1 - _lFogPower));
		UNITY_APPLY_FOG_COLOR(i.fogCoord, c, fogColor);
		return c;
}





//================================================================================================================================================================================
sampler2D _IndexMap;
sampler2D _BlendMap14;
sampler2D _BlendMap48;


float4 _Scale1;
float4 _Scale2;
float4 _Scale3;
float4 _Scale4;
float4 _Gloss1;
float4 _Gloss2;
UNITY_DECLARE_TEX2DARRAY(_MainTexs);

#if defined(NORMALMAP) && !defined(x_LIGHT_MODE_TEXTURE)
	UNITY_DECLARE_TEX2DARRAY(_NormalTexs);
	fixed3 texNormalColor(float2 uv, fixed v)
	{
		float tidx = ceil(v * 15.0);

		int varNum = int(tidx / 4);

		float4x4 scale_values = float4x4(_Scale1, _Scale2, _Scale3, _Scale4);

		uv *= scale_values[varNum][tidx % 4];

		return UNITY_SAMPLE_TEX2DARRAY(_NormalTexs, float3(uv, tidx));
	}
#endif

fixed3 texColor(float2 uv, fixed v)
{
	float tidx = ceil(v * 15.0);

	int varNum = int(tidx / 4);

	float4x4 scale_values = float4x4(_Scale1, _Scale2, _Scale3, _Scale4);

	uv *= scale_values[varNum][tidx % 4];

	return UNITY_SAMPLE_TEX2DARRAY(_MainTexs, float3(uv, tidx));
}



fixed texBlendColor(fixed v, float4x4 bcolor)
{
	float tidx = ceil(v * 15.0);
	int varNum = int(tidx / 4);
	return bcolor[varNum][tidx % 4];
}

fixed parGloss(fixed v)
{
	float tidx = ceil(v * 15.0);

	int varNum = int(tidx / 4);

	float4x4 scale_values = float4x4(_Gloss1, _Gloss2, fixed4(0,0,0,0), fixed4(0, 0, 0, 0));

	return scale_values[varNum][tidx % 4];
}

fixed3 terrainColor(float2 uv,out fixed3 normal,out fixed gloss)
{
	gloss = 0;
	normal = 0;
	fixed4 indexValue = tex2D(_IndexMap, uv);

	fixed3 color1 = texColor(uv, indexValue.r);
	fixed3 color2 = texColor(uv, indexValue.g);
	fixed3 color3 = texColor(uv, indexValue.b);

	fixed4 blend14 = tex2D(_BlendMap14, uv);
	fixed4 blend48 = tex2D(_BlendMap48, uv);
	float4x4 bcolor = float4x4(blend14, blend48, fixed4(0, 0, 0, 0), fixed4(0, 0, 0, 0));

	fixed bcolorr = texBlendColor(indexValue.r, bcolor);
	fixed bcolorg = texBlendColor(indexValue.g, bcolor);
	fixed bcolorb = texBlendColor(indexValue.b, bcolor);

	fixed totalValue = bcolorr + bcolorg + bcolorb;

	//normal
	#if defined(NORMALMAP) && !defined(x_LIGHT_MODE_TEXTURE)
		fixed3 ncolor1 = texNormalColor(uv, indexValue.r);
		fixed3 ncolor2 = texNormalColor(uv, indexValue.g);
		fixed3 ncolor3 = texNormalColor(uv, indexValue.b);
		fixed3 normalBlendColor = (ncolor1 * (bcolorr + (1 - totalValue))) + (ncolor2 * bcolorg) + (ncolor3 * bcolorb);

		#if defined(X_GLOSS_NORMAL_B)
			fixed3 normalTangent = X_UnpackScaleNormal(fixed4(normalBlendColor,1), 1);
			//gloss = bumpTex.b;
		#else
			fixed3 normalTangent = UnpackNormal(fixed4(normalBlendColor, 1));
		#endif

		normal = normalTangent;

		gloss = bcolorr * parGloss(indexValue.r) + bcolorg * parGloss(indexValue.g) + bcolorb * parGloss(indexValue.b);
	#endif

	return (color1 * (bcolorr + (1 - totalValue))) + (color2 * bcolorg) + (color3 * bcolorb);
}




fixed4 fragterrainNew(v2f i) : SV_Target
{
	UNITY_APPLY_DITHER_CROSSFADE(i.pos.xy);
	UNITY_SETUP_INSTANCE_ID(i);
	fixed3 normalTangent = 0;
	fixed texGloss = 0;
	fixed4 tex = fixed4(terrainColor(i.texcoord.xy, normalTangent, texGloss),1);

	tex.rgb *= _Color.rgb * 2;
	#if defined(_ALPHATEST_ON)
		fixed pa = X_Alpha(tex.a, i.texcoord.xy);
		clip(pa - (1 - _Color.a));
	#endif

	fixed4 c = 0;
	float3 worldPos = float3(i.tSpace0.w, i.tSpace1.w, i.tSpace2.w);
	fixed3 lightDir = 0;

	#if !defined(x_LIGHT_NONE_TEXTURE) 
		lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
	#endif


	float3 worldViewDir = 0;
	#if !defined(x_LIGHT_MODE_TEXTURE) 
		worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
	#endif

	half3 worldNormal = 0;
	fixed gloss = texGloss;
	#if defined(NORMALMAP) && !defined(x_LIGHT_MODE_TEXTURE)
		#if defined(XWEATHER_WATER)
			fixed waterMask = tex2D(_WaterBump, i.texcoord.xy).b;
			waterMask = (waterMask - 0.3) * _WaterMask * X_GLOBAL_XWEATHER_FADE;
			fixed3 waterBump1 = lerp(fixed3(0,0,0),X_UnpackScaleNormal(tex2D(_WaterBump, i.weather_parameter.xy),_WaterNormalPower),waterMask);
			fixed3 waterBump2 = lerp(fixed3(0,0,0),X_UnpackScaleNormal(tex2D(_WaterBump, i.weather_parameter.zw),_WaterNormalPower),waterMask);
			fixed3 waterBump = (waterBump1 + waterBump2) * 0.5;

			normalTangent = waterBump + normalTangent;
			normalTangent.xy += X_Ripplefunc(i.texcoord.xy) * waterMask;
		#elif defined(XWEATHER_SNOW)
			float4 snowNormal = tex2D(_SnowNormal, i.weather_parameter.xy);
			fixed3 oldNormal = normalTangent;
			normalTangent = lerp(normalTangent, (normalTangent * _SnowThick) + X_UnpackScaleNormal(snowNormal,_SnowNormalPower),snowFac(i.tSpace1.z));
			normalTangent = lerp(oldNormal,normalTangent,_SnowMask * X_GLOBAL_XWEATHER_FADE);
		#endif


		worldNormal.x = dot(i.tSpace0, normalTangent);
		worldNormal.y = dot(i.tSpace1, normalTangent);
		worldNormal.z = dot(i.tSpace2, normalTangent);
		worldNormal = normalize(worldNormal);

	#elif defined(x_LIGHT_MODE_TEXTURE)     
		worldNormal = half3(i.tSpace0.z, i.tSpace1.z, i.tSpace2.z);
	#else
		worldNormal = normalize(half3(i.tSpace0.z, i.tSpace1.z, i.tSpace2.z));

		#if defined(XWEATHER_WATER)
		fixed waterMask = tex2D(_WaterBump, i.texcoord.xy).b;
		waterMask = (waterMask - 0.3) * _WaterMask * X_GLOBAL_XWEATHER_FADE;
		worldNormal.xy += X_Ripplefunc(i.texcoord.xy) * waterMask;
		#endif
	#endif


	UNITY_LIGHT_ATTENUATION(atten, i, worldPos);

	#if defined(XWEATHER_SNOW)
	#elif defined(XWEATHER_WATER)
		atten = max(atten,0.15);
	#endif

	UnityGI gi = XFragmentGI(lightDir,worldPos,worldViewDir,i.ambientOrLightmapUV,worldNormal,atten);

	#if defined(X_LIGHT_MODE_BLINNPHONG) || defined(REFLECTMATCAP) || defined(REFLECTCUBEMAP)
		#if !defined(X_GLOSS_NORMAL_B)
			gloss = tex2D(_GlossMap, i.texcoord.xy).r;
		#endif 
	#endif 

	#if defined(XWEATHER_SNOW)    
		c = XFragmentfunc(i.texcoord.xy,tex,_Shininess,gloss,worldViewDir,worldNormal,gi,i.weather_parameter.xy);
	#else
		c = XFragmentfunc(i.texcoord.xy,tex,_Shininess,gloss,worldViewDir,worldNormal,gi,float2(0,0));
	#endif

	#if defined(XHIGHT_FOG)
		// i.fogCoordH = saturate( ( _HFogEnd - worldPos.y ) / ( _HFogEnd - _HFogStart ));
		c.rgb = lerp(c.rgb,lerp(c.rgb ,_HFogColor,i.fogCoordH * _HFogColor.a),_Fog);
	#endif    

	fixed3 fogColor = lerp(c.rgb,unity_FogColor,_Fog * (1 - _lFogPower));
	XRLINEFOG_COLOR(c.rgb);
	UNITY_APPLY_FOG_COLOR(i.fogCoord, c, fogColor);
	return c;
}
#endif // XSCENESTANDARDCORE_INCLUDED
#endif

#if defined(ClOUDPARALLAXCORE)
sampler2D _MainTex;
float4 _MainTex_ST;
half _Height;
float4 _HeightTileSpeed;
half _HeightAmount;
half4 _Color;
half _Alpha;
half _LightIntensity;

half4 _LightingColor;
half4 _FixedLightDir;
half _UseFixedLight;

fixed _Shininess;
//fixed4 _SpecColor;

struct v2f 
{
    float4 pos : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 normalDir : TEXCOORD1;
    float3 viewDir : TEXCOORD2;
    float4 posWorld : TEXCOORD3;
    float2 uv2 : TEXCOORD4;
    float4 color : TEXCOORD5;
    UNITY_FOG_COORDS(7)
    float2 uv3 : TEXCOORD8;
};

v2f vert (appdata_full v) 
{
    v2f o;
    o.pos = UnityObjectToClipPos(v.vertex);
    
    o.uv = TRANSFORM_TEX(v.texcoord,_MainTex) + frac(_Time.y*_HeightTileSpeed.zw);
    o.uv2 = v.texcoord * _HeightTileSpeed.xy;
    o.uv3 = v.texcoord;
    o.posWorld = mul(unity_ObjectToWorld, v.vertex);
    o.normalDir = UnityObjectToWorldNormal(v.normal);
    TANGENT_SPACE_ROTATION;
    o.viewDir = mul(rotation, ObjSpaceViewDir(v.vertex));
    o.color = v.color;
    UNITY_TRANSFER_FOG(o,o.pos);
    return o;
}

float4 frag(v2f i) : COLOR
{
    float3 viewRay=normalize(i.viewDir*-1);
    viewRay.z=abs(viewRay.z)+0.2;
    viewRay.xy *= _Height;

    float3 shadeP = float3(i.uv,0);
    float3 shadeP2 = float3(i.uv2,0);


    float linearStep = 16;
    
    float4 mask = tex2D(_MainTex,i.uv3);

    float4 T = tex2D(_MainTex, shadeP2.xy).g;
    
    float h2 = T.r * _HeightAmount;

    float3 lioffset = viewRay / (viewRay.z * linearStep);
    float d = 1.0 - tex2Dlod(_MainTex, float4(shadeP.xy,0,0)).r * h2;
    float3 prev_d = d;
    float3 prev_shadeP = shadeP;

#if defined(Parallax)
    while(d > shadeP.z)
    {
        prev_shadeP = shadeP;
        shadeP += lioffset;
        prev_d = d;
        d = 1.0 - tex2Dlod(_MainTex, float4(shadeP.xy,0,0)).r * h2;
    }
#endif

    float d1 = d - shadeP.z;
    float d2 = prev_d - prev_shadeP.z;
    float w = d1 / (d1 - d2);
    shadeP = lerp(shadeP, prev_shadeP, w);

    half4 c = tex2D(_MainTex,shadeP.xy).g * T * _Color;
    half Alpha = lerp(c.r, 1.0, _Alpha) * i.color.a * mask.b;

    float3 normal = normalize(i.normalDir);
    
    half3 lightDir1 = normalize(_FixedLightDir.xyz);
    half3 lightDir2 = UnityWorldSpaceLightDir(i.posWorld);

    half3 lightDir = lerp(lightDir2, lightDir1, _UseFixedLight);
    half3 lightColor = _LightColor0.rgb;

    fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(i.posWorld));
    half3 h = normalize (lightDir + worldViewDir);
    fixed diff = max (0, dot (normal,lightDir));
    float nh = max (0, dot (normal, h));
    float spec = pow (nh, _Shininess*128.0);
    
    c.rgb = c.rgb * (lightColor * diff + 1.0) + lightColor * _SpecColor.rgb * spec;
    
    c.a = Alpha;
    UNITY_APPLY_FOG(i.fogCoord, c.rgb);

    return c;
}

#endif

#if defined(WATE1)

struct appdata
{
	float4 vertex : POSITION;
	float4 color : COLOR;
	UNITY_VERTEX_INPUT_INSTANCE_ID
	float4 ase_texcoord : TEXCOORD0;
	float4 ase_tangent : TANGENT;
	float3 ase_normal : NORMAL;
	float4 texcoord1 : TEXCOORD1;
	float4 texcoord2 : TEXCOORD2;
};

struct v2f
{
	float4 vertex : SV_POSITION;
	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
	float4 ase_texcoord : TEXCOORD0;
	float4 ase_texcoord1 : TEXCOORD1;
	float4 ase_texcoord2 : TEXCOORD2;
	float4 ase_texcoord3 : TEXCOORD3;
	float4 ase_texcoord4 : TEXCOORD4;
	UNITY_SHADOW_COORDS(5)
	float4 ase_lmap : TEXCOORD6;
	float4 ase_sh : TEXCOORD7;
	float4 ase_color : COLOR;
	float4 ase_texcoord8 : TEXCOORD8;
	UNITY_FOG_COORDS(9)
};

uniform samplerCUBE _sky;
uniform float _normalscale;
uniform sampler2D _normal01;
uniform float4 _tiling02;
uniform float4 _tiling01;
uniform float4 _Speed;
uniform float4 _skycolor;
uniform float _skyfeel;
uniform float _specular01;
uniform float4 _specularcolor;
uniform float3 _lightdor;
uniform float _Shininess;
uniform float4 _light;
uniform float4 _diffusecolor;
uniform sampler2D _diffusemask;
uniform float4 _diffusemask_ST;
uniform float _fresnelscale;
uniform float _fresnelpower;
uniform float4 _fresnelColor;
uniform float4 _DeepColor;
uniform float4 _ShalowColor;
UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
uniform float4 _CameraDepthTexture_TexelSize;
uniform float _spindrift;
uniform float _automaticmask;

v2f vert ( appdata v )
{
	v2f o;
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
	UNITY_TRANSFER_INSTANCE_ID(v, o);

	float3 ase_worldTangent = UnityObjectToWorldDir(v.ase_tangent);
	o.ase_texcoord1.xyz = ase_worldTangent;
	float3 ase_worldNormal = UnityObjectToWorldNormal(v.ase_normal);
	o.ase_texcoord2.xyz = ase_worldNormal;
	float ase_vertexTangentSign = v.ase_tangent.w * unity_WorldTransformParams.w;
	float3 ase_worldBitangent = cross( ase_worldNormal, ase_worldTangent ) * ase_vertexTangentSign;
	o.ase_texcoord3.xyz = ase_worldBitangent;
	float3 ase_worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
	o.ase_texcoord4.xyz = ase_worldPos;
	#ifdef DYNAMICLIGHTMAP_ON //dynlm
	o.ase_lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
	#endif //dynlm
	#ifdef LIGHTMAP_ON //stalm
	o.ase_lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
	#endif //stalm
	#ifndef LIGHTMAP_ON //nstalm
	#if UNITY_SHOULD_SAMPLE_SH //sh
	o.ase_sh.xyz = 0;
	#ifdef VERTEXLIGHT_ON //vl
	o.ase_sh.xyz += Shade4PointLights (
	unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
	unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
	unity_4LightAtten0, ase_worldPos, ase_worldNormal);
	#endif //vl
	o.ase_sh.xyz = ShadeSHPerVertex (ase_worldNormal, o.ase_sh.xyz);
	#endif //sh
	#endif //nstalm
	float4 ase_clipPos = UnityObjectToClipPos(v.vertex);
	float4 screenPos = ComputeScreenPos(ase_clipPos);
	o.ase_texcoord8 = screenPos;
	
	o.ase_texcoord.xyz = v.ase_texcoord.xyz;
	o.ase_color = v.color;
	
	//setting value to unused interpolator channels and avoid initialization warnings
	o.ase_texcoord.w = 0;
	o.ase_texcoord1.w = 0;
	o.ase_texcoord2.w = 0;
	o.ase_texcoord3.w = 0;
	o.ase_texcoord4.w = 0;
	o.ase_sh.w = 0;
	float3 vertexValue =  float3(0,0,0) ;
	#if ASE_ABSOLUTE_VERTEX_POS
	v.vertex.xyz = vertexValue;
	#else
	v.vertex.xyz += vertexValue;
	#endif
	o.vertex = UnityObjectToClipPos(v.vertex);
	UNITY_TRANSFER_FOG(o,o.vertex);
	return o;
}

fixed4 frag (v2f i ) : SV_Target
{
	UNITY_SETUP_INSTANCE_ID(i);
	fixed4 finalColor;
	float2 appendResult141 = (float2(_tiling02.x , 1.0));
	float mulTime74 = _Time.y * 0.005;
	float2 appendResult63 = (float2(_Speed.x , 0.0));
	float2 uv042 = i.ase_texcoord.xyz * float2( 1,1 ) + float2( 0,0 );
	float2 panner41 = ( mulTime74 * appendResult63 + uv042);
	float2 temp_output_142_0 = ( appendResult141 * ( _tiling01.x * panner41 ) );
	float2 appendResult143 = (float2(1.0 , _tiling02.y));
	float2 appendResult64 = (float2(0.0 , _Speed.y));
	float2 panner50 = ( mulTime74 * appendResult64 + uv042);
	float2 appendResult314 = (float2(_tiling02.z , 1.0));
	float2 appendResult317 = (float2(_Speed.z , 0.0));
	float2 panner316 = ( mulTime74 * appendResult317 + uv042);
	float2 appendResult320 = (float2(1.0 , _tiling02.w));
	float2 appendResult323 = (float2(0.0 , _Speed.w));
	float2 panner322 = ( mulTime74 * appendResult323 + uv042);
	float3 temp_output_325_0 = BlendNormals( BlendNormals( UnpackScaleNormal( tex2D( _normal01, temp_output_142_0 ), _normalscale ) , UnpackScaleNormal( tex2D( _normal01, ( appendResult143 * ( _tiling01.y * panner50 ) ) ), _normalscale ) ) , BlendNormals( UnpackScaleNormal( tex2D( _normal01, ( appendResult314 * ( _tiling01.z * panner316 ) ) ), _normalscale ) , UnpackScaleNormal( tex2D( _normal01, ( appendResult320 * ( _tiling01.w * panner322 ) ) ), _normalscale ) ) );
	float3 ase_worldTangent = i.ase_texcoord1.xyz;
	float3 ase_worldNormal = i.ase_texcoord2.xyz;
	float3 ase_worldBitangent = i.ase_texcoord3.xyz;
	float3 tanToWorld0 = float3( ase_worldTangent.x, ase_worldBitangent.x, ase_worldNormal.x );
	float3 tanToWorld1 = float3( ase_worldTangent.y, ase_worldBitangent.y, ase_worldNormal.y );
	float3 tanToWorld2 = float3( ase_worldTangent.z, ase_worldBitangent.z, ase_worldNormal.z );
	float3 ase_worldPos = i.ase_texcoord4.xyz;
	float3 ase_worldViewDir = UnityWorldSpaceViewDir(ase_worldPos);
	ase_worldViewDir = normalize(ase_worldViewDir);
	float3 worldRefl190 = reflect( -ase_worldViewDir, float3( dot( tanToWorld0, temp_output_325_0 ), dot( tanToWorld1, temp_output_325_0 ), dot( tanToWorld2, temp_output_325_0 ) ) );
	float3 worldRefl446 = reflect( -ase_worldViewDir, float3( dot( tanToWorld0, UnpackScaleNormal( tex2D( _normal01, temp_output_142_0 ), _skyfeel ) ), dot( tanToWorld1, UnpackScaleNormal( tex2D( _normal01, temp_output_142_0 ), _skyfeel ) ), dot( tanToWorld2, UnpackScaleNormal( tex2D( _normal01, temp_output_142_0 ), _skyfeel ) ) ) );
	float4 temp_output_43_0_g7 = ( _specular01 * _specularcolor );
	float3 normalizeResult71_g7 = normalize( ( ase_worldViewDir + _lightdor ) );
	float3 tanNormal12_g7 = temp_output_325_0;
	float3 worldNormal12_g7 = float3(dot(tanToWorld0,tanNormal12_g7), dot(tanToWorld1,tanNormal12_g7), dot(tanToWorld2,tanNormal12_g7));
	float3 normalizeResult64_g7 = normalize( worldNormal12_g7 );
	float dotResult19_g7 = dot( normalizeResult71_g7 , normalizeResult64_g7 );
	UNITY_LIGHT_ATTENUATION(ase_atten, i, ase_worldPos)
	float4 temp_output_40_0_g7 = ( _light * ase_atten );
	float dotResult14_g7 = dot( normalizeResult64_g7 , _lightdor );
	UnityGIInput data34_g7;
	UNITY_INITIALIZE_OUTPUT( UnityGIInput, data34_g7 );
	#if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON) //dylm34_g7
	data34_g7.lightmapUV = i.ase_lmap;
	#endif //dylm34_g7
	#if UNITY_SHOULD_SAMPLE_SH //fsh34_g7
	data34_g7.ambient = i.ase_sh;
	#endif //fsh34_g7
	UnityGI gi34_g7 = UnityGI_Base(data34_g7, 1, normalizeResult64_g7);
	float2 uv_diffusemask = i.ase_texcoord.xyz.xy * _diffusemask_ST.xy + _diffusemask_ST.zw;
	float4 appendResult259 = (float4(_diffusecolor.rgb , ( _diffusecolor.a * tex2D( _diffusemask, uv_diffusemask ).r * i.ase_color.a )));
	float4 temp_output_42_0_g7 = appendResult259;
	float3 tanNormal329 = temp_output_325_0;
	float fresnelNdotV329 = dot( float3(dot(tanToWorld0,tanNormal329), dot(tanToWorld1,tanNormal329), dot(tanToWorld2,tanNormal329)), ase_worldViewDir );
	float fresnelNode329 = ( 0.0 + _fresnelscale * pow( 1.0 - fresnelNdotV329, _fresnelpower ) );
	float4 temp_cast_5 = (0.0).xxxx;
	float4 screenPos = i.ase_texcoord8;
	float eyeDepth530 = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture,UNITY_PROJ_COORD( screenPos ))));
	float temp_output_537_0 = saturate( pow( abs( ( eyeDepth530 - screenPos.w ) ) , -0.28 ) );
	float4 lerpResult572 = lerp( _DeepColor , _ShalowColor , temp_output_537_0);
	float4 temp_cast_6 = (_spindrift).xxxx;
	#ifdef _SPINDRIFTOFF_ON
	float4 staticSwitch581 = pow( lerpResult572 , temp_cast_6 );
	#else
	float4 staticSwitch581 = temp_cast_5;
	#endif
	float lerpResult540 = lerp( _DeepColor.a , _ShalowColor.a , temp_output_537_0);
	float myVarName548 = ( 1.0 - ( lerpResult540 * lerpResult540 * lerpResult540 * lerpResult540 * _automaticmask ) );
	#ifdef _AUTOMATICOFF_ON
	float staticSwitch542 = myVarName548;
	#else
	float staticSwitch542 = 1.0;
	#endif
	float4 appendResult185 = (float4(saturate( ( ( ( 3.0 * texCUBE( _sky, worldRefl190 ) * _skycolor * _skycolor.a * texCUBE( _sky, worldRefl446 ) ) * ( ( float4( (temp_output_43_0_g7).rgb , 0.0 ) * (temp_output_43_0_g7).a * pow( max( dotResult19_g7 , 0.0 ) , ( _Shininess * 128.0 ) ) * temp_output_40_0_g7 ) + ( ( ( temp_output_40_0_g7 * max( dotResult14_g7 , 0.0 ) ) + float4( gi34_g7.indirect.diffuse , 0.0 ) ) * float4( (temp_output_42_0_g7).rgb , 0.0 ) ) ) ) + ( fresnelNode329 * _fresnelColor ) + staticSwitch581 ) ).rgb , saturate( ( (temp_output_42_0_g7).a * staticSwitch542 ) )));
	
	
	finalColor = appendResult185;
	UNITY_APPLY_FOG(i.fogCoord, finalColor);
	return finalColor;
}

#endif

#if defined(Vertex_Glossy_Reflect)

struct v2f
{
	float2 uv : TEXCOORD0;
	UNITY_FOG_COORDS(1)
	fixed4 spec : TEXCOORD2;
	fixed3 refl : TEXCOORD3;
	fixed3 SHLighting: TEXCOORD4;

	SHADOW_COORDS(5)

	#ifdef LIGHTMAP_ON
		float2 lmap : TEXCOORD6;
	#endif

	float4 pos : SV_POSITION;
};

samplerCUBE _EnvTex;
sampler2D _MainTex;
float4 _MainTex_ST;
half _SHLightingScale;
fixed4 _Color;
float3 _SpecOffset;
float _SpecRange;
fixed4 _SpecColor;
float _Shininess;

v2f vert (appdata_full v)
{
	v2f o;
	o.pos = UnityObjectToClipPos(v.vertex);
	o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);

	float3 worldNormal = UnityObjectToWorldNormal(v.normal);

	float3 viewNormal = mul(UNITY_MATRIX_V,worldNormal);

	float3 viewPos = UnityObjectToViewPos(v.vertex);

	float3 viewDir = float3(0,0,1);

	float3 viewLightPos = _SpecOffset * float3(1,1,-1);
	
	float3 dirToLight = viewPos.xyz - viewLightPos;
	
	float3 h = ( normalize(-dirToLight)) * 0.5;
	float atten = 1.0 - saturate(length(dirToLight) / _SpecRange);

	float specular = pow(saturate(dot(viewNormal, normalize(h))), _Shininess * 128) * 3 * atten;
	o.spec = fixed4(_SpecColor.rgb * specular,specular);

	o.refl = reflect(-WorldSpaceViewDir(v.vertex), worldNormal);
	o.refl.x = -o.refl.x;

	o.SHLighting  = ShadeSH9(float4(worldNormal,1));// * _SHLightingScale;

	#ifdef LIGHTMAP_ON
		o.lmap = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
	#endif

	UNITY_TRANSFER_FOG(o,o.pos);
	TRANSFER_SHADOW(o);
	return o;
}

fixed4 frag (v2f i) : SV_Target
{

	fixed4 col = tex2D(_MainTex, i.uv);
	fixed3 spec = i.spec.rgb * col.a;

	fixed4 c = 0;
	c.rgb = i.SHLighting * _SHLightingScale;

	#ifdef LIGHTMAP_ON
		fixed3 lm = DecodeLightmap (UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lmap));
		c.rgb *= lm + col;
		c.rgb += (col + spec - col.rgb * spec) * _Color.rgb;
	#else
		
		fixed shadow = SHADOW_ATTENUATION(i);
		c.rgb *= shadow;
		c.rgb += (col + spec - col.rgb * spec) * _Color.rgb;
	#endif

	fixed3 env = texCUBE(_EnvTex,i.refl) * col.a * _Color.a;

	c.rgb += env * i.spec.a * _SpecColor.a;

	c.a = col.a;

	UNITY_APPLY_FOG(i.fogCoord, c);
	return c;
}
#endif

#if defined(TEXTURES)
struct appdata
{
	float4 vertex : POSITION;
	float4 color : COLOR;
	UNITY_VERTEX_INPUT_INSTANCE_ID
	float4 ase_texcoord : TEXCOORD0;
};

struct v2f
{
	float4 vertex : SV_POSITION;
	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
	float4 ase_texcoord : TEXCOORD0;
};

uniform float4 _Color0;
uniform sampler2D _TextureSample0;
uniform float4 _TextureSample0_ST;

v2f vert ( appdata v )
{
	v2f o;
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
	UNITY_TRANSFER_INSTANCE_ID(v, o);

	o.ase_texcoord.xy = v.ase_texcoord.xy;
	
	//setting value to unused interpolator channels and avoid initialization warnings
	o.ase_texcoord.zw = 0;
	float3 vertexValue =  float3(0,0,0) ;
	#if ASE_ABSOLUTE_VERTEX_POS
	v.vertex.xyz = vertexValue;
	#else
	v.vertex.xyz += vertexValue;
	#endif
	o.vertex = UnityObjectToClipPos(v.vertex);
	return o;
}

fixed4 frag (v2f i ) : SV_Target
{
	UNITY_SETUP_INSTANCE_ID(i);
	fixed4 finalColor;
	float2 uv_TextureSample0 = i.ase_texcoord.xy * _TextureSample0_ST.xy + _TextureSample0_ST.zw;
	
	
	finalColor = ( _Color0 * tex2D( _TextureSample0, uv_TextureSample0 ) );
	return finalColor;
}

#endif

#if defined(SKYBOX01)
struct appdata
{
	float4 vertex : POSITION;
	float4 color : COLOR;
	UNITY_VERTEX_INPUT_INSTANCE_ID
	
};

struct v2f
{
	float4 vertex : SV_POSITION;
	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
	float4 ase_texcoord : TEXCOORD0;
	float4 ase_texcoord1 : TEXCOORD1;
	float4 ase_texcoord2 : TEXCOORD2;
};

uniform half4 _Tex_HDR;
uniform samplerCUBE _Tex;
uniform half _Rotation;
uniform half _RotationSpeed;
uniform half4 _TintColor;
uniform half _Exposure;
uniform float4 _Colorfog;
uniform half _FogHeight;
uniform half _FogSmoothness;
uniform half _FogFill;
uniform float _ahplamask;
inline half3 DecodeHDR1189( half4 Data )
{
	return DecodeHDR(Data, _Tex_HDR);
}


v2f vert ( appdata v )
{
	v2f o;
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
	UNITY_TRANSFER_INSTANCE_ID(v, o);

	float lerpResult268 = lerp( 1.0 , ( unity_OrthoParams.y / unity_OrthoParams.x ) , unity_OrthoParams.w);
	half CAMERA_MODE300 = lerpResult268;
	float3 appendResult1129 = (float3(v.vertex.xyz.x , ( v.vertex.xyz.y * CAMERA_MODE300 ) , v.vertex.xyz.z));
	float3 normalizeResult1130 = normalize( appendResult1129 );
	float3 appendResult56 = (float3(cos( radians( ( _Rotation + ( _Time.y * _RotationSpeed ) ) ) ) , 0.0 , ( sin( radians( ( _Rotation + ( _Time.y * _RotationSpeed ) ) ) ) * -1.0 )));
	float3 appendResult266 = (float3(0.0 , CAMERA_MODE300 , 0.0));
	float3 appendResult58 = (float3(sin( radians( ( _Rotation + ( _Time.y * _RotationSpeed ) ) ) ) , 0.0 , cos( radians( ( _Rotation + ( _Time.y * _RotationSpeed ) ) ) )));
	float3 normalizeResult247 = normalize( v.vertex.xyz );
	#ifdef _ENABLEROTATION_ON
	float3 staticSwitch1164 = mul( float3x3(appendResult56, appendResult266, appendResult58), normalizeResult247 );
	#else
	float3 staticSwitch1164 = normalizeResult1130;
	#endif
	float3 vertexToFrag774 = staticSwitch1164;
	o.ase_texcoord.xyz = vertexToFrag774;
	float3 ase_worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
	o.ase_texcoord1.xyz = ase_worldPos;
	
	o.ase_texcoord2 = v.vertex;
	
	//setting value to unused interpolator channels and avoid initialization warnings
	o.ase_texcoord.w = 0;
	o.ase_texcoord1.w = 0;
	float3 vertexValue =  float3(0,0,0) ;
	#if ASE_ABSOLUTE_VERTEX_POS
	v.vertex.xyz = vertexValue;
	#else
	v.vertex.xyz += vertexValue;
	#endif
	o.vertex = UnityObjectToClipPos(v.vertex);
	return o;
}

fixed4 frag (v2f i ) : SV_Target
{
	UNITY_SETUP_INSTANCE_ID(i);
	fixed4 finalColor;
	float3 vertexToFrag774 = i.ase_texcoord.xyz;
	half4 Data1189 = texCUBE( _Tex, vertexToFrag774 );
	half3 localDecodeHDR1189 = DecodeHDR1189( Data1189 );
	half4 CUBEMAP222 = ( float4( localDecodeHDR1189 , 0.0 ) * unity_ColorSpaceDouble * _TintColor * _Exposure );
	float3 ase_worldPos = i.ase_texcoord1.xyz;
	#ifdef _VERTEXWORLD_ON
	float3 staticSwitch1207 = i.ase_texcoord2.xyz;
	#else
	float3 staticSwitch1207 = ase_worldPos;
	#endif
	float3 normalizeResult319 = normalize( staticSwitch1207 );
	float lerpResult678 = lerp( saturate( pow( (0.0 + (abs( normalizeResult319.y ) - 0.0) * (1.0 - 0.0) / (_FogHeight - 0.0)) , ( 1.0 - _FogSmoothness ) ) ) , 0.0 , _FogFill);
	half FOG_MASK359 = lerpResult678;
	float4 lerpResult317 = lerp( _Colorfog , CUBEMAP222 , FOG_MASK359);
	#ifdef _ENABLEFOG_ON
	float4 staticSwitch1179 = lerpResult317;
	#else
	float4 staticSwitch1179 = CUBEMAP222;
	#endif
	float4 appendResult1199 = (float4(staticSwitch1179.rgb , _ahplamask));
	
	
	finalColor = appendResult1199;
	return finalColor;
}
#endif

#if defined(SKYBOX)
samplerCUBE _Tex;
half4 _Tex_HDR;
half4 _Tint;
half _Exposure;
float _Rotation;

float3 RotateAroundYInDegrees (float3 vertex, float degrees)
{
	float alpha = degrees * UNITY_PI / 180.0;
	float sina, cosa;
	sincos(alpha, sina, cosa);
	float2x2 m = float2x2(cosa, -sina, sina, cosa);
	return float3(mul(m, vertex.xz), vertex.y).xzy;
}

struct appdata_t {
	float4 vertex : POSITION;
	float2 uv : TEXCOORD0;
	//UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f {
	float4 vertex : SV_POSITION;
	float3 texcoord : TEXCOORD0;
	//UNITY_VERTEX_OUTPUT_STEREO
};

v2f vert (appdata_t v)
{
	v2f o;
	//UNITY_SETUP_INSTANCE_ID(v);
	//UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
	float3 rotated = RotateAroundYInDegrees(v.vertex, _Rotation);
	o.vertex = UnityObjectToClipPos(rotated); 
	
	// o.vertex = UnityObjectToClipPos(mul(unity_ObjectToWorld,rotated) + _WorldSpaceCameraPos);
	o.texcoord = v.vertex.xyz;
	return o;
}

fixed4 frag (v2f i) : SV_Target
{
	half4 tex = texCUBE (_Tex, i.texcoord);
	half3 c = DecodeHDR (tex, _Tex_HDR);
	c = c * _Tint.rgb * unity_ColorSpaceDouble.rgb;
	c *= _Exposure;
	return fixed4(c,1);
}
#endif

#if defined(Reflect_Refraction_Fresnel)
uniform float _RefractionIntensity;
uniform sampler2D _Refraction; uniform float4 _Refraction_ST;
uniform samplerCUBE _ReflMap;
uniform float4 _ReflColor;
uniform float _Alpha;

fixed4 _FresnelColor;
float _FresnelPower;
float _FresnelWidth;

struct VertexInput {
	float4 vertex : POSITION;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
	float2 texcoord0 : TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};
struct VertexOutput {
	float4 pos : SV_POSITION;
	float2 uv0 : TEXCOORD0;
	float4 posWorld : TEXCOORD1;
	float3 normalDir : TEXCOORD2;
	float3 tangentDir : TEXCOORD3;
	float3 bitangentDir : TEXCOORD4;
	float4 screenPos : TEXCOORD5;
	UNITY_FOG_COORDS(6)
};
VertexOutput vert (VertexInput v) {
	VertexOutput o = (VertexOutput)0;
	UNITY_SETUP_INSTANCE_ID(v);
	o.uv0 = v.texcoord0;
	o.normalDir = UnityObjectToWorldNormal(v.normal);
	o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
	o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
	o.posWorld = mul(unity_ObjectToWorld, v.vertex);
	o.pos = UnityObjectToClipPos(v.vertex );
	UNITY_TRANSFER_FOG(o,o.pos);	
	return o;
}
float4 frag(VertexOutput i) : COLOR {

	float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
	float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
	float2 node_27 = (i.uv0*1.0);
	float3 _Refraction_var = UnpackNormal(tex2D(_Refraction,TRANSFORM_TEX(node_27, _Refraction)));
	float3 normalLocal = lerp(float3(0,0,1),_Refraction_var.rgb,_RefractionIntensity);
	float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
	float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
	float3 emissive = (_ReflColor.rgb*texCUBE(_ReflMap,viewReflectDirection).rgb);


	float fresnel = pow(1.0-saturate(dot (viewDirection, i.normalDir)),_FresnelWidth)*_FresnelPower;


	emissive +=  _FresnelColor * fresnel * _FresnelColor.a;
emissive += tex2D(_Refraction,TRANSFORM_TEX(node_27, _Refraction)).rgb * 0.3;


	float4 finalColor = float4(emissive.rgb,1);
finalColor.a = _Alpha * tex2D(_Refraction,TRANSFORM_TEX(node_27, _Refraction)).a ;
UNITY_APPLY_FOG(i.fogCoord, finalColor);
	return finalColor;
	
	

}
#endif

#if defined(MATCAP)
struct appdata
{
	float4 vertex : POSITION;
	float4 color : COLOR;
	UNITY_VERTEX_INPUT_INSTANCE_ID
	float4 ase_texcoord : TEXCOORD0;
	float4 ase_tangent : TANGENT;
	float3 ase_normal : NORMAL;
};

struct v2f
{
	float4 vertex : SV_POSITION;
	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
	float4 ase_texcoord : TEXCOORD0;
	float4 ase_texcoord1 : TEXCOORD1;
	float4 ase_texcoord2 : TEXCOORD2;
	float4 ase_texcoord3 : TEXCOORD3;
	float4 ase_texcoord4 : TEXCOORD4;
};

uniform float4 _skyColor0;
uniform samplerCUBE _sky;
uniform float _scale;
uniform sampler2D _Normals;
uniform float4 _Normals_ST;
uniform float _RimPower;
uniform float4 _RimColor;
uniform sampler2D _Matcap;
uniform float4 _capcolor;
uniform sampler2D _Texturemask;
uniform float4 _Texturemask_ST;

v2f vert ( appdata v )
{
	v2f o;
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
	UNITY_TRANSFER_INSTANCE_ID(v, o);

	float3 ase_worldTangent = UnityObjectToWorldDir(v.ase_tangent);
	o.ase_texcoord1.xyz = ase_worldTangent;
	float3 ase_worldNormal = UnityObjectToWorldNormal(v.ase_normal);
	o.ase_texcoord2.xyz = ase_worldNormal;
	float ase_vertexTangentSign = v.ase_tangent.w * unity_WorldTransformParams.w;
	float3 ase_worldBitangent = cross( ase_worldNormal, ase_worldTangent ) * ase_vertexTangentSign;
	o.ase_texcoord3.xyz = ase_worldBitangent;
	float3 ase_worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
	o.ase_texcoord4.xyz = ase_worldPos;
	
	o.ase_texcoord.xyz = v.ase_texcoord.xyz;
	
	//setting value to unused interpolator channels and avoid initialization warnings
	o.ase_texcoord.w = 0;
	o.ase_texcoord1.w = 0;
	o.ase_texcoord2.w = 0;
	o.ase_texcoord3.w = 0;
	o.ase_texcoord4.w = 0;
	float3 vertexValue =  float3(0,0,0) ;
	#if ASE_ABSOLUTE_VERTEX_POS
	v.vertex.xyz = vertexValue;
	#else
	v.vertex.xyz += vertexValue;
	#endif
	o.vertex = UnityObjectToClipPos(v.vertex);
	return o;
}

fixed4 frag (v2f i ) : SV_Target
{
	UNITY_SETUP_INSTANCE_ID(i);
	fixed4 finalColor;
	float2 uv_Normals = i.ase_texcoord.xyz * _Normals_ST.xy + _Normals_ST.zw;
	float3 tex2DNode36 = UnpackScaleNormal( tex2D( _Normals, uv_Normals ), _scale );
	float3 ase_worldTangent = i.ase_texcoord1.xyz;
	float3 ase_worldNormal = i.ase_texcoord2.xyz;
	float3 ase_worldBitangent = i.ase_texcoord3.xyz;
	float3 tanToWorld0 = float3( ase_worldTangent.x, ase_worldBitangent.x, ase_worldNormal.x );
	float3 tanToWorld1 = float3( ase_worldTangent.y, ase_worldBitangent.y, ase_worldNormal.y );
	float3 tanToWorld2 = float3( ase_worldTangent.z, ase_worldBitangent.z, ase_worldNormal.z );
	float3 ase_worldPos = i.ase_texcoord4.xyz;
	float3 ase_worldViewDir = UnityWorldSpaceViewDir(ase_worldPos);
	ase_worldViewDir = normalize(ase_worldViewDir);
	float3 worldRefl62 = reflect( -ase_worldViewDir, float3( dot( tanToWorld0, ( tex2DNode36 * 0.5 ) ), dot( tanToWorld1, ( tex2DNode36 * 0.5 ) ), dot( tanToWorld2, ( tex2DNode36 * 0.5 ) ) ) );
	float3 ase_tanViewDir =  tanToWorld0 * ase_worldViewDir.x + tanToWorld1 * ase_worldViewDir.y  + tanToWorld2 * ase_worldViewDir.z;
	ase_tanViewDir = normalize(ase_tanViewDir);
	float3 normalizeResult27 = normalize( ase_tanViewDir );
	float dotResult28 = dot( tex2DNode36 , normalizeResult27 );
	float2 uv_Texturemask = i.ase_texcoord.xyz.xy * _Texturemask_ST.xy + _Texturemask_ST.zw;
	float4 tex2DNode84 = tex2D( _Texturemask, uv_Texturemask );
	float4 appendResult78 = (float4(( ( ( _skyColor0 * texCUBE( _sky, worldRefl62 ) ) + ( pow( ( 1.0 - saturate( dotResult28 ) ) , _RimPower ) * _RimColor ) + ( tex2D( _Matcap, ( ( mul( UNITY_MATRIX_V, float4( ase_worldNormal , 0.0 ) ).xyz * 0.5 ) + 0.5 ).xy ) * _capcolor * 2.0 ) ) + ( _RimColor.a * tex2DNode84 ) ).rgb , ( _capcolor.a * tex2DNode84.a )));
	
	
	finalColor = appendResult78;
	return finalColor;
}
#endif

#if defined(CLOUD01)
sampler3D _DitherMaskLOD;
struct v2f
{
	V2F_SHADOW_CASTER;
	float2 customPack1 : TEXCOORD1;
	float3 worldPos : TEXCOORD2;
	half4 color : COLOR0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};
v2f vert( appdata_full v )
{
	v2f o;
	UNITY_SETUP_INSTANCE_ID( v );
	UNITY_INITIALIZE_OUTPUT( v2f, o );
	UNITY_TRANSFER_INSTANCE_ID( v, o );
	Input customInputData;
	float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
	half3 worldNormal = UnityObjectToWorldNormal( v.normal );
	o.customPack1.xy = customInputData.uv_texcoord;
	o.customPack1.xy = v.texcoord;
	o.worldPos = worldPos;
	TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
	o.color = v.color;
	return o;
}
half4 frag( v2f IN
#if !defined( CAN_SKIP_VPOS )
, UNITY_VPOS_TYPE vpos : VPOS
#endif
) : SV_Target
{
	UNITY_SETUP_INSTANCE_ID( IN );
	Input surfIN;
	UNITY_INITIALIZE_OUTPUT( Input, surfIN );
	surfIN.uv_texcoord = IN.customPack1.xy;
	float3 worldPos = IN.worldPos;
	half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
	surfIN.vertexColor = IN.color;
	SurfaceOutputStandard o;
	UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
	surf( surfIN, o );
	#if defined( CAN_SKIP_VPOS )
	float2 vpos = IN.pos;
	#endif
	half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
	clip( alphaRef - 0.01 );
	SHADOW_CASTER_FRAGMENT( IN )
}
#endif

#if defined(BILLBOARDS)
struct v2f
{
	V2F_SHADOW_CASTER;
	float2 customPack1 : TEXCOORD1;
	float4 tSpace0 : TEXCOORD2;
	float4 tSpace1 : TEXCOORD3;
	float4 tSpace2 : TEXCOORD4;
	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
};
v2f vert( appdata_full v )
{
	v2f o;
	UNITY_SETUP_INSTANCE_ID( v );
	UNITY_INITIALIZE_OUTPUT( v2f, o );
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
	UNITY_TRANSFER_INSTANCE_ID( v, o );
	Input customInputData;
	vertexDataFunc( v, customInputData );
	float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
	half3 worldNormal = UnityObjectToWorldNormal( v.normal );
	half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
	half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
	half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
	o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
	o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
	o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
	o.customPack1.xy = customInputData.uv_texcoord;
	o.customPack1.xy = v.texcoord;
	TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
	return o;
}
half4 frag( v2f IN
#if !defined( CAN_SKIP_VPOS )
, UNITY_VPOS_TYPE vpos : VPOS
#endif
) : SV_Target
{
	UNITY_SETUP_INSTANCE_ID( IN );
	Input surfIN;
	UNITY_INITIALIZE_OUTPUT( Input, surfIN );
	surfIN.uv_texcoord = IN.customPack1.xy;
	float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
	half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
	surfIN.worldPos = worldPos;
	surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
	surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
	surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
	surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
	SurfaceOutputCustomLightingCustom o;
	UNITY_INITIALIZE_OUTPUT( SurfaceOutputCustomLightingCustom, o )
	surf( surfIN, o );
	UnityGI gi;
	UNITY_INITIALIZE_OUTPUT( UnityGI, gi );
	o.Alpha = LightingStandardCustomLighting( o, worldViewDir, gi ).a;
	#if defined( CAN_SKIP_VPOS )
	float2 vpos = IN.pos;
	#endif
	SHADOW_CASTER_FRAGMENT( IN )
}
#endif