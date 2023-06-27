import styles from '@common/components/Banner.module.scss';
import ButtonText from '@common/components/ButtonText';
import classNames from 'classnames';
import React, { ReactNode } from 'react';

interface Props {
  children: ReactNode;
  wide?: boolean;
  onClose?: () => void;
  testId?: string;
}

export default function Banner({
  children,
  wide = false,
  onClose,
  testId,
}: Props) {
  return (
    <div className={styles.container} data-testid={testId}>
      <div
        className={classNames('govuk-width-container', {
          'dfe-width-container--wide': wide,
        })}
      >
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-three-quarters">{children}</div>
          {onClose && (
            <div className="govuk-grid-column-one-quarter dfe-align--right">
              <ButtonText
                className={classNames(styles.close, 'govuk-!-margin-bottom-2')}
                onClick={onClose}
              >
                Close
              </ButtonText>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
