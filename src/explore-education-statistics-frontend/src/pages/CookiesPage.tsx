import React, { Component } from 'react';
import { H1 } from '../components/Heading';

class CookiesPage extends Component {
  public render() {
    return (
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <H1>Cookies</H1>
        </div>
      </div>
    );
  }
}

export default CookiesPage;
