const canvas = document.querySelector(".bg-shader");
const sandbox = new GlslCanvas(canvas);

function resize() {
    canvas.width = window.innerWidth;
    canvas.height = window.innerHeight;
    sandbox.setUniform("u_resolution", canvas.width, canvas.height);
}
function hexToVec4(input, alpha = 1.0) {
    input = input.trim();

    const rgbMatch = input.match(/rgba?\(\s*([\d.]+)[,\s]+([\d.]+)[,\s]+([\d.]+)(?:[,\s]+([\d.]+))?\s*\)/i);
    if (rgbMatch) {
        const r = parseFloat(rgbMatch[1]) / 255;
        const g = parseFloat(rgbMatch[2]) / 255;
        const b = parseFloat(rgbMatch[3]) / 255;
        const a = rgbMatch[4] !== undefined ? parseFloat(rgbMatch[4]) : alpha;
        return [r, g, b, a];
    }

    if (input.startsWith("#")) {
        const r = parseInt(input.slice(1, 3), 16) / 255;
        const g = parseInt(input.slice(3, 5), 16) / 255;
        const b = parseInt(input.slice(5, 7), 16) / 255;
        return [r, g, b, alpha];
    }

    console.warn("hexToVec4: unrecognized colour format:", input);
    return [0, 0, 0, 0];
}

window.addEventListener("resize", resize);

resize();

let startTime = Date.now();
let lastTimeShader = performance.now();

function animate() {

    const now = Date.now();
    const elapsed = (now - startTime) / 1000.0;

    sandbox.setUniform("u_time", elapsed);

    let colour = getComputedStyle(document.documentElement).getPropertyValue("--shader-base").trim();
    let [r, g, b, a] = hexToVec4(colour);
    sandbox.setUniform("u_shaderBase", r, g, b, 1.0);

    colour = getComputedStyle(document.documentElement).getPropertyValue("--shader-primary").trim();
    [r, g, b, a] = hexToVec4(colour);
    sandbox.setUniform("u_shaderPrimary", r, g, b, 1.0);

    colour = getComputedStyle(document.documentElement).getPropertyValue("--shader-accent").trim();
    [r, g, b, a] = hexToVec4(colour);
    sandbox.setUniform("u_shaderAccent", r, g, b, 1.0);

    requestAnimationFrame(animate);
}

animate();