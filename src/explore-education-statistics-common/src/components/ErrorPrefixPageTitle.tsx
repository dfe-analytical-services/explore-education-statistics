import React, { Component } from 'react';
import Head from 'next/head';

class ErrorPrefixPageTitle extends Component {
  public componentWillUnmount() {
    document.title = document.title.replace(/ERROR: /g, '');
  }

  public render() {
    document.title = `ERROR: ${document.title.replace(/ERROR: /g, '')}`;
    return (
      <Head>
        <title>ERROR: {document.title.replace(/ERROR: /g, '')}</title>
      </Head>
    );
  }
}

export default ErrorPrefixPageTitle;
