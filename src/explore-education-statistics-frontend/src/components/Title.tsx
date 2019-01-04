import React from 'react';
import { H1 } from './Heading';

interface Props {
  label: string;
}

const Title = ({ label }: Props) => (
  <div className="app-content__header">
    <span className="govuk-caption-xl">{label}</span>
    <H1>Find {(label || '').toLowerCase()} statistics</H1>
  </div>
);

export default Title;
