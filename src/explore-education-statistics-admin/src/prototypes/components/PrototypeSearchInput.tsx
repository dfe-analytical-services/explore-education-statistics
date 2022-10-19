import React from 'react';

interface Props {
  title?: string;
  summary?: string;
  published?: string;
  org?: string;
  type?: string;
  link?: string;
  theme?: string;
  topic?: string;
}

const PrototypeSearchResult = ({
  title,
  summary,
  published,
  org,
  type,
  link,
  theme,
  topic,
}: Props) => {
  return (
    <>
      <h3 className="govuk-heading-m govuk-!-margin-bottom-2">
        <a href={link || '#'}>{title}</a>
      </h3>
      <p>{summary}</p>

      <div className="govuk-grid-row govuk-body-s">
        <div className="govuk-grid-column-one-half">
          Type: {type}
          <br />
          Organisation: {org}
          <br />
          Published: {published}
        </div>
        <div className="govuk-grid-column-one-half">
          Theme: {theme}
          <br />
          Topic: {topic}
        </div>
      </div>
      <hr />
    </>
  );
};

export default PrototypeSearchResult;
