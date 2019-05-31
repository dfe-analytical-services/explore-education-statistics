/**
 * Add the Hotjar tracking code to the DOM.
 *
 * @param {string} hotjarId
 * @param {number} snippetVersion
 */
export default (hotjarId, snippetVersion) => {
  /* eslint-disable */
  (function(h, o, t, j, a, r) {
    h.hj =
      h.hj ||
      function() {
        (h.hj.q = h.hj.q || []).push(arguments);
      };
    h._hjSettings = { hjid: hotjarId, hjsv: snippetVersion };
    a = o.getElementsByTagName('head')[0];
    r = o.createElement('script');
    r.async = 1;
    r.src = t + h._hjSettings.hjid + j + h._hjSettings.hjsv;
    a.appendChild(r);
  })(window, document, '//static.hotjar.com/c/hotjar-', '.js?sv=');
};
