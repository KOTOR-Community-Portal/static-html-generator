import { __deps } from "../dist/deps.js";

/**
 * Checks if an identifier is reserved for internal use
 * @param {string} name JavaScript identifier
 * @returns True if {@link name} is a reserved identifier
 */
function __isReserved(name) {
  return (
    /^__/.test(name) ||
    ["console", "ctx", "deps", "doc", "html", "utils"].includes(name)
  );
}

/**
 * Parses JavaScript code for top-level function declarations
 *
 * @param {string} code JavaScript code
 * @param {Function|undefined} validator Callback function to validate additional requirements
 * @returns {string} Code containing components in {@link code}
 */
function __parseFunctions(code, validator) {
  const ast = __deps.esprima.parseScript(code, { loc: true, range: true });
  const fns = [];
  Object.values(ast.body).forEach((x) => {
    const type = x.type;
    const where = JSON.stringify(x.loc.start);
    if (type !== "FunctionDeclaration") {
      throw new Error(`Not a function declaration ${where}`);
    }
    const name = x.id.name;
    if (__isReserved(name)) {
      throw new Error(`The name '${name}' is reserved ${where}`);
    }
    if (validator) {
      validator(x);
    }
    fns.push({
      code: code.slice(x.range.start, x.range.end),
      start: x.loc.start,
      end: x.loc.end,
    });
  });
  // Preserve line numbers
  let currentLine = 1;
  return fns
    .map((fn) => {
      const emptyLines = fn.start - currentLine;
      currentLine = fn.end + 1;
      return "\n".repeat(emptyLines) + fn.code + "\n";
    })
    .join("");
}

/**
 * Parses JavaScript code for top-level function declarations which are valid components
 *
 * @param {string} code JavaScript code
 * @returns {string} Code containing components in {@link code}
 */
export function __parseComponents(code) {
  return __parseFunctions(code);
}

/**
 * Parses JavaScript code for top-level function declarations which are valid templates
 *
 * @param {string} code JavaScript code
 * @returns {string} Code containing templates in {@link code}
 */
export function __parseTemplates(code) {
  return __parseFunctions(code, (x) => {
    const where = JSON.stringify(x.loc.start);
    if (x.params.length > 0) {
      throw new Error(`Template function should not take arguments ${where}`);
    }
  });
}
