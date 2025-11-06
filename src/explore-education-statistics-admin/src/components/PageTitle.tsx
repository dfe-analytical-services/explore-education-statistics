import classNames from 'classnames';
import React from 'react';
import PageMetaTitle from './PageMetaTitle';

interface Props {
  caption?: string;
  className?: string;
  metaTitle?: string;
  title: string;
}

const PageTitle = ({ caption, className, metaTitle, title }: Props) => {
  return (
    <>
      <PageMetaTitle title={metaTitle ?? title} />

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
