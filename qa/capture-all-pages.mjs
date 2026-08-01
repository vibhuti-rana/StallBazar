import { mkdir, writeFile } from "node:fs/promises";
import path from "node:path";

const [port = "9345", baseUrl = "http://127.0.0.1:5275", outputDir = "screenshots"] = process.argv.slice(2);
await mkdir(outputDir, { recursive: true });

const tab = await fetch(`http://127.0.0.1:${port}/json/new?about:blank`, { method: "PUT" }).then(response => {
    if (!response.ok) throw new Error(`Chrome debugging endpoint returned ${response.status}`);
    return response.json();
});
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

await send("Page.enable");
await send("Runtime.enable");
await send("Network.enable");
await send("Emulation.setDeviceMetricsOverride", {
    width: 1440,
    height: 1000,
    deviceScaleFactor: 1,
    mobile: false,
    screenWidth: 1440,
    screenHeight: 1000
});

const settlePage = async () => {
    await send("Runtime.evaluate", {
        awaitPromise: true,
        returnByValue: true,
        expression: `(async () => {
            await document.fonts?.ready;
            const pendingImages = [...document.images]
                .filter(image => !image.complete)
                .map(image => new Promise(resolve => {
                    image.addEventListener('load', resolve, { once: true });
                    image.addEventListener('error', resolve, { once: true });
                }));
            await Promise.race([
                Promise.all(pendingImages),
                new Promise(resolve => setTimeout(resolve, 7000))
            ]);
            await new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)));
            return true;
        })()`
    });
    await wait(250);
};

const navigate = async route => {
    const target = new URL(route, baseUrl).href;
    const navigation = await send("Page.navigate", { url: target });
    if (navigation.errorText) throw new Error(`Navigation failed for ${target}: ${navigation.errorText}`);
    await wait(900);
    await settlePage();
};

const evaluate = async expression => {
    const result = await send("Runtime.evaluate", { expression, returnByValue: true, awaitPromise: true });
    if (result.exceptionDetails) throw new Error(result.exceptionDetails.text || "Browser evaluation failed");
    return result.result.value;
};

const report = [];
const missing = [];

const capture = async (name, route, access) => {
    if (!route) {
        missing.push({ name, reason: "No valid route or backing record was available" });
        return;
    }

    await navigate(route);
    await evaluate(`(async () => {
        // Full-page captures should show the completed state of scroll-reveal animations.
        document.documentElement.classList.remove('reveal-ready');
        document.documentElement.style.scrollBehavior = 'auto';
        document.body.style.scrollBehavior = 'auto';
        const step = Math.max(500, Math.floor(innerHeight * 0.8));
        const bottom = Math.max(document.body.scrollHeight, document.documentElement.scrollHeight);
        for (let y = 0; y < bottom; y += step) {
            scrollTo(0, y);
            await new Promise(resolve => setTimeout(resolve, 90));
        }
        scrollTo(0, bottom);
        await new Promise(resolve => setTimeout(resolve, 180));
        scrollTo(0, 0);
        await new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)));
        return true;
    })()`);
    await wait(200);
    const page = await evaluate(`(() => ({
        url: location.href,
        path: location.pathname + location.search,
        title: document.title,
        heading: document.querySelector('h1')?.innerText?.trim() || '',
        bodyTextLength: document.body?.innerText?.trim().length || 0,
        failedImages: [...document.images].filter(image => image.complete && image.naturalWidth === 0).length,
        responseStatus: performance.getEntriesByType('navigation')[0]?.responseStatus || null
    }))()`);
    const metrics = await send("Page.getLayoutMetrics");
    const width = Math.max(1440, Math.ceil(metrics.contentSize.width));
    const height = Math.max(1000, Math.ceil(metrics.contentSize.height));
    const screenshot = await send("Page.captureScreenshot", {
        format: "png",
        fromSurface: true,
        captureBeyondViewport: true,
        clip: { x: 0, y: 0, width, height, scale: 1 }
    });
    const file = `${name}.png`;
    await writeFile(path.join(outputDir, file), Buffer.from(screenshot.data, "base64"));
    report.push({ name, file, requestedRoute: route, access, width, height, ...page });
};

const firstHref = async selector => evaluate(`(() => document.querySelector(${JSON.stringify(selector)})?.getAttribute('href') || null)()`);

const login = async (email, password) => {
    await send("Network.clearBrowserCookies");
    await navigate("/Account/Login");
    await evaluate(`(() => {
        const email = document.querySelector('[name="Email"]');
        const password = document.querySelector('[name="Password"]');
        if (!email || !password) throw new Error('Login form controls were not found');
        email.value = ${JSON.stringify(email)};
        password.value = ${JSON.stringify(password)};
        email.dispatchEvent(new Event('input', { bubbles: true }));
        password.dispatchEvent(new Event('input', { bubbles: true }));
        email.closest('form').requestSubmit();
        return true;
    })()`);
    await wait(1200);
    await settlePage();
    const result = await evaluate(`(() => ({ path: location.pathname, errors: [...document.querySelectorAll('.validation-summary-errors li')].map(x => x.innerText) }))()`);
    if (result.path === "/Account/Login") {
        throw new Error(`Login failed for ${email}: ${result.errors.join("; ") || "unknown error"}`);
    }
};

