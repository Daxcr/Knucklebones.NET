attribute vec3 aPosition;

void main() {
    float x = (aPosition.x < 0.0) ? -1.0 : 1.0;
    float y = (aPosition.y < 0.0) ? -1.0 : 1.0;

    gl_Position = vec4(x, y, aPosition.z, 1.0);
}