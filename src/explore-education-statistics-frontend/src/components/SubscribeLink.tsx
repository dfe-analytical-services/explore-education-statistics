import SubscribeIcon from '@common/components/SubscribeIcon';
import Link from '@frontend/components/Link';
import styles from '@frontend/components/SubscribeLink.module.scss';
import React from 'react';

interface Props {
  text?: string;
  url: string;
}

export default function SubscribeLink({
  text = 'Get email alerts',
  url,
}: Props) {
  return (
    <Link
      className={`${styles.link} govuk-!-display-none-print`}
      unvisited
      to={url}
    >
      <SubscribeIcon className={styles.icon} />
      {text}
    </Link>
  );
}
