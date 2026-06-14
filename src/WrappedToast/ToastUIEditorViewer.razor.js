import {
    disposeToastUiInstance,
    getToastUiInstance,
    initializeToastUiInstance,
    setToastUiElementStyle
} from './toastui-loader.js';

const viewerInstances = new WeakMap();

export async function initialize(viewerElement, options) {
    const resolvedOptions = {
        height: "100%",
        initialEditType: 'wysiwyg',
        viewer: true,
        ...(options ?? {})
    };

    resolvedOptions.el = viewerElement;

    await initializeToastUiInstance(
        viewerElement,
        viewerInstances,
        'viewer',
        resolvedOptions,
        (instanceOptions) => new globalThis.toastui.Editor.factory(instanceOptions)
    );

    const instance = getToastUiInstance(viewerElement, viewerInstances, 'viewer');

    return {
        setMarkdown: (markdown) => instance.setMarkdown(markdown),
        isViewer: () => instance.isViewer(),
        isMarkdownMode: () => instance.isMarkdownMode(),
        isWysiwygMode: () => instance.isWysiwygMode(),
        off: (type) => instance.off(type),
        setElementStyle: (styles) => setToastUiElementStyle(viewerElement, styles),
        dispose: () => disposeToastUiInstance(viewerElement, viewerInstances)
    };
}
