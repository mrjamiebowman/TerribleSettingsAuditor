// Copies the tab's static files next to the esbuild output in dist/tab.
import { copyFileSync, mkdirSync } from "node:fs";

mkdirSync("dist/tab", { recursive: true });

for (const file of ["tab.html", "tab.css"]) {
    copyFileSync(`src/tab/${file}`, `dist/tab/${file}`);
}
