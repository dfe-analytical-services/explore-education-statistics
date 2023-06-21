import classNames from 'classnames';
import React from 'react';
import styles from './UrlContainer.module.scss';

interface Props {
  'data-testid'?: string;
  className?: string;
  url: string;
}

const UrlContainer = ({
  'data-testid': dataTestId = 'url',
  className,
  url,
}: Props) => {
  const handleFocus = (event: React.FocusEvent<HTMLInputElement>) =>
    event.target.select();
  return (
    <>
      <label htmlFor={dataTestId} className="govuk-visually-hidden">
        Url
      </label>
      <input
        type="text"
        value={url}
        id={dataTestId}
        className={classNames(styles.url, className)}
        data-testid={dataTestId}
        onFocus={handleFocus}
        readOnly
      />
    </>
  );
};

export default UrlContainer;
