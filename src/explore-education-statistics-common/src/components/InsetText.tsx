import classNames from 'classnames';
import React, { ReactNode } from 'react';
import styles from './InsetText.module.scss';

interface Props {
  children: ReactNode;
  className?: string;
  testId?: string;
  variant?: 'success' | 'error' | 'warning';
}

const InsetText = ({ children, className, testId, variant }: Props) => {
  const variantStyle = variant && styles[variant];

  return (
    <div
      className={classNames('govuk-inset-text', className, variantStyle)}
      data-testid={testId}
    >
      {children}
    </div>
  );
};

export default InsetText;
