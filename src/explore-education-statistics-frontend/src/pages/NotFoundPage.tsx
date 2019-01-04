import React, { Component } from 'react';

class NotFoundPage extends Component {
  public render() {
    return (
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <h1>Page not found</h1>
          <p>If you typed the web address, check it is correct.</p>
          <p>
            If you pasted the web address, check you copied the entire address.
          </p>
        </div>
      </div>
    );
  }
}

export default NotFoundPage;
