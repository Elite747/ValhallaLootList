interface TitleManager {
    getTitle(): string;
    setTitle(title: string): void;
}

interface Window {
    titleManager: TitleManager;
}

window.titleManager = {
    getTitle: () => document.title,
    setTitle: (title: string) => document.title = title
};