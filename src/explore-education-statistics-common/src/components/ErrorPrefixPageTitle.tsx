import React, { useEffect } from 'react';
import Head from 'next/head';

const ErrorPrefixPageTitle = () => {
  useEffect(() => {
    document.title = `ERROR: ${document.title.replace(/ERROR: /g, '')}`;
    return () => {
      document.title = document.title.replace(/ERROR: /g, '');
    };
  }, []);

  return (
    <Head>
      <title>ERROR: {document.title.replace(/ERROR: /g, '')}</title>
    </Head>
  );
};

export default ErrorPrefixPageTitle;
