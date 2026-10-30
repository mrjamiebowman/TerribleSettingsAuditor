import { rmSync } from "node:fs";

for (const path of ["dist", "out", "tasks/TsaScreen/index.js", "tasks/TsaScreen/node_modules"]) {
    rmSync(path, { recursive: true, force: true });
}
