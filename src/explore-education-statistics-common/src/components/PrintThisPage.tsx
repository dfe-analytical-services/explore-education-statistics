import React from 'react';
import classNames from 'classnames';

const PrintThisPage = () => {
  return (
    <div className={classNames('govuk-!-margin-top-6', 'dfe-print-hidden')}>
      <a href="#" onClick={() => window.print()}>
        Print this page
      </a>
    </div>
  );
};

export default PrintThisPage;
