import React from 'react';
import { Helmet } from 'react-helmet';

interface Props {
  caption?: string;
  title: string;
}

const PageTitle = ({ caption, title }: Props) => {
  return (
    <>
      <Helmet>
        <title>{`${title} - Explore education statistics - GOV.UK`}</title>
      </Helmet>

      <h1 className="govuk-heading-xl" data-testid={`page-title ${title}`}>
        {caption && (
          <span className="govuk-caption-xl" data-testid="page-title-caption">
            {caption}
          </span>
        )}
        {title}
      </h1>
    </>
  );
};

export default PageTitle;
