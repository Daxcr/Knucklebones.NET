let canvases;
let instances = [];

function RefreshInstances() {
    canvases = document.querySelectorAll(".bg-shader");
    instances = Array.from(canvases).map(canvas => ({ canvas, sandbox: new GlslCanvas(canvas) }));
    Resize();
}

window.addEventListener("display:refresh", RefreshInstances);

function Resize() {
    for (const { canvas, sandbox } of instances) {
        canvas.width = canvas.clientWidth || window.innerWidth;
        canvas.height = canvas.clientHeight || window.innerHeight;
        sandbox.setUniform("u_resolution", canvas.width, canvas.height);
    }
}

function HexToVec4(input, alpha = 1.0) {
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
    return [0, 0, 0, 0];
}

window.addEventListener("resize", Resize);

let startTime = Date.now();

function Animate() {
    const now = Date.now();
    const elapsed = (now - startTime) / 1000.0;

    const rootStyle = getComputedStyle(document.documentElement);

    let colour = rootStyle.getPropertyValue("--shader-base").trim();
    let [rBase, gBase, bBase] = HexToVec4(colour);

    colour = rootStyle.getPropertyValue("--shader-primary").trim();
    let [rPrimary, gPrimary, bPrimary] = HexToVec4(colour);

    colour = rootStyle.getPropertyValue("--shader-accent").trim();
    let [rAccent, gAccent, bAccent] = HexToVec4(colour);

    for (const { sandbox } of instances) {
        sandbox.setUniform("u_time", elapsed);
        sandbox.setUniform("u_shaderBase", rBase, gBase, bBase, 1.0);
        sandbox.setUniform("u_shaderPrimary", rPrimary, gPrimary, bPrimary, 1.0);
        sandbox.setUniform("u_shaderAccent", rAccent, gAccent, bAccent, 1.0);
    }

    requestAnimationFrame(Animate);
}

Animate();