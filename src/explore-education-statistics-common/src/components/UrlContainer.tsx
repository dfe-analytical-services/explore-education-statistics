import styles from '@common/components/UrlContainer.module.scss';
import classNames from 'classnames';
import React, { ReactNode } from 'react';

interface Props {
  className?: string;
  id: string;
  label?: string | ReactNode;
  labelHidden?: boolean;
  widthLimited?: boolean;
  testId?: string;
  url: string;
}

export default function UrlContainer({
  className,
  id,
  label = 'URL',
  labelHidden,
  widthLimited,
  testId = id,
  url,
}: Props) {
  return (
    <div
      className={classNames(className, {
        'dfe-flex dfe-align-items--center': !labelHidden,
        'dfe-flex-grow--1': !widthLimited,
      })}
    >
      <label
        htmlFor={id}
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
        id={id}
        className={styles.url}
        data-testid={testId}
        onFocus={e => e.target.select()}
        readOnly
      />
    </div>
  );
}
