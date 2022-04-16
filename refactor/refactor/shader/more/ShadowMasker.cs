using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowMasker
{
    //generate shadow mask type
        //1. array of shadowmap for each TOD --online ++
        //2. zonal harmonics --offline
        //3. raytracer --
        //4. unity texture reader --offline

        //5. cubemap shadow (sampling shadow from local cubemap mask) --online ++
        //6. linear atlas (baking direction index in a tile for each pixel) --offline

        //7. array of lightmap --offline
        //8. bitplan compression shadow layer (32 per texture pixel) --offline

        //1,2,3,4->7->8


        //1. 256² -> 2048² -> 8² tiles (64)
        //get mesh
        //for each angles - draw mesh on atlas
}