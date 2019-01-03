import React from 'react';

interface Props {
  label: string;
}

const Title = ({ label }: Props) => (
  <div className="app-content__header">
    <span className="govuk-caption-xl">{label}</span>
    <h1 className="govuk-heading-xl">
      Find {(label || '').toLowerCase()} statistics
    </h1>
  </div>
);

export default Title;
