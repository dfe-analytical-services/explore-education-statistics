import React, { ReactNode } from 'react';
import styles from './KeyStatTile.module.scss';

interface Props {
  children?: ReactNode;
  testId?: string;
  title: string;
  value: string;
}

const KeyStatTile = ({
  children,
  testId = 'keyStatTile',
  title,
  value,
}: Props) => {
  return (
    <div className={styles.tile}>
      <h3 className="govuk-heading-s" data-testid={`${testId}-title`}>
        {title}
      </h3>

      <p className="govuk-heading-xl" data-testid={`${testId}-value`}>
        {value}
      </p>

      {children}
    </div>
  );
};

export default KeyStatTile;
