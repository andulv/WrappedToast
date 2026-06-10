const TOAST_UI_SCRIPT_PATH = "_content/WrappedToast/lib/toastui-editor/toastui-editor-all.min.js";
const TOAST_UI_STYLESHEET_PATHS = [
    "_content/WrappedToast/lib/toastui-editor/toastui-editor.min.css",
    "_content/WrappedToast/lib/toastui-editor/theme/toastui-editor-dark.min.css"
];

function defaultHtmlResolver(element) {
    const contentElement = element.querySelector('.toastui-editor-contents');
    return contentElement?.innerHTML ?? '';
}

function getLoaderState() {
    const loaderKey = "__wrappedToastToastUiLoader";
    const state = globalThis[loaderKey];
    if (state) {
        return state;
    }

    const nextState = { promise: null };
    globalThis[loaderKey] = nextState;
    return nextState;
}

function hasAssetTag(tagName, assetPath) {
    return Array.from(document.head.querySelectorAll(tagName)).some((element) => {
        const value = element.getAttribute(tagName === "link" ? "href" : "src");
        return value === assetPath;
    });
}

function ensureStylesheet(href) {
    if (hasAssetTag("link", href)) {
        return;
    }

    const link = document.createElement("link");
    link.rel = "stylesheet";
    link.href = href;
    document.head.appendChild(link);
}

function waitForExistingScript(script) {
    return new Promise((resolve, reject) => {
        if (globalThis.toastui?.Editor) {
            resolve();
            return;
        }

        script.addEventListener("load", () => resolve(), { once: true });
        script.addEventListener("error", () => reject(new Error(`Failed to load ${TOAST_UI_SCRIPT_PATH}`)), { once: true });
    });
}

function ensureScript(src) {
    if (globalThis.toastui?.Editor) {
        return Promise.resolve();
    }

    const existingScript = Array.from(document.head.querySelectorAll("script")).find((element) => {
        return element.getAttribute("src") === src;
    });

    if (existingScript) {
        return waitForExistingScript(existingScript);
    }

    return new Promise((resolve, reject) => {
        const script = document.createElement("script");
        script.src = src;
        script.addEventListener("load", () => resolve(), { once: true });
        script.addEventListener("error", () => reject(new Error(`Failed to load ${src}`)), { once: true });
        document.head.appendChild(script);
    });
}

export async function ensureToastUiAssets() {
    const state = getLoaderState();
    if (!state.promise) {
        state.promise = (async () => {
            TOAST_UI_STYLESHEET_PATHS.forEach(ensureStylesheet);
            await ensureScript(TOAST_UI_SCRIPT_PATH);

            if (!globalThis.toastui?.Editor) {
                throw new Error("TOAST UI assets loaded without exposing toastui.Editor.");
            }
        })().catch((error) => {
            state.promise = null;
            throw error;
        });
    }

    await state.promise;
}

export function getToastUiInstance(element, instanceStore, componentName) {
    const instance = instanceStore.get(element);
    if (!instance) {
        throw new Error(`${componentName} instance is not initialized for this element.`);
    }

    return instance;
}

export async function initializeToastUiInstance(element, instanceStore, componentName, options, createInstance, onInitialized) {
    if (!element) {
        throw new Error(`${componentName} element is required for initialization.`);
    }

    await ensureToastUiAssets();

    const existingInstance = instanceStore.get(element);
    if (existingInstance) {
        existingInstance.destroy();
        instanceStore.delete(element);
    }

    const instance = createInstance(options);
    instanceStore.set(element, instance);

    if (onInitialized) {
        onInitialized(instance);
    }
}

export function getToastUiHtml(element, instance, htmlResolver = defaultHtmlResolver) {
    if (typeof instance.getHTML === 'function') {
        return instance.getHTML();
    }

    return htmlResolver(element);
}

export async function copyPlainTextToClipboard(text) {
    await navigator.clipboard.writeText(text);
}

export async function copyRichTextToClipboard(html, plainText) {
    if (navigator.clipboard?.write && globalThis.ClipboardItem) {
        const clipboardItem = new ClipboardItem({
            'text/html': new Blob([html], { type: 'text/html' }),
            'text/plain': new Blob([plainText], { type: 'text/plain' })
        });

        await navigator.clipboard.write([clipboardItem]);
        return;
    }

    await copyPlainTextToClipboard(plainText);
}

export function setToastUiElementStyle(element, styles) {
    for (const property in styles) {
        if (Object.prototype.hasOwnProperty.call(styles, property)) {
            element.style[property] = styles[property];
        }
    }
}

export function disposeToastUiInstance(element, instanceStore, onDisposed) {
    const instance = instanceStore.get(element);
    if (instance) {
        instance.destroy();
        instanceStore.delete(element);
    }

    if (onDisposed) {
        onDisposed();
    }
}