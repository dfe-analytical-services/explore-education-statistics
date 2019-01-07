import React from 'react';
import ReactMarkdown from 'react-markdown';
import Link from '../../../components/Link';
import { KeyIndicator, KeyIndicatorProps } from './KeyIndicator';
import styles from './PublicationSummary.module.scss';

export interface PublicationSummaryProps {
  keyIndicator: KeyIndicatorProps;
  link: string;
  summary: string;
  title: string;
}

export const PublicationSummary = ({
  keyIndicator,
  link,
  summary,
  title,
}: PublicationSummaryProps) => {
  return (
    <div className={styles.container}>
      <h3>
        <Link to={link}>{title}</Link>
      </h3>

      <div className="govuk-grid-row">
        <div className="govuk-grid-column-one-half">
          <KeyIndicator {...keyIndicator} />
        </div>
        <div className="govuk-grid-column-one-half">
          <ReactMarkdown className="govuk-body-s" source={summary} />
        </div>
      </div>

      <hr />

      <ul>
        <li>
          <Link to="#" >View report</Link>
        </li>
        <li>
          <Link to="#">Download data</Link>
        </li>
        <li>
          <Link to="#">Visualise data</Link>
        </li>
        <li>
          <Link to="#">Create your own dataset</Link>
        </li>
      </ul>
      </div>
  );
};
