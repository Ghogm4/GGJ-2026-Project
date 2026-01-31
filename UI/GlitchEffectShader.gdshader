shader_type canvas_item;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;

// --- 频率控制 ---
uniform float glitch_speed : hint_range(1.0, 60.0) = 15.0;      // 干扰跳变的速度 (Hz)
uniform float occurrence_chance : hint_range(0.0, 1.0) = 0.05; // 每一跳产生干扰的概率

// --- 视觉强度 ---
uniform float dot_density : hint_range(0.0, 0.002, 0.0001) = 0.0002; // 白点密度
uniform float line_intensity : hint_range(0.0, 1.0) = 0.3;          // 线条亮度
uniform float line_thickness : hint_range(0.0, 0.01) = 0.001;        // 线条粗细

float random(vec2 uv) {
    return fract(sin(dot(uv.xy, vec2(12.9898, 78.233))) * 43758.5453123);
}

void fragment() {
    vec4 color = texture(SCREEN_TEXTURE, SCREEN_UV);
    
    // 使用 glitch_speed 自由调节每秒更新多少次随机状态
    float time_step = floor(TIME * glitch_speed);
    float frame_gen = random(vec2(time_step));
    
    // 判定：这一跳是否属于“干扰时刻”
    if (frame_gen < occurrence_chance) {
        
        // 1. 随机白点 (颗粒感)
        float p = random(SCREEN_UV + frame_gen);
        if (p > 1.0 - dot_density) {
            color.rgb = mix(color.rgb, vec3(1.0), 0.7);
        }
        
        // 2. 随机横线 (信号毛刺)
        float line_y = random(vec2(frame_gen, 0.88));
        float line_dist = abs(SCREEN_UV.y - line_y);
        
        if (line_dist < line_thickness) {
            // 给线条增加横向的断裂感和随机透明度
            float horiz_noise = random(vec2(SCREEN_UV.x, time_step));
            if (horiz_noise > 0.4) {
                // 使用线性减淡（Addition），避免生硬的覆盖
                color.rgb += vec3(line_intensity * horiz_noise);
            }
        }
    }

    COLOR = vec4(clamp(color.rgb, 0.0, 1.0), color.a);
}