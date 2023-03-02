# MISRayTracingInUnity
 * Implement custom scriptable render pipeline for ray trace.
 * Include the material with closest hit shader in Peter Shirley's Ray Tracing series.
 * Apply MIS with specular and diffuse pdf.

### Work Framework Description
 * `Assets/RT Render Pipeline/Runtime/`includes core srp script `Ray Tracing Render Pipeline` and basic parent class `Ray Tracing Manager`.  Besides, two asset files are used to provide SO interface to manage properties in inspector. 
 * The mesh that needs to be drawn needs to be manually added to the list of SceneManager in the inspector, where `Assets/RT Render Pipeline/Runtime/SceneManager` takes charge of these thing.
 * Inherit and extend the parent class `Ray Tracing Manager` to implement a separate raytrace, including control of memory allocation, communication with shader, etc.

### Workflow 
 * Implement the ray trace among the three material of DIffuse, Dielectrics and Metal.
  
    ![image](https://github.com/qkyo/MISRayTracingInUnity/blob/main/RenderResultSet/Different%20Material.png)
    
 * Implement the cornell box using mixture pdf between based-on-light and based-on-random-normal-on-sphere.

 * Using random value to pick pdf btw specular and diffuse so that the direction is chosen properly.
    * spheres with 0.9 specular and 0.1 diffuse coefficient
    * cube with 0.9 diffuse and 0.1 specular coefficient
    * One bounce reflection without denoiser
 
    ![image](https://github.com/qkyo/MISRayTracingInUnity/blob/main/RenderResultSet/MIS.png)
    
    * Accumulate result
    
    ![image](https://github.com/qkyo/MISRayTracingInUnity/blob/main/RenderResultSet/MIS%20with%20accumulate%20frame.png)
