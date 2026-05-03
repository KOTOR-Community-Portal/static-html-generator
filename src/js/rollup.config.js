"use strict";

import commonjs from "@rollup/plugin-commonjs";
import { nodeResolve } from "@rollup/plugin-node-resolve";
import { addExport } from "./src/build/rollup-plugin-add-export.js";
import { removeExport } from "./src/build/rollup-plugin-remove-export.js";

export default [
  {
    input: "src/deps.js",
    output: {
      name: "__deps",
      dir: "dist",
      format: "iife",
    },
    plugins: [
      commonjs(),
      nodeResolve({ browser: true }),
      addExport({
        exports: [{ file: "deps.js", items: ["__deps"] }],
      }),
    ],
  },
  {
    input: "src/index.js",
    output: {
      dir: "dist",
      format: "es",
    },
    plugins: [
      removeExport({
        files: ["index.js"],
      }),
    ],
  },
];
