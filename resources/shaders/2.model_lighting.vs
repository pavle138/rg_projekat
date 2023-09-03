#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoords;
layout (location = 3) in vec3 aTangent;
layout (location = 4) in vec3 biTangent;

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

out vec2 TexCoords;
out vec3 Normal;
out vec3 FragPos;
out vec3 TangentLightPos;
out vec3 TangentViewPos;
out vec3 TangentFragPos;
out vec3 SpotLightDir;
out vec3 SpotLightPos;
out vec3 PointLightPos;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

uniform PointLight pointLight;
uniform SpotLight spotLight;
uniform vec3 viewPos;

void main()
{
    FragPos = vec3(model * vec4(aPos, 1.0));
    Normal = aNormal;
    TexCoords = aTexCoords;

    mat3 normalMatrix = transpose(inverse(mat3(model)));
    vec3 T = normalize(normalMatrix * aTangent);
    vec3 N = normalize(normalMatrix * aNormal);

    T = normalize(T-dot(T,N)*N);
    vec3 B = cross(N,T);

    mat3 TBN = transpose(mat3(T,B,N));
    SpotLightDir = TBN * spotLight.direction;
    SpotLightPos= TBN * spotLight.position;
    PointLightPos= TBN * pointLight.position;
    TangentViewPos = TBN * viewPos;
    TangentFragPos = TBN * FragPos;

    gl_Position = projection * view * vec4(FragPos, 1.0);
}