import styles from '@common/components/UrlContainer.module.scss';
import classNames from 'classnames';
import React, { ReactNode } from 'react';

interface Props {
  className?: string;
  label?: string | ReactNode;
  labelHidden?: boolean;
  testId?: string;
  url: string;
}

const UrlContainer = ({
  className,
  label = 'Url',
  labelHidden = true,
  testId = 'url',
  url,
}: Props) => {
  const handleFocus = (event: React.FocusEvent<HTMLInputElement>) =>
    event.target.select();
  return (
    <div
      className={classNames(className, {
        'dfe-flex dfe-align-items--center': !labelHidden,
      })}
    >
      <label
        htmlFor={testId}
        className={classNames({
          'govuk-visually-hidden': labelHidden,
          'govuk-!-margin-right-2': !labelHidden,
        })}
      >
        {label}
      </label>
      <input
        type="text"
        value={url}
        id={testId}
        className={styles.url}
        data-testid={testId}
        onFocus={handleFocus}
        readOnly
      />
    </div>
  );
};

export default UrlContainer;
