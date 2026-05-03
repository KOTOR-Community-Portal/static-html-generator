import { __html } from "./html.js";
import { __site } from "./site.js";

const utils = Object.seal({
  fs: {},
  html: __html,
  site: __site,
});

/**
 * Exposes members of an embedded object to the global {@link utils} object
 * @param {String} key {@link utils} key
 * @param {String} name Name of embedded object
 */
export function __loadUtil(key, name) {
  const oldUtil = utils[key];
  const util = globalThis[name] || {};
  utils[key] = util;
  Object.assign(util, oldUtil);
}

export { utils };
