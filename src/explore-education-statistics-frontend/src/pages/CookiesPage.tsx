import React, { Component } from 'react';
import { Helmet } from 'react-helmet';

class CookiesPage extends Component {
  public render() {
    return (
      <div className="govuk-grid-row">
        <Helmet>
          <title>Cookies - GOV.UK</title>
        </Helmet>
        <div className="govuk-grid-column-two-thirds">
          <h1>Cookies</h1>
        </div>
      </div>
    );
  }
}

export default CookiesPage;
