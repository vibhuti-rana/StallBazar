import { mkdir, writeFile } from "node:fs/promises";
import path from "node:path";

const [port = "9225", baseUrl = "http://127.0.0.1:5271", outputDir = ".qa-output"] = process.argv.slice(2);
await mkdir(outputDir, { recursive: true });

const tab = await fetch(`http://127.0.0.1:${port}/json/new?about:blank`, { method: "PUT" }).then(response => response.json());
const socket = new WebSocket(tab.webSocketDebuggerUrl);
const pending = new Map();
let commandId = 0;

await new Promise((resolve, reject) => {
    socket.addEventListener("open", resolve, { once: true });
    socket.addEventListener("error", reject, { once: true });
});

socket.addEventListener("message", event => {
    const message = JSON.parse(event.data);
    if (!message.id || !pending.has(message.id)) return;
    const { resolve, reject } = pending.get(message.id);
    pending.delete(message.id);
    if (message.error) reject(new Error(message.error.message));
    else resolve(message.result);
});

const send = (method, params = {}) => new Promise((resolve, reject) => {
    const id = ++commandId;
    pending.set(id, { resolve, reject });
    socket.send(JSON.stringify({ id, method, params }));
});

await send("Page.enable");
await send("Runtime.enable");
await send("Emulation.setDeviceMetricsOverride", {
    width: 390,
    height: 844,
    deviceScaleFactor: 1,
    mobile: true,
    screenWidth: 390,
    screenHeight: 844
});
await send("Emulation.setTouchEmulationEnabled", { enabled: true, maxTouchPoints: 5 });

const routes = [
    ["home", "/"],
    ["events", "/Events"],
    ["event-details", "/Events/Details/1"],
    ["login", "/Account/Login"],
    ["register", "/Account/Register"],
    ["forgot-password", "/Account/ForgotPassword"],
    ["resend-verification", "/Account/ResendVerification"],
    ["about", "/Home/About"],
    ["contact", "/Home/Contact"],
    ["privacy", "/Home/Privacy"]
];
const report = [];

for (const [name, route] of routes) {
    await send("Page.navigate", { url: `${baseUrl}${route}` });
    await new Promise(resolve => setTimeout(resolve, 1800));
    const evaluation = await send("Runtime.evaluate", {
        returnByValue: true,
        expression: `(() => {
            const overflow = [...document.querySelectorAll('body *')]
                .filter(element => {
                    const style = getComputedStyle(element);
                    const box = element.getBoundingClientRect();
                    return style.position !== 'fixed' && (box.right > innerWidth + 2 || box.left < -2);
                })
                .slice(0, 12)
                .map(element => ({ tag: element.tagName, className: String(element.className).slice(0, 100), right: Math.round(element.getBoundingClientRect().right) }));
            const unnamedControls = [...document.querySelectorAll('button,input,select,textarea,a[href]')]
                .filter(element => !((element.innerText || element.value || element.getAttribute('aria-label') || element.getAttribute('title') || element.getAttribute('placeholder') || '').trim()))
                .length;
            return {
                route: location.pathname,
                title: document.title,
                viewport: innerWidth,
                scrollWidth: document.documentElement.scrollWidth,
                overflow,
                failedImages: [...document.images].filter(image => image.complete && image.naturalWidth === 0).length,
                unnamedControls,
                menuButtonVisible: !!document.querySelector('.navbar-toggler') && getComputedStyle(document.querySelector('.navbar-toggler')).display !== 'none'
            };
        })()`
    });
    const result = evaluation.result.value;
    report.push(result);
    const screenshot = await send("Page.captureScreenshot", { format: "png", fromSurface: true });
    await writeFile(path.join(outputDir, `${name}-mobile.png`), Buffer.from(screenshot.data, "base64"));
}

await writeFile(path.join(outputDir, "browser-report.json"), JSON.stringify(report, null, 2));
process.stdout.write(JSON.stringify(report, null, 2));
socket.close();
