shader_type spatial;

uniform vec3 glow_color: source_color = vec3(0.2, 0.6, 0.3);
uniform float glow_power: hint_range(0.0, 8.0, 0.1) = 2.0;

void fragment() {
	ALBEDO = glow_color * glow_power;
	//ALBEDO = glow_color;
}