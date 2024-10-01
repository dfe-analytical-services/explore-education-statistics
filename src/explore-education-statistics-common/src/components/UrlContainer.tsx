import styles from '@common/components/UrlContainer.module.scss';
import classNames from 'classnames';
import React, { ReactNode } from 'react';

interface Props {
  className?: string;
  id: string;
  inline?: boolean;
  label?: string | ReactNode;
  labelHidden?: boolean;
  testId?: string;
  url: string;
}

export default function UrlContainer({
  className,
  id,
  inline = true,
  label = 'URL',
  labelHidden,
  testId = id,
  url,
}: Props) {
  return (
    <div
      className={classNames(className, 'dfe-flex dfe-flex-wrap', {
        'dfe-flex-direction--column': !inline,
        'dfe-align-items--center': inline,
      })}
    >
      <label
        htmlFor={id}
        className={classNames({
          'govuk-visually-hidden': labelHidden,
          'govuk-!-margin-right-2': !labelHidden,
          'govuk-!-display-block govuk-!-margin-bottom-1': !inline,
        })}
      >
        {label}
      </label>
      <input
        className={styles.url}
        data-testid={testId}
        id={id}
        readOnly
        type="text"
        value={url}
        onFocus={e => e.target.select()}
      />
    </div>
  );
}
