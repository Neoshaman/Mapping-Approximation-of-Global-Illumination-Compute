float4 unwrap(float2 uv, float w){
	uv = float2(uv.x, 1-uv.y);
	return float4((uv.xy*2)-1,1, w);
}


float4 float2to4_00(float2 number){
	return float4(number,0,0);
}
float4 float2to4_01(float2 number){
	return float4(number,0,1);
}
float4 float4_0001(){
	return float4(0,0,0,1);
}

float4 Float32ToIntrgba(float floatValue){
	const float toFixed = 255.0/256;
	float4 output;
	output.r = frac(floatValue*toFixed*1);
	output.g = frac(floatValue*toFixed*255);
	output.b = frac(floatValue*toFixed*255*255);
	output.a = frac(floatValue*toFixed*255*255*255);
	return output;
}
float IntrgbaToFloat32(float4 Value){
	const float fromFixed = 256.0/255;
	float input;
	input =
		 Value.r*fromFixed/(1)
		+Value.g*fromFixed/(255)
		+Value.b*fromFixed/(255*255)
		+Value.a*fromFixed/(255*255*255);
	return input;
}
//--------------------------------------------------------------------

//
// RGB / Full-range YCbCr conversions (ITU-R BT.601)
//
float3 RgbToYCbCr(float3 c){
    float Y  =  0.299 * c.r + 0.587 * c.g + 0.114 * c.b;
    float Cb = -0.169 * c.r - 0.331 * c.g + 0.500 * c.b;
    float Cr =  0.500 * c.r - 0.419 * c.g - 0.081 * c.b;
    return float3(Y, Cb, Cr);
}

float3 YCbCrToRgb(float3 c){
    float R = c.x + 0.000 * c.y + 1.403 * c.z;
    float G = c.x - 0.344 * c.y - 0.714 * c.z;
    float B = c.x - 1.773 * c.y + 0.000 * c.z;
    return float3(R, G, B);
}

const float3 toY  = float3( 0.299, 0.587, 0.114);
const float3 toCb = float3(-0.169,-0.331, 0.500);
const float3 toCr = float3( 0.500,-0.419,-0.081);

const float3 toR  = float3( 1.000, 0.000, 1.403);
const float3 toG  = float3( 1.000,-0.344,-0.714);
const float3 toB  = float3( 1.000,-0.773, 0.000);

//--------------------------------------------------------------------
float wrappedDiffuse(float wrap, float3 normal, float3 light){
    return saturate((dot(normal, light) + wrap) / ((1 + wrap) * (1 + wrap)));
}
float wrappedTo1(float3 normal, float3 light){
    return saturate((dot(normal, light) + 1)*0.25);// / 4);
}

float wrappedExtended(float wrap, float3 normal, float3 light, float power){
    // w is between 0 and 1
    // n is not -1
    return pow(saturate((dot(normal, light) + wrap) / (1.0f + wrap)), power) * (power + 1) / (2 * (1 + wrap));
}

float4 flatTex2D( sampler2D tex, float4 spos){
	//screenspace texture projection
	float2 uv = spos.xy / spos.w;
	//bgolus
	float4 cpos = UnityObjectToClipPos(float3(0,0,0));
	uv -= cpos.xy / cpos.w;
	uv *= cpos.w / UNITY_MATRIX_P._m11;// scale by depth and the fov
	uv.x *= _ScreenParams.x / _ScreenParams.y;// correct for aspect ratio, if that's a thing you want
	//uv = TRANSFORM_TEX(uv, tex);
	return tex2D(tex, uv);
}
float4 UnWrapToScreenSpace(float2 vertexUV, float4 vertexpos){
    float2 uv = float2(vertexUV.x, 1-vertexUV.y);
    return float4((uv.xy*2)-1,1,vertexpos.w);
}
float2 makeDiagonal(float2 uv, float width){
	float diagonal;
	diagonal = uv.x + uv.y;
	diagonal = diagonal / 2;
	diagonal = diagonal % 0.5;
	diagonal = step(diagonal, width);//width 0.49.... = thin;      0.1.... thick
	return diagonal;
}

