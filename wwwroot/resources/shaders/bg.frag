#ifdef GL_ES
precision mediump float;
#endif

varying vec2 vTextureCoord;
uniform float u_time;
uniform vec2 u_resolution;
uniform vec4 u_shaderBase;
uniform vec4 u_shaderPrimary;
uniform vec4 u_shaderAccent;

float noise(vec2 p) {
	vec2 p2 = mod(p, 10.0);
	return sin(fract(sin(dot(p2, vec2(127.1, 311.7))) * 43758.5453123) * 3.141592654);
}

float smoothNoise(vec2 p) {
	vec2 i = floor(p);
	vec2 f = fract(p);
	vec2 u = f * f * (3.0 - 2.0 * f);
	return mix(mix(noise(i), noise(i + vec2(1.0, 0.0)), u.x), mix(noise(i + vec2(0.0, 1.0)), noise(i + vec2(1.0, 1.0)), u.x), u.y);
}

float fbm(vec2 p) {
	float total = 0.0;
	float amplitude = 0.5;
	float frequency = 1.0;
	for (int i = 0; i < 2; i++) {
		total += amplitude * smoothNoise(p * frequency);
		frequency *= 2.0;
		amplitude *= 0.5;
	}
	return total;
}

void main() {
    vec2 uv = gl_FragCoord.xy / u_resolution;
    vec4 colour = vec4(0.0, 0.0, 0.0, 0.0);

    float bg = fbm((gl_FragCoord.xy / 100.0) + (0.1 * u_time));
    bg += fbm((gl_FragCoord.xy / 140.0) - (0.067 * u_time));

    bg += noise((gl_FragCoord.xy + u_time) / 1000.0) * 0.04;
    bg += fbm(vec2(gl_FragCoord.x / 100.0, gl_FragCoord.x / 2.0)) * 0.05;

    bg /= 2.05;

    vec2 distanceVector = abs(uv - 0.5);
    float dist = (distanceVector.x * distanceVector.x) + (distanceVector.y * distanceVector.y);

    bg *= dist * 1.5;

    float bg1 = mix(bg, fbm(gl_FragCoord.xy / 200.0), 0.1);
    float bg2 = mix(bg, fbm(gl_FragCoord.xy / 500.0), 0.1);

    if (bg > 0.15) {
        colour = u_shaderBase;
    }

    if (bg1 > 0.235) {
        colour = u_shaderAccent;
    }
    
    if (bg1 > 0.24) {
        colour = vec4(0.0, 0.0, 0.0, 1.0);
    }

    if (bg2 > 0.25) {
        colour = u_shaderPrimary;
    }

    gl_FragColor = colour;
}