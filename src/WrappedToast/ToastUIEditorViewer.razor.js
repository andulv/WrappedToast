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
    const toBlob = (str) => new Blob([new TextEncoder().encode(str)]);
    instance.__wrappedToastMarkdown = resolvedOptions.initialValue ?? '';

    return {
        //Methods in native components exposed as is
        setMarkdown: (markdown) => {
            instance.__wrappedToastMarkdown = markdown ?? '';
            instance.setMarkdown(markdown);
        },
        isViewer: () => instance.isViewer(),
        isMarkdownMode: () => instance.isMarkdownMode(),
        isWysiwygMode: () => instance.isWysiwygMode(),

        // Wrapper-owned read methods
        getMarkdown: () => toBlob(instance.__wrappedToastMarkdown),
        getHTML: () => {
            const contentElement = viewerElement.querySelector('.toastui-editor-contents');
            return toBlob(contentElement.innerHTML);
        },
        setElementStyle: (styles) => setToastUiElementStyle(viewerElement, styles),
        dispose: () => disposeToastUiInstance(viewerElement, viewerInstances)
    };
}