//--------------------------------------------------------------------
float3x3 objectToTangent (float3 normal, float4 tangent){
	return float3x3(
		tangent.xyz,
		cross(normal, tangent.xyz) * tangent.w,
		normal
	);
}
float3 tangentViewDir (float3 normal, float4 tangent, float4 vertex){
	return normalize(mul(objectToTangent(normal, tangent),ObjSpaceViewDir(vertex)));
}

float3 boxproject(float2 uv, float3 direction){
	//_EnviCubeMapPos	– the cubemap origin position
	const float3 center = 0.5;
	//_BBoxMax			– the bounding volume (bounding box) of the environment
	const float3 bmax = 0;//1;
	//_BBoxMin			– the bounding volume (bounding box) of the environment
	const float3 bmin = 1;//0;
	//V 				– the vertex/fragment position in world space
	float3 pos = float3(uv,0.5);
	//L 				– the normalized vertex-to-light vector in world space

	//Working in World Coordinate System.
	//vec3 intersectMaxPointPlanes = (_BBoxMax - V) / L;
	//vec3 intersectMinPointPlanes = (_BBoxMin - V) / L;
	float3 invdir = 1 / direction;
	float3 imax = ( bmax - pos ) * invdir; // / direction;
	float3 imin = ( bmin - pos ) * invdir; // / direction;
	
	// Looking only for intersections in the forward direction of the ray.    
	//vec3 largestRayParams = max(intersectMaxPointPlanes, intersectMinPointPlanes);
	float3 m = max(imax,imin);
	
	// Smallest value of the ray parameters gives us the intersection.
	//float dist = min(min(largestRayParams.x, largestRayParams.y), largestRayParams.z);
	float dist = min(min(m.x,m.y),m.z);
	
	// Find the position of the intersection point.
	//vec3 intersectPositionWS = V + L * dist;
	float3 ipos = pos + direction * dist;
	
	// Get the local corrected vector.
	//Lp = intersectPositionWS - _EnviCubeMapPos;
	return normalize(ipos - center);
}
float3 BoxProjectVector(float3 wpos, float3 wvect, float3 cubecenter, float3 cubemin, float3 cubemax){
    //wvect should be normalized
    float3 bmax = (cubemax - wpos) / wvect;
    float3 bmin = (cubemin - wpos) / wvect;
    float3 minmax = (wvect > 0) ? bmax : bmin;
    float f = min(min(minmax.x,minmax.y),minmax.z);

    wpos -= cubecenter;
    return wpos + wvect * f;
}

//--------------------------------------------------------------------
//--------------------------------------------------------------------
//---Mali 400 MP
//-max texture size = 4096
//-support shadow
//-no multi sample texture
//-32 bit index buffer false
//-shader level: 30
//-multi threaded
//-ogles 2.0
//-12/18Gflops
//---extension
// depth texture cubemap
//multi sample render to texture
//compressed palletted texture
//depth texture packed depth stencil
//frame buffer object depth stencil

//logicom l-ement 741
//480 x 800 = 384 000px
//android 4.4.4
//512 ram
//-> 300 MB left storage
// 224 after clean
//66.3hz refresh
//tex unit 8
//cortex a7
//armv7 rev5
//clock - 216-1200Mhz (912-1008)
//mali 400 MP 200 - 400 Mhz
//23M tri/s -> 383 333 tri per frame at 60fps
//210 Mpx/s -> 3.5Mpx per frame at 60fps (9.11 "fill" per pixel)
//***open world -> tex density 32px/m, 32MB ram (1/16th)

//SH (seblargarde)
//for each pixel of cubemap
//  -pixel light
//  -weight (solid angle pixel)
//  -normal toward pixel
//   ---
//  SH array(shlight)
// chaque: magic number x light x weight (sample x delta "is area") x normal swizzle x optimization
//cosine factor array: array item x cosine factor item -> (convolution factor tied to surface property)
//get SH data
//-sh array
//-item result = magic number itemm (knorm?)
//x normal swizzle x surface cosine factor x item SH array (loop SH array: accumulate items: result x diffuse)

//brdf texture
//           Light direction
//   nodtl--------------------------
//      ^             |             |
//      |             |             |
//      |light facing |light on side|
//      |             |             |
//      |             |             |
//f0°   |---------------------------| f90°
//      |             |             |
//      |             |             |
//      |shadow facing| side shadow |
//      |             |             |
//      |             |             |
//      O---------------------------> ndotv
//             shadow direction

