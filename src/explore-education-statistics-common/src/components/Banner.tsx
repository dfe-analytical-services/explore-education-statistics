import styles from '@common/components/Banner.module.scss';
import ButtonText from '@common/components/ButtonText';
import VisuallyHidden from '@common/components/VisuallyHidden';
import classNames from 'classnames';
import React, { ReactNode } from 'react';

interface Props {
  children: ReactNode;
  className?: string;
  label?: string;
  onClose?: () => void;
  testId?: string;
}

export default function Banner({
  children,
  className = 'govuk-width-container',
  label = 'banner',
  onClose,
  testId,
}: Props) {
  return (
    <div
      className={styles.container}
      data-testid={testId}
      aria-label={label}
      role="region"
    >
      <div className={className}>
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-three-quarters">{children}</div>
          {onClose && (
            <div className="govuk-grid-column-one-quarter govuk-!-text-align-right">
              <ButtonText
                className={classNames(styles.close, 'govuk-!-margin-bottom-2')}
                onClick={onClose}
              >
                Close<VisuallyHidden> {label}</VisuallyHidden>
              </ButtonText>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
