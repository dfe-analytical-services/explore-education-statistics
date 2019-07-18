import React from 'react';

// eslint-disable-next-line @typescript-eslint/prefer-interface
export type FootnoteProps = {
  content: string[];
};

const Link = ({ content }: FootnoteProps) => {
  const footnoteList = content.map(function createFootnotes(foot, index) {
    return (
      // eslint-disable-next-line react/no-array-index-key
      <p className="govuk-body-s govuk-!-margin-bottom-1" key={index}>
        {foot}
      </p>
    );
  });
  return <div className="govuk-!-margin-bottom-8">{footnoteList}</div>;
};

export default Link;
