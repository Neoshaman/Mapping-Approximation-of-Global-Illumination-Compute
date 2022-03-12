float3 UnpackNormalFromOct(float2 f){
    float3 n = float3(f.x, f.y, 1.0 - abs(f.x) - abs(f.y));
    float t = max(-n.z, 0.0);
    n.xy += n.xy >= 0.0 ? -t.xx : t.xx;
    return normalize(n);
}
float2 PackNormalToOct(float3 n)
{
    n *= rcp(dot(abs(n), 1.0));
    float t = saturate(-n.z);
    return n.xy + (n.xy >= 0.0 ? t : -t);
}

//TODO
//to test
float4 UnWrapToScreenSpace(float2 vertexUV, float4 vertexpos){
    float2 uv = float2(vertexUV.x, 1-vertexUV.y);
    return float4((uv.xy*2)-1,1,vertexpos.w);
}

float3 BoxProjectVector(float3 wpos, float3 wvect, float3 cubecenter, float3 cubemin, float3 cubemax)
{
    //wvect should be normalized
    float3 bmax = (cubemax - wpos) / wvect;
    float3 bmin = (cubemin - wpos) / wvect;
    float3 minmax = (wvect > 0) ? bmax : bmin;
    float f = min(min(minmax.x,minmax.y),minmax.z);

    wpos -= cubecenter;
    return wpos + wvect * f;
}

// Ref: http://www.vis.uni-stuttgart.de/~engelhts/paper/vmvOctaMaps.pdf
// Encode with Oct, this function work with any size of output
// return real between [-1, 1]
// real2 PackNormalOctRectEncode(real3 n)
// {
//     // Perform planar projection.
//     real3 p = n * rcp(dot(abs(n), 1.0));
//     real  x = p.x, y = p.y, z = p.z;

//     // Unfold the octahedron.
//     // Also correct the aspect ratio from 2:1 to 1:1.
//     real r = saturate(0.5 - 0.5 * x + 0.5 * y);
//     real g = x + y;

//     // Negative hemisphere on the left, positive on the right.
//     return real2(CopySign(r, z), g);
// }

// real3 UnpackNormalOctRectEncode(real2 f)
// {
//     real r = f.r, g = f.g;

//     // Solve for {x, y, z} given {r, g}.
//     real x = 0.5 + 0.5 * g - abs(r);
//     real y = g - x;
//     real z = max(1.0 - abs(x) - abs(y), REAL_EPS); // EPS is absolutely crucial for anisotropy

//     real3 p = real3(x, y, CopySign(z, r));

//     return normalize(p);
// }

// Ref: http://jcgt.org/published/0003/02/01/paper.pdf
// Encode with Oct, this function work with any size of output
// return float between [-1, 1]
float2 PackNormalOctQuadEncode(float3 n)
{
    //float l1norm    = dot(abs(n), 1.0);
    //float2 res0     = n.xy * (1.0 / l1norm);

    //float2 val      = 1.0 - abs(res0.yx);
    //return (n.zz < float2(0.0, 0.0) ? (res0 >= 0.0 ? val : -val) : res0);

    // Optimized version of above code:
    n *= rcp(dot(abs(n), 1.0));
    float t = saturate(-n.z);
    return n.xy + (n.xy >= 0.0 ? t : -t);
}

float3 UnpackNormalOctQuadEncode(float2 f)
{
    float3 n = float3(f.x, f.y, 1.0 - abs(f.x) - abs(f.y));

    //float2 val = 1.0 - abs(n.yx);
    //n.xy = (n.zz < float2(0.0, 0.0) ? (n.xy >= 0.0 ? val : -val) : n.xy);

    // Optimized version of above code:
    float t = max(-n.z, 0.0);
    n.xy += n.xy >= 0.0 ? -t.xx : t.xx;

    return normalize(n);
}

// real2 PackNormalHemiOctEncode(real3 n)
// {
//     real l1norm = dot(abs(n), 1.0);
//     real2 res = n.xy * (1.0 / l1norm);

//     return real2(res.x + res.y, res.x - res.y);
// }

// real3 UnpackNormalHemiOctEncode(real2 f)
// {
//     real2 val = real2(f.x + f.y, f.x - f.y) * 0.5;
//     real3 n = real3(val, 1.0 - dot(abs(val), 1.0));

//     return normalize(n);
// }