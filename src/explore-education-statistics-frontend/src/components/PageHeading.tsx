import React, { ReactNode } from 'react';

interface Props {
  caption: string;
  heading: string;
}

const PageHeading = ({ caption, heading }: Props) => {
  return (
    <h1>
      <span className="govuk-caption-xl">{caption}</span>
      {heading}
    </h1>
  );
};

export default PageHeading;
