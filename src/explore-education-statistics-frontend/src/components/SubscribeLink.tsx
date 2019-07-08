import React from 'react';
import Link from './Link';
import styles from './SubscribeLink.module.scss';

export interface SubscribeLinkProps {
  slug: string;
}

const SubscribeLink = ({ slug }: SubscribeLinkProps) => {
  return (
    <Link
      className={styles.container}
      unvisited
      to={`/subscriptions?slug=${slug}`}
      data-testid={`subsciption-${slug}`}
    >
      Sign up for email alerts
    </Link>
  );
};

export default SubscribeLink;
