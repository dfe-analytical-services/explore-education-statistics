import React from 'react';
import Link from '../../../components/Link';
import styles from './KeyIndicator.module.scss';

interface Change {
  description: string;
  units: string;
  value: number;
}

interface Props {
  changes: Change[];
  link: string;
  reference: string;
  referenceLink: string;
  title: string;
  units: string;
  value: number;
}

const getChangeText = ({ description, units, value }: Change) => {
  let diff = 'same as';
  let marker = '\u2BC8';

  if (value > 0) {
    diff = 'higher than';
    marker = '\u2BC5';
  } else if (value < 0) {
    diff = 'lower than';
    marker = '\u2BC6';
  }

  const signedValue = value > 0 ? `+${value}` : value;

  return `${marker} ${
    units ? signedValue + units : signedValue
  } ${diff} ${description}`;
};

const KeyIndicator = ({
  changes = [],
  link,
  reference,
  referenceLink,
  title,
  units,
  value,
}: Props) => (
  <div className={styles.container}>
    <h3>
      <Link to={link}>{title}</Link>
    </h3>
    <h2>{units ? `${value}${units}` : value}</h2>

    <ul className={styles.changeList}>
      {changes.map(change => (
        <li key={`${change.description}:${change.value}`}>
          {getChangeText(change)}
        </li>
      ))}
    </ul>

    <Link to={referenceLink} className={styles.referenceLink}>
      From: {reference}
    </Link>
  </div>
);

export default KeyIndicator;
