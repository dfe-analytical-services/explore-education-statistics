import classNames from 'classnames';
import React, { ReactNode } from 'react';
import styles from './InsetText.module.scss';

interface Props {
  children: ReactNode;
  className?: string;
  id?: string;
  testId?: string;
  variant?: 'success' | 'error' | 'warning';
}

const InsetText = ({ children, className, id, testId, variant }: Props) => {
  const variantStyle = variant && styles[variant];

  return (
    <div
      className={classNames('govuk-inset-text', className, variantStyle)}
      data-testid={testId}
      id={id}
    >
      {children}
    </div>
  );
};

export default InsetText;
