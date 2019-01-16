import React, { ReactNode } from 'react';
import styles from './MenuDetails.module.scss';

interface Props {
  children?: ReactNode;
  open?: boolean;
  summary: string;
}

const MenuDetails = ({ children, open, summary }: Props) => {
  return (
    <details className={styles.details} open={open}>
      <summary className="govuk-details__summary">
        <span className="govuk-details__summary-text">{summary}</span>
      </summary>
      <div className={styles.content}>{children}</div>
    </details>
  );
};

export default MenuDetails;
