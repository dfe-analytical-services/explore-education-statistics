(() => {
  class Copy {
    constructor($module) {
      this.$module = $module;
    }

    init() {
      // Bail if no clipboard support (e.g. IE11)
      if (!navigator.clipboard) {
        return;
      }

      const $button = document.createElement('button');
      $button.className = 'app-copy-button js-copy-button';
      $button.setAttribute('aria-live', 'assertive');
      $button.textContent = 'Copy code';

      this.$module.insertAdjacentElement('beforebegin', $button);

      $button.addEventListener('click', this.handleCopy);
    }

    handleCopy(event) {
      const target = event.target.nextElementSibling;

      navigator.clipboard
        .writeText(target.textContent)
        .then(() => {
          // eslint-disable-next-line no-param-reassign
          event.target.textContent = 'Code copied';

          setTimeout(() => {
            // eslint-disable-next-line no-param-reassign
            event.target.textContent = 'Copy code';
          }, 5000);
        })
        .catch(err => {
          console.error(err);
        });
    }
  }

  document.querySelectorAll('pre.highlight').forEach($el => {
    new Copy($el).init();
  });
})();
