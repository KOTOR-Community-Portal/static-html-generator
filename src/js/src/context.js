const ctx = Object.seal({
  page: {},
  site: {},
});

/**
 * Loads a page into the global context object {@link ctx}
 * @param {string} json
 */
export function __loadPage(json) {
  ctx.page = Object.freeze({ ...JSON.parse(json), ids: {} });
}

/**
 * Loads site into the global context object {@link ctx}
 * @param {string} json
 */
export function __loadSite(json) {
  ctx.site = Object.freeze(JSON.parse(json));
}

export { ctx };
