import SubscribeIcon from '@common/components/SubscribeIcon';
import Link from '@frontend/components/Link';
import styles from '@frontend/components/SubscribeLink.module.scss';
import React from 'react';

interface Props {
  text?: string;
  url: string;
  onClick?: React.MouseEventHandler<HTMLAnchorElement>;
}

export default function SubscribeLink({
  text = 'Get email alerts',
  url,
  onClick,
}: Props) {
  return (
    <Link
      className={`${styles.link} govuk-!-display-none-print`}
      unvisited
      to={url}
      onClick={onClick}
    >
      <SubscribeIcon className={styles.icon} />
      {text}
    </Link>
  );
}
