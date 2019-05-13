import React from 'react';

const PrintThisPage = () => {
  return (
    <div className="govuk-!-margin-top-6">
      <a href="#" onClick={() => window.print()}>
        Print this page
      </a>
    </div>
  );
};

export default PrintThisPage;
