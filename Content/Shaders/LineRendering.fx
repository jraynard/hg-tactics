// Line rendering helper shader (MonoGame port — sm2.0)

float4x4 worldViewProj : WorldViewProjection;

struct VertexInput
{
    float3 pos   : POSITION;
    float4 color : COLOR;
};

struct VertexOutput
{
    float4 pos   : SV_Position;
    float4 color : COLOR;
};

VertexOutput LineRenderingVS(VertexInput In)
{
    VertexOutput Out;
    Out.pos   = mul(float4(In.pos, 1), worldViewProj);
    Out.color = In.color;
    return Out;
}

float4 LineRenderingPS(VertexOutput In) : SV_Target
{
    return In.color;
}

VertexOutput LineRendering2DVS(VertexInput In)
{
    VertexOutput Out;
    Out.pos   = float4(In.pos, 1);
    Out.color = In.color;
    return Out;
}

float4 LineRendering2DPS(VertexOutput In) : SV_Target
{
    return In.color;
}

technique LineRendering3D
{
    pass PassFor3D
    {
        VertexShader = compile vs_2_0 LineRenderingVS();
        PixelShader  = compile ps_2_0 LineRenderingPS();
    }
}

technique LineRendering2D
{
    pass PassFor2D
    {
        VertexShader = compile vs_2_0 LineRendering2DVS();
        PixelShader  = compile ps_2_0 LineRendering2DPS();
    }
}
