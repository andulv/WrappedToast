let viewerInstance = null;

export function initialize(viewerElement, options) {
    if (viewerInstance)
        throw new Error("Viewer instance already exists. Dispose it before initializing a new one.");
    if (!viewerElement)
        throw new Error("viewerElement is required for initialization.");

    options = options || {
        height: "100%",
        viewer: true,
    };
    options.el = viewerElement;
    viewerInstance = toastui.Editor.factory(options);
}

export function setMarkdown(viewerElement, markdown) {
    viewerInstance.setMarkdown(markdown);
}

export function setElementStyle(viewerElement, styles) {
    for (const property in styles) {
        if (Object.prototype.hasOwnProperty.call(styles, property)) {
            viewerElement.style[property] = styles[property];
        }
    }
}

export function dispose(viewerElement) {
    if (viewerInstance) {
        viewerInstance.destroy();
        viewerInstance = null;
    }
}
