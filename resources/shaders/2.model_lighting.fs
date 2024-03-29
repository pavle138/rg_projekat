#version 330 core
out vec4 FragColor;

struct PointLight {
    vec3 position;

    vec3 specular;
    vec3 diffuse;
    vec3 ambient;

    float constant;
    float linear;
    float quadratic;
};
struct SpotLight {
    vec3 position;
    vec3 direction;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;

    float cutoff;
    float cutoff_outer;

    float constant;
    float linear;
    float quadratic;
};

struct Material {
    sampler2D texture_diffuse1;
    sampler2D texture_specular1;
    sampler2D texture_normal1;
    float shininess;
};
in vec2 TexCoords;
in vec3 Normal;
in vec3 FragPos;
in vec3 TangentLightPos;
in vec3 TangentViewPos;
in vec3 TangentFragPos;
in vec3 SpotLightDir;
in vec3 SpotLightPos;
in vec3 PointLightPos;

uniform SpotLight spotLight;
uniform PointLight pointLight;
uniform Material material;
uniform bool blinn;

uniform vec3 viewPosition;

vec3 CalcPointLight(vec3 PointLightPos,PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir)
{
    vec3 lightDir = normalize(PointLightPos - fragPos);
    // diffuse shading
    float diff = max(dot(normal, lightDir), 0.0);
    // specular shading
    vec3 reflectDir = reflect(-lightDir, normal);
    vec3 halfwayDir=normalize(lightDir+viewDir);
    float spec=0.0;
    if(blinn){
       spec = pow(max(dot(viewDir, halfwayDir), 0.0), material.shininess);
    }
    else{
       spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    }
    // attenuation
    float distance = length(PointLightPos - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));
    // combine results
    vec3 ambient = light.ambient * vec3(texture(material.texture_diffuse1, TexCoords));
    vec3 diffuse = light.diffuse * diff * vec3(texture(material.texture_diffuse1, TexCoords));
    vec3 specular = light.specular * spec * vec3(texture(material.texture_specular1, TexCoords).xxx);
    ambient *= attenuation;
    diffuse *= attenuation;
    specular *= attenuation;
    return (ambient + diffuse + specular);
}
vec3 CalcSpotLight(vec3 SpotLightDir,vec3 SpotLightPos,SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir)
{
    vec3 lightDir = normalize(SpotLightPos - fragPos);

    float diff = max(dot(normal, lightDir), 0.0);

    vec3 reflectDir = reflect(-lightDir, normal);
    vec3 halfwayDir=normalize(lightDir+viewDir);
    float spec=0.0;
    if(blinn){
           spec = pow(max(dot(viewDir, halfwayDir), 0.0), material.shininess);
    }
    else{
           spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    }

    float distance = length(SpotLightPos - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));

    float teta=dot(lightDir,normalize(-SpotLightDir));
    float epsilon=light.cutoff-light.cutoff_outer;
    float intensity=clamp((teta-light.cutoff_outer)/epsilon,0.0,1.0);

    vec3 ambient = light.ambient * vec3(texture(material.texture_diffuse1, TexCoords));
    vec3 diffuse = light.diffuse * diff * vec3(texture(material.texture_diffuse1, TexCoords));
    vec3 specular = light.specular * spec * vec3(texture(material.texture_specular1, TexCoords).xxx);
    ambient *= attenuation*intensity;
    diffuse *= attenuation*intensity;
    specular *= attenuation*intensity;
    return (ambient + diffuse + specular);
}

void main()
{
    vec3 normal = texture(material.texture_normal1,TexCoords).rgb;
    normal=normalize(normal*2 -1.0);
    vec3 viewDir = normalize(TangentViewPos - TangentFragPos);
    vec3 result = CalcPointLight(PointLightPos,pointLight, normal, TangentFragPos, viewDir);
    result+=CalcSpotLight(SpotLightDir,SpotLightPos,spotLight,normal,TangentFragPos,viewDir);
    //     if(vec4(texture(material.texture_diffuse1,TexCoords)).a <0.3){
    //         discard;
    //     }
    FragColor = vec4(result, vec4(texture(material.texture_diffuse1,TexCoords)).a);
}