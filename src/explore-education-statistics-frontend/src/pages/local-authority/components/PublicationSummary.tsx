import React from 'react';
import ReactMarkdown from 'react-markdown';
import KeyIndicatorTile, {
  KeyIndicatorTileProps,
} from '../../../components/KeyIndicatorTile';
import Link from '../../../components/Link';
import styles from './PublicationSummary.module.scss';

export interface PublicationSummaryProps {
  keyIndicator: KeyIndicatorTileProps;
  link: string;
  summary: string;
  title: string;
}

const PublicationSummary = ({
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
          <KeyIndicatorTile {...keyIndicator} />
        </div>
        <div className="govuk-grid-column-one-half">
          <ReactMarkdown className="govuk-body-s" source={summary} />
        </div>
      </div>

      <hr />

      <ul className="govuk-list">
        <li>
          <Link to="#">View report</Link>
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

export default PublicationSummary;
