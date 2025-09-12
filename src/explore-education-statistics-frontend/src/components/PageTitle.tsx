import React from 'react';

interface Props {
  caption?: string;
  captionInsideTitle?: boolean;
  title: string;
}

const PageTitle = ({ caption, captionInsideTitle = false, title }: Props) => {
  const captionElement = () =>
    caption ? (
      <span className="govuk-caption-xl" data-testid="page-title-caption">
        {caption}
      </span>
    ) : null;

  return (
    <>
      {!captionInsideTitle && captionElement()}

      <h1 className="govuk-heading-xl" data-testid="page-title">
        {captionInsideTitle && captionElement()}
        {title}
      </h1>
    </>
  );
};

export default PageTitle;