const ensureBookingForReview = async eventDetailsRoute => {
    await login("stallbazar.vendor@gmail.com", "Vendor@12345");
    await navigate(eventDetailsRoute);
    const submitted = await evaluate(`(() => {
        const form = document.querySelector('form[action*="/Bookings/Create"]');
        if (!form) return false;
        const note = form.querySelector('[name="vendorNote"]');
        if (note) note.value = 'Screenshot coverage request';
        form.requestSubmit();
        return true;
    })()`);
    if (submitted) {
        await wait(1200);
        await settlePage();
    }
    return submitted;
};

try {
    await send("Network.clearBrowserCookies");

    await capture("01-home", "/", "Public");
    await capture("02-events-index", "/Events/Index", "Public");
    const publicEventDetails = await firstHref('a[href*="/Events/Details/"]');
    await capture("03-event-details-public", publicEventDetails, "Public");
    await capture("04-about", "/Home/About", "Public");
    await capture("05-contact", "/Home/Contact", "Public");
    await capture("06-privacy", "/Home/Privacy", "Public");
    await capture("07-login", "/Account/Login", "Public");
    await capture("08-register", "/Account/Register", "Public");
    await capture("09-forgot-password", "/Account/ForgotPassword", "Public");
    await capture("10-resend-verification", "/Account/ResendVerification", "Public");
    await capture("11-reset-password", "/Account/ResetPassword?userId=preview&token=preview", "Public, preview token");
    await capture("12-access-denied", "/Account/AccessDenied", "Public");
    await capture("13-error", "/Home/Error", "Public");

    await login("stallbazar.organizer@gmail.com", "Organizer@12345");
    await navigate("/Dashboard/Organizer");
    let bookingReview = await firstHref('a[href*="/Bookings/Review/"]');
    const organizerEventDetails = await firstHref('a[href*="/Events/Details/"]');
    const organizerEventEdit = await firstHref('a[href*="/Events/Edit/"]');
    const stallCreate = await firstHref('a[href*="/Stalls/Create"]');

    if (!bookingReview && organizerEventDetails) {
        await ensureBookingForReview(organizerEventDetails);
        await login("stallbazar.organizer@gmail.com", "Organizer@12345");
        await navigate("/Dashboard/Organizer");
        bookingReview = await firstHref('a[href*="/Bookings/Review/"]');
    }

    await capture("14-organizer-dashboard", "/Dashboard/Organizer", "Organizer");
    await capture("15-event-details-organizer", organizerEventDetails, "Organizer");
    const stallEdit = await firstHref('a[href*="/Stalls/Edit/"]');
    await capture("16-event-create", "/Events/Create", "Organizer");
    await capture("17-event-edit", organizerEventEdit, "Organizer, owned event");
    await capture("18-stall-create", stallCreate, "Organizer, owned event");
    await capture("19-stall-edit", stallEdit, "Organizer, owned stall");
    await capture("20-booking-review", bookingReview, "Organizer, owned booking");

    await login("stallbazar.vendor@gmail.com", "Vendor@12345");
    await capture("21-vendor-dashboard", "/Dashboard/Vendor", "Vendor");
    await capture("22-event-details-vendor", organizerEventDetails || publicEventDetails, "Vendor");
    await capture("23-profile", "/Account/Profile", "Authenticated (Vendor)");
    await capture("24-settings", "/Account/Settings", "Authenticated (Vendor)");
    await capture("25-notifications", "/Notifications/Index", "Authenticated (Vendor)");

    await login("stallbazar.admin@gmail.com", "Admin@12345");
    await capture("26-admin-dashboard", "/Dashboard/Admin", "Admin");

    const routeInventory = {
        generatedAt: new Date().toISOString(),
        baseUrl,
        viewportWidth: 1440,
        captureMode: "Full page",
        screenshots: report,
        missing,
        nonPageActionsExcluded: [
            "POST form actions",
            "/Account/ConfirmEmail (redirect result only)",
            "/Account/Logout (POST redirect only)",
            "/Dashboard/Index (role redirect only)"
        ]
    };
    await writeFile(path.join(outputDir, "capture-report.json"), JSON.stringify(routeInventory, null, 2));
    process.stdout.write(JSON.stringify({ captured: report.length, missing, files: report.map(item => item.file) }, null, 2));
    if (missing.length) process.exitCode = 2;
} finally {
    socket.close();
}
