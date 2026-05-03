import { ctx, __loadPage, __loadSite } from "./context.js";
import { html } from "./html.js";
import { __parseComponents, __parseTemplates } from "./parser.js";
import { utils, __loadUtil } from "./utils.js";

globalThis.ctx = ctx;
globalThis.utils = utils;

export {
  ctx,
  utils,
  html,
  __loadPage,
  __loadSite,
  __loadUtil,
  __parseComponents,
  __parseTemplates,
};
