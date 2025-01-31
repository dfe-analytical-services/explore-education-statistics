import React from 'react';

interface Props {
  caption?: string;
  title: string;
}

const PageTitle = ({ caption, title }: Props) => {
  return (
    <>
      {caption && (
        <span className="govuk-caption-xl" data-testid="page-title-caption">
          {caption}
        </span>
      )}

      <h1 className="govuk-heading-xl" data-testid="page-title">
        {title}
      </h1>
    </>
  );
};

export default PageTitle;
