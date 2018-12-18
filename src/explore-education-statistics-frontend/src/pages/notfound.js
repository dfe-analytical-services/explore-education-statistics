import React, { Component } from 'react';

class NotFound extends Component {
    render() {
      return (
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-two-thirds">
            <h1 className="govuk-heading-xl">Page not found</h1>
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

  export default NotFound;