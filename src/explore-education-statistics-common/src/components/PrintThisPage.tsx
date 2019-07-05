import React from 'react';
import classNames from 'classnames';
import printStyles from '@frontend/components/PrintCSS.module.scss';

const PrintThisPage = () => {
  return (
    <div className={classNames('govuk-!-margin-top-6', printStyles.hidden)}>
      <a href="#" onClick={() => window.print()}>
        Print this page
      </a>
    </div>
  );
};

export default PrintThisPage;
