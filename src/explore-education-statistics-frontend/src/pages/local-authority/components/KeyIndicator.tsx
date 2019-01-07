import React from 'react';
import { ChangeText, ChangeTextProps } from '../../../components/ChangeText';
import Link from '../../../components/Link';
import styles from './KeyIndicator.module.scss';

export interface KeyIndicatorProps {
  changes: ChangeTextProps[];
  link: string;
  reference?: {
    title: string;
    link: string;
  };
  title: string;
  units: string;
  value: number;
}

export const KeyIndicator = ({
  changes = [],
  link,
  reference,
  title,
  units,
  value,
}: KeyIndicatorProps) => (
  <div className={styles.container}>
    <h3>
      <Link to={link}>{title}</Link>
    </h3>
    <h2>{units ? `${value}${units}` : value}</h2>

    <ul className={styles.changeList}>
      {changes.map(change => (
        <li key={`${change.description}:${change.value}`}>
          <ChangeText {...change} />
        </li>
      ))}
    </ul>

    {reference && (
      <Link to={reference.link} className={styles.referenceLink}>
        From: {reference.title}
      </Link>
    )}
  </div>
);
