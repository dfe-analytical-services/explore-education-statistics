import Page from '@admin/components/Page';
import Link from '@admin/components/Link';
import React from 'react';

const PageNotFoundPage = () => {
  return (
    <Page title="Page not found">
      <p className="govuk-body">
        If you typed the web address, check it is correct.
      </p>
      <p className="govuk-body">
        If you pasted the web address, check you copied the entire address.
      </p>
      <p>
        If the web address is correct or you clicked a link or button and ended
        up on this page,{' '}
        <Link className="govuk-link" to="/contact-us">
          contact our Explore education statistics team
        </Link>{' '}
        if you need any help or support.
      </p>
    </Page>
  );
};

export default PageNotFoundPage;
