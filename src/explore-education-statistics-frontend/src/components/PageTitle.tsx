import React from 'react';

interface Props {
  caption?: string;
  title: string;
}

const PageTitle = ({ caption, title }: Props) => {
  return (
    <>
      <h1 className="govuk-heading-xl" data-testid={`page-title ${title}`}>
        {caption && <span className="govuk-caption-xl">{caption}</span>}
        {title}
      </h1>
    </>
  );
};

export default PageTitle;
