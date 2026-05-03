import { __deps } from "../dist/deps.js";

/**
 * Tagged template literal function for writing inline HTML with JSX-like syntax
 *
 * Based on
 * https://blog.jim-nielsen.com/2019/jsx-like-syntax-for-tagged-template-literals/
 *
 * @param {string[]} strings String literals
 * @param {...*} values Values to insert
 */
export function html(strings, ...values) {
  let result = "";
  for (let i = 0; i < strings.length; ++i) {
    const string = strings[i];
    const value = values[i];
    if (Array.isArray(value)) {
      result += string + value.join("");
    } else if (typeof value === "string" || typeof value === "number") {
      result += string + value;
    }
    // Discard values of other types
    else {
      result += string;
    }
  }
  return result;
}

const __html = Object.freeze({
  adjacentPosition: {
    beforebegin: 0,
    afterbegin: 1,
    beforeend: 2,
    afterend: 3,
  },

  query: Object.freeze({
    cell: "th, td",
    heading: "h1, h2, h3, h4, h5, h6",
    sectioning: "article, aside, nav, section",
  }),

  /**
   * Appends a text node with the specified content to an HTML element.
   * @param {IElement} el HTML element
   * @param {string} text Text to append
   * @returns {INode} Text node which was created
   */
  appendTextNode(el, text) {
    return el.appendChild(globalThis.__doc.createTextNode(text));
  },

  /**
   * Divides HTML elements based on a selector,
   * isolating the matching elements and grouping the non-matching elements
   * @param {IElement[]} els HTML elements
   * @param {string} selector Query selector
   * @returns {Object.<element, list>[]} Elements of {@link els} divided into
   * groups which each contain either one element matching {@link selector} or a
   * list of elements which don't match
   */
  divide: function(els, selector) {
    let groups = [];
    let stack = [];
    for (const el of els) {
      if (el.matches(selector)) {
        if (stack.length > 0) {
          groups.push({ list: [...stack] });
          stack.length = 0;
        }
        groups.push({ element: el });
      } else {
        stack.push(el);
      }
    }
    if (stack.length > 0) {
      groups.push({ list: stack });
    }
    return groups;
  },

  /**
   * Formats arguments into one list of space delimited class names.
   *
   * @param {string[]} names Class names
   * @return {string} Class attribute value
   */
  formatClass(...names) {
    return [...names]
      .flat()
      .map((x) => x ? x.split(" ") : [])
      .flat()
      .filter((x) => x)
      .join(" ");
  },

  /**
   * Gives an ID to an HTML element which is unique in the page context {@link ctx.page}
   *
   * If {@link value} is not specified, an ID will be generated from the element's text.
   *
   * @param {IElement} el HTML element
   * @param {string|undefined} value Identifier
   */
  giveId(el, value) {
    const id = value || __deps.slug(el.textContent || el.innerText);
    el.setAttribute("id", globalThis.utils.html.id(id));
  },

  /**
   * Groups HTML elements based on a selector,
   * with each matched element in separate arrays
   * @param {IElement[]} els HTML elements
   * @param {string} selector Query selector
   * @returns {IElement[][]} Elements of {@link els} grouped into arrays which
   * each contain one element matching {@link selector} and subsequent elements
   * which don't match
   */
  group: function(els, selector) {
    let groups = [];
    let stack = [];
    for (const el of els) {
      if (el.matches(selector)) {
        if (stack.length > 0) {
          groups.push(stack);
        }
        stack = [el];
      } else {
        stack.push(el);
      }
    }
    if (stack.length > 0) {
      groups.push(stack);
    }
    return groups;
  },

  /**
   * Gets an identifier which is unique in the page context {@link ctx.page}
   *
   * @param {string} value Identifier
   * @returns {string} Unique identifier
   */
  id(value, opts) {
    const { index } = opts || {};
    const ids = globalThis.ctx.page.ids;
    const count = (ids[value] || 0) + 1;
    ids[value] = count;
    return index || count > 1 ? `${value}-${count}` : value;
  },

  /**
   * Gets a mapping of keys to HTML identifiers which are unique in the page context {@link ctx.page}
   *
   * @param {string[][]} items List of key/value pairs; if value is empty it will be generated from key
   * @returns {object.<string, string>} Mapping of {@link keys} to unique identifiers
   */
  ids(entries) {
    const htmlUtils = globalThis.utils.html;
    return Object.fromEntries(entries.map((x) => {
      const [k, v] = Array.isArray(x) ? x : [x];
      return [k, htmlUtils.id(v || k)];
    }));
  },

  /**
   * Adds a temporary element to a document, calls a callback function on it, then removes it
   * @param {string} content HTML content
   * @param {Function} callbackFn Function processes an element
   * @returns {*} Return value of {@link callbackFn} if any
   */
  parse: function (content, callbackFn) {
    const el = globalThis.__doc.createElement("div");
    el.innerHtml = content;
    const result = callbackFn(el);
    el.remove();
    return result;
  },

  /**
   * Replaces an HTML element with a new element having the specified tag.
   * @param {IElement} el HTML element to replace
   * @param {string} name HTML tag
   * @returns {IElement} The HTML element which was replaced
   */
  replace: function(el, name) {
    const newEl = globalThis.__doc.createElement(name);
    for (const a of el.attributes) {
      newEl.setAttribute(a.name, a.value);
    }
    newEl.innerHtml = el.innerHtml;
    el.parent.replaceChild(newEl, el);
    return newEl;
  },

  /**
   * Generates a valid HTML identifier based on the specified string
   *
   * @param {string} string Text
   * @returns {string} HTML identifier
   */
  slug: function (string) {
    return __deps.slug(string);
  },

  /**
   * Creates an element which wraps other HTML elements as their parent
   *
   * Wrapper element will replace the first child in the DOM.
   * @param {IElement[]} children HTML elements
   * @param {string} outerHtml Wrapper HTML
   * @returns {IElement} Wrapper element
   */
  wrap: function (children, outerHtml) {
    const htmlUtils = globalThis.utils.html;
    const first = children[0];
    first.insert(htmlUtils.adjacentPosition.beforebegin, outerHtml);
    const wrapper = first.previousSibling;
    children.forEach((x) => wrapper.appendChild(x));
    return wrapper;
  },
});

export { __html };
