shader_type spatial;

render_mode specular_toon;

uniform sampler2D noise;
uniform sampler2D normalmap;
uniform float height_scale = 0.5;
uniform vec3 color: source_color = vec3(0.05, 0.11, 0.27);

varying vec2 tex_position;

void fragment() {
	float fresnel = sqrt(1.0 - dot(NORMAL, VIEW));
	//NORMAL_MAP = texture(normalmap, tex_position).xyz;
	RIM = 0.2;
	METALLIC = 0.0;
	ROUGHNESS = 0.01 * (1.0 - fresnel);
	//ALBEDO = vec3(0.01, 0.03, 0.05) + (0.1 * fresnel);
	ALBEDO = color + (0.1 * fresnel);
}

float wave(vec2 position) {
	position += texture(noise, position / 10.0).x * 2.0 - 1.0;
	vec2 wv = 1.0 - abs(sin(position));
	return pow(1.0 - pow(wv.x * wv.y, 0.65) , 4.0);
}

float height(vec2 position, float time) {
	//vec2 offset = 0.01 * cos(position + time);
	//return texture(noise, (position / 10.0) - offset).x;

	//return wave(position * 0.4);

	float d = wave((position + time) * 0.4) * 0.3;
	d += wave((position - time) * 0.3) * 0.3;
	d += wave((position + time) * 0.5) * 0.2;
	d += wave((position - time) * 0.6) * 0.2;
	return d;
}

void vertex() {
	//VERTEX.y += cos(VERTEX.x * 4.0) * sin(VERTEX.z * 4.0);

	tex_position = VERTEX.xz / 2.0 + 0.5;
	vec2 pos = VERTEX.xz;
	float k = height(tex_position, TIME);
	NORMAL = normalize(vec3(k - height(pos + vec2(0.1, 0.0), TIME), 0.1, k - height(pos + vec2(0.0, 0.1), TIME)));
	//VERTEX.y += k * height_scale;
	VERTEX.y += k;
}
