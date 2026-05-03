import * as fs from "fs";

function doAddExport(dir, file, items) {
  const path = dir + "/" + file.fileName;
  const code =
    `${fs.readFileSync(path, "utf8")}\n` + `export { ${items.join(", ")} };\n`;
  fs.writeFileSync(path, code);
}

export function addExport(userOptions = {}) {
  const exports = userOptions.exports || [];
  return {
    name: "addExport",
    writeBundle(options, bundle) {
      for (let key in exports) {
        const { file, items } = exports[key];
        try {
          doAddExport(options.dir, bundle[file], items);
        } catch (err) {
          console.warn(`Failed to add export (file: '${file}')`, err);
        }
      }
    },
  };
}
