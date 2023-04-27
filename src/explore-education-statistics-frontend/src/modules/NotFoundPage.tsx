import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import { NextPage } from 'next';
import React from 'react';

const NotFoundPage: NextPage = () => {
  return (
    <Page title="Page not found">
      <p>If you typed the web address, check it's correct.</p>
      <p>
        If you cut and pasted the web address, check you copied the entire
        address.
      </p>
      <p>
        If the web address is correct or you clicked a link or button and ended
        up on this page,{' '}
        <Link to="/contact-us">
          contact our Explore education statistics team
        </Link>{' '}
        if you need any help or support.
      </p>
    </Page>
  );
};

export default NotFoundPage;
