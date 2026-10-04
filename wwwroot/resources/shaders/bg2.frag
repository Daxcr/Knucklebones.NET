#ifdef GL_ES
precision mediump float;
#endif

varying vec2 vTextureCoord;
uniform float u_time;
uniform vec2 u_resolution;

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

const vec4 base = vec4(0.05098, 0.05098, 0.07843, 1.0);
const vec4 overlay = vec4(0.06667, 0.06667, 0.10588, 1.0);
const vec4 overlay2 = vec4(0.19216, 0.19608, 0.26667, 1.0);
const vec4 border = vec4(1.0, 1.0, 1.0, 1.0);

const vec4 gradMauve = vec4(0.79608, 0.65098, 0.96863, 1.0);
const vec4 gradRed = vec4(0.95294, 0.54510, 0.65882, 1.0);
const vec4 gradPeach = vec4(0.98039, 0.70196, 0.52941, 1.0);
const vec4 gradYellow = vec4(0.97647, 0.88627, 0.68627, 1.0);
const vec4 gradGreen = vec4(0.65098, 0.89020, 0.63137, 1.0);
const vec4 gradTeal = vec4(0.58039, 0.88627, 0.83529, 1.0);
const vec4 gradSapphire = vec4(0.45490, 0.78039, 0.92549, 1.0);
const vec4 gradBlue  = vec4(0.53725, 0.70588, 0.98039, 1.0);
const vec4 gradLavender = vec4(0.70588, 0.74510, 0.99608, 1.0);

void main() {
    vec2 uv = gl_FragCoord.xy / u_resolution;
    vec4 colour = base;
    vec4 gradient = vec4(0.0);

    float nse = fbm((gl_FragCoord.xy / 600.0) + (u_time / 20.0));
    nse += fbm((gl_FragCoord.xy / 1100.0) - (u_time / 44.0));
    if (nse > 1.0) {
        colour = overlay;
        if (mod(nse, 0.1) > 0.05) {
            colour = mix(colour, overlay2, nse - 1.0);
        }

        if (nse < 1.005) {
            colour = border;
        }
    }

    float speed = 0.05;
    float t = fract((uv.x + uv.y) * 0.15 + u_time * speed);
    float segment = t * 9.0;

    if (segment < 1.0) {
        gradient = mix(gradMauve, gradRed, segment);
    } else if (segment < 2.0) {
        gradient = mix(gradRed, gradPeach, segment - 1.0);
    } else if (segment < 3.0) {
        gradient = mix(gradPeach, gradYellow, segment - 2.0);
    } else if (segment < 4.0) {
        gradient = mix(gradYellow, gradGreen, segment - 3.0);
    } else if (segment < 5.0) {
        gradient = mix(gradGreen, gradTeal, segment - 4.0);
    } else if (segment < 6.0) {
        gradient = mix(gradTeal, gradSapphire, segment - 5.0);
    } else if (segment < 7.0) {
        gradient = mix(gradSapphire, gradBlue, segment - 6.0);
    } else if (segment < 8.0) {
        gradient = mix(gradBlue, gradLavender, segment - 7.0);
    } else {
        gradient = mix(gradLavender, gradMauve, segment - 8.0);
    }

    vec4 fgradient = mix(base, gradient, (noise(gl_FragCoord.xy / 1000.0) * 0.25) + 0.75);

    if (nse < 1.0){
        colour = mix(colour, gradient, clamp(((nse) * 20.0) - 19.0, 0.0, 1.0));
        colour = mix(colour, fgradient, clamp(((nse) * 5.0) - 4.6, 0.0, 1.0));
        colour = mix(colour, fgradient, clamp(((nse) * 2.5) - 2.2, 0.0, 1.0));
    }

    gl_FragColor = colour;
}