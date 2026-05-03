const __site = Object.freeze({
  /**
   * Gets the home page
   * @returns {Object.<path, title, template, parent, tokens>[]} Home page
   */
  get home() {
    return globalThis.ctx.site.pages["index.html"];
  },

  /**
   * Gets the ancestors of a page, in order from root to the page
   * @param {Object.<parent>} page Page
   * @returns {Object.<path, title, template, parent, tokens>[]} Ancestors of {@link page}
   */
  getAncestors: function (page) {
    let ancestors = [];
    const site = globalThis.ctx.site;
    let ancestor = site.pages[page.parent];
    while (ancestor) {
      ancestors.push(ancestor);
      ancestor = site.pages[ancestor.parent];
    }
    return ancestors;
  },
});

export { __site };
