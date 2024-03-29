
// float4 traceRay ( sampler2d directlight, sampler2d skybox, float4 sample, float2 octdirection, float3 normalDirection ){
float4 traceRay ( sampler2D directlight, sampler2D skybox, float4 sample, float2 octdirection, float3 normalDirection ){
  //box sample cubemap to get uv
  //use uv to sample direct lightmap to get illumination
  float4 direct    = tex2D   ( directlight, sample.rg );
  //mask sample by skybox mask //the sample would try to get uv invalid on LM
         direct   *= 1 - sample.b;
  
  //attenuate by distance //and sample cosine? //and fog (how)?
  //--
  //resolve sample BRDF //++++ take wnormal at sample
  //--

  //sample skybox
  float4 sky       = tex2D    ( skybox, octdirection ); //use the worldnormal to oct
  //mask skybox by skybox mask //mask part fromdirect light
         sky      *= sample.b;

  //we gonna use the ambient for now
  float wrapsky = wrappedTo1(float3(0,1,0), normalDirection);//invoke actual wrap normal
  _Ambientsky *= sample.b * wrapsky;
  sky = sky + _Ambientsky;// ?

  //return sample + skybox
  return sky + direct;
}

//struct to pass hash position
//haspos
//hash offset
//hash id

//function hash projection

// //sampler struct
// //scene
// sampler2D _Atlas;
// sampler2D _LMdirect;
// //buffer
// sampler2D _Accumulation;        //not used yet - this shader accumulate to
// sampler2D _Display;             
// //lmgb
// sampler2D _Albedo;              //not used - see lmdirect
// sampler2D _Wnormal;
// sampler2D _Wposition;           //not used yet - possibly attenuation
// sampler2D _Shadowmask; 