import * as fs from "fs";

function doRemoveExport(dir, file) {
  const path = dir + "/" + file.fileName;
  const code = fs.readFileSync(path, "utf8").replace(/(^export\s.*$)/m, "// $1");
  fs.writeFileSync(path, code);
}

export function removeExport(userOptions = {}) {
  const files = userOptions.files || [];
  return {
    name: "removeExport",
    writeBundle(options, bundle) {
      for (let key in files) {
        const file = files[key];
        try {
          doRemoveExport(options.dir, bundle[file]);
        } catch (err) {
          console.warn(`Failed to remove export (file: '${file}')`, err);
        }
      }
    },
  };
}