//bilinear noise tile
//x----y
//|  | |
//|--p-|-->t
//|  | |
//n----z
//   |
//   v
// facteur
//n = rand.xyzw
// i1 = lerp n.x, n.y, facteur
// i2 = lerp n.z, n.w, facteur
//p = lerp (i1, i2)
//--> i.xy = lerp n.xz, n.yw, facteur
//p = lerp i.x, i.y, t

//( index  / bit position in power of two [starting at 0] ) % 2 = 1 or 0
//( number / pulse ) % 2 = 1 or 0

//A-> shader core x clock speed = cycle per second (*0.8 for realistic number) -> fragment processing cycle per second
//B-> frame height x frame width = pixel per frame
//     x frame rate = pixel per second
//     x 2.5 = average overdrawn pixel per second
//A / B = average number cycle a fragment can be

//Blit is expensive: render directly in framebuffer (direct rendering)


//--------------------------------------------------------------------
//hemi-octahedral mapping
float3 decodeFromHemiOct(float2 v){
    float2 t = float2(v.x + v.y, v.x - v.y);
    return normalize(float3(t, 2.0 - abs(t.x) - abs(t.y)));
}
float2 encodeToHemiOct(float3 v) {
    float2 t = v.xy * (1.0 / (abs(v.x) + abs(v.y) + abs(v.z)));
    return float2(t.x + t.y, t.x - t.y); // in [-1,1]^2
}

float2 PackNormalHemiOctEncode(float3 n){
    float l1norm = dot(abs(n), 1.0);
    float2 res = n.xy * (1.0 / l1norm);

    return float2(res.x + res.y, res.x - res.y);
}
float3 UnpackNormalHemiOctEncode(float2 f){
    float2 val = float2(f.x + f.y, f.x - f.y) * 0.5;
    float3 n = float3(val, 1.0 - dot(abs(val), 1.0));

    return normalize(n);
}

//Octohedral mapping
float3 OctahedronToVector( float2 Oct ){
	float3 N = float3( Oct, 1.0 - dot( 1.0, abs( Oct ) ) );
	if(N.z< 0 )
		{
			N.xy = ( 1 - abs( N.yx) ) * (N.xy >= 0 ? 1.0 : -1.0 );
		}
	return normalize( N);
}
float2 VectortoOctahedron( float3 N ){
	N /= dot( 1.0, abs( N ) ); // Equivalent to：N/= abs(N.x)+abs(N.y)+abs(N.z)
	if( N.z <= 0 )
		{
			N.xy = ( 1 - abs( N.yx ) ) * ( N.xy >= 0 ? 1.0 : -1.0 );
		}
	return N.xy;
}			

float3 UnpackNormalFromOct(float2 f){
    float3 n = float3(f.x, f.y, 1.0 - abs(f.x) - abs(f.y));
    float t = max(-n.z, 0.0);
    n.xy += n.xy >= 0.0 ? -t.xx : t.xx;
    return normalize(n);
}
float2 PackNormalToOct(float3 n){
    n *= rcp(dot(abs(n), 1.0));
    float t = saturate(-n.z);
    return n.xy + (n.xy >= 0.0 ? t : -t);
}

float3 UnpackNormalOctQuadEncode(float2 f){
    float3 n = float3(f.x, f.y, 1.0 - abs(f.x) - abs(f.y));

    //float2 val = 1.0 - abs(n.yx);
    //n.xy = (n.zz < float2(0.0, 0.0) ? (n.xy >= 0.0 ? val : -val) : n.xy);

    // Optimized version of above code:
    float t = max(-n.z, 0.0);
    n.xy += n.xy >= 0.0 ? -t.xx : t.xx;

    return normalize(n);
}
float2 PackNormalOctQuadEncode(float3 n){
    //float l1norm    = dot(abs(n), 1.0);
    //float2 res0     = n.xy * (1.0 / l1norm);

    //float2 val      = 1.0 - abs(res0.yx);
    //return (n.zz < float2(0.0, 0.0) ? (res0 >= 0.0 ? val : -val) : res0);

    // Optimized version of above code:
    n *= rcp(dot(abs(n), 1.0));
    float t = saturate(-n.z);
    return n.xy + (n.xy >= 0.0 ? t : -t);
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