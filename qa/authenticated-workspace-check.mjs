import { mkdir, writeFile } from "node:fs/promises";
import path from "node:path";

const [port = "9335", baseUrl = "http://127.0.0.1:5271", outputDir = ".qa-workspaces"] = process.argv.slice(2);
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

const wait = milliseconds => new Promise(resolve => setTimeout(resolve, milliseconds));
const navigate = async route => {
    await send("Page.navigate", { url: `${baseUrl}${route}` });
    await wait(1600);
};

await send("Page.enable");
await send("Runtime.enable");
await send("Network.enable");

const setViewport = async (width, height, mobile = false) => {
    await send("Emulation.setDeviceMetricsOverride", {
        width,
        height,
        deviceScaleFactor: 1,
        mobile,
        screenWidth: width,
        screenHeight: height
    });
};

const login = async (email, password) => {
    await send("Network.clearBrowserCookies");
    await navigate("/Account/Login");
    await send("Runtime.evaluate", {
        expression: `(() => {
            const email = document.querySelector('[name="Email"]');
            const password = document.querySelector('[name="Password"]');
            email.value = ${JSON.stringify(email)};
            password.value = ${JSON.stringify(password)};
            email.dispatchEvent(new Event('input', { bubbles: true }));
            password.dispatchEvent(new Event('input', { bubbles: true }));
            email.closest('form').requestSubmit();
        })()`
    });
    await wait(1900);
};

const report = [];
const capture = async (name, route, viewport) => {
    await setViewport(viewport.width, viewport.height, viewport.mobile);
    await navigate(route);
    const evaluation = await send("Runtime.evaluate", {
        returnByValue: true,
        expression: `(() => ({
            route: location.pathname,
            title: document.title,
            heading: document.querySelector('h1')?.innerText || '',
            viewport: innerWidth,
            scrollWidth: document.documentElement.scrollWidth,
            failedImages: [...document.images].filter(image => image.complete && image.naturalWidth === 0).length,
            overflow: [...document.querySelectorAll('body *')]
                .filter(element => {
                    const style = getComputedStyle(element);
                    const box = element.getBoundingClientRect();
                    return style.position !== 'fixed' && style.overflowX !== 'auto' && (box.right > innerWidth + 2 || box.left < -2);
                })
                .slice(0, 12)
                .map(element => ({ tag: element.tagName, className: String(element.className).slice(0, 100), parentClass: String(element.parentElement?.className || '').slice(0, 100), text: String(element.textContent || '').trim().slice(0, 80), left: Math.round(element.getBoundingClientRect().left), right: Math.round(element.getBoundingClientRect().right) })),
            unnamedControls: [...document.querySelectorAll('button,input,select,textarea,a[href]')]
                .filter(element => !((element.innerText || element.value || element.getAttribute('aria-label') || element.getAttribute('title') || element.getAttribute('placeholder') || '').trim()))
                .length
        }))()`
    });
    const result = { name, ...evaluation.result.value };
    report.push(result);
    const screenshot = await send("Page.captureScreenshot", { format: "png", fromSurface: true });
    await writeFile(path.join(outputDir, `${name}.png`), Buffer.from(screenshot.data, "base64"));
};

await setViewport(1440, 1000, false);
await login("stallbazar.organizer@gmail.com", "Organizer@12345");
await capture("organizer-desktop", "/Dashboard/Organizer", { width: 1440, height: 1000, mobile: false });
await capture("event-builder-desktop", "/Events/Create", { width: 1440, height: 1000, mobile: false });
await capture("event-builder-mobile", "/Events/Create", { width: 390, height: 844, mobile: true });

await setViewport(1440, 1000, false);
await login("stallbazar.vendor@gmail.com", "Vendor@12345");
await capture("vendor-desktop", "/Dashboard/Vendor", { width: 1440, height: 1000, mobile: false });
await capture("event-details-desktop", "/Events/Details/1", { width: 1440, height: 1000, mobile: false });
await capture("vendor-mobile", "/Dashboard/Vendor", { width: 390, height: 844, mobile: true });

await writeFile(path.join(outputDir, "report.json"), JSON.stringify(report, null, 2));
process.stdout.write(JSON.stringify(report, null, 2));
socket.close();
