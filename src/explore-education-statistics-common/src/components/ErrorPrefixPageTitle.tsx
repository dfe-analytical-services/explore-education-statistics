import React, { Component } from 'react';
import { Helmet } from 'react-helmet';

class ErrorPrefixPageTitle extends Component {
  public componentWillUnmount() {
    document.title = document.title.replace(/ERROR: /g, '');
  }

  public render() {
    document.title = `ERROR: ${document.title.replace(/ERROR: /g, '')}`;
    return (
      <Helmet>
        <title>ERROR: {document.title.replace(/ERROR: /g, '')}</title>
      </Helmet>
    );
  }
}

export default ErrorPrefixPageTitle;
