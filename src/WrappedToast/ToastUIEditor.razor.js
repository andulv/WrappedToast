let editorInstance = null;

export function initialize(editorElement, options) {
    if (editorInstance)
        throw new Error("Editor instance already exists. Dispose it before initializing a new one.");
    if (!editorElement)
        throw new Error("editorElement is required for initialization.");

    options = options || {
        height: "100%",
    };
    options.el = editorElement;
    editorInstance = new toastui.Editor(options);
}

export function getMarkdown(editorElement) {
    return editorInstance.getMarkdown();
}

export function setMarkdown(editorElement, markdown) {
    editorInstance.setMarkdown(markdown);
}

export function setElementStyle(editorElement, styles) {
    for (const property in styles) {
        if (Object.prototype.hasOwnProperty.call(styles, property)) {
            editorElement.style[property] = styles[property];
        }
    }
}

export function dispose(editorElement) {
    if (editorInstance) {
        editorInstance.destroy();
        editorInstance = null;
    }
}
