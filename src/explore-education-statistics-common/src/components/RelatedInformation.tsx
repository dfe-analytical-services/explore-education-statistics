import React, { ReactNode } from 'react';
import styles from './RelatedInformation.module.scss';

interface Props {
  children: ReactNode;
  heading?: string;
  id?: string;
}

const RelatedInformation = ({
  children,
  heading = 'Related information',
  id = 'related-information',
}: Props) => {
  return (
    <aside className={styles.container}>
      <nav role="navigation" aria-labelledby={id}>
        <h2 className="govuk-heading-m" id={id}>
          {heading}
        </h2>
        {children}
      </nav>
    </aside>
  );
};

export default RelatedInformation;
