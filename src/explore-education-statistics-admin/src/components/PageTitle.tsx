import classNames from 'classnames';
import React from 'react';
import { Helmet } from 'react-helmet';

interface Props {
  caption?: string;
  className?: string;
  title: string;
}

const PageTitle = ({ caption, className, title }: Props) => {
  return (
    <>
      <Helmet>
        <title>{`${title} - Explore education statistics - GOV.UK`}</title>
      </Helmet>

      {caption && (
        <span className="govuk-caption-xl" data-testid="page-title-caption">
          {caption}
        </span>
      )}

      <h1
        className={classNames('govuk-heading-xl', className)}
        data-testid="page-title"
      >
        {title}
      </h1>
    </>
  );
};

export default PageTitle;
