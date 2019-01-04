import React, { Component } from 'react';
import { H1 } from '../components/Heading';

class NotFoundPage extends Component {
  public render() {
    return (
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <H1>Page not found</H1>
          <p className="govuk-body">
            If you typed the web address, check it is correct.
          </p>
          <p className="govuk-body">
            If you pasted the web address, check you copied the entire address.
          </p>
        </div>
      </div>
    );
  }
}

export default NotFoundPage;
