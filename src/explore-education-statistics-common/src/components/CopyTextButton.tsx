import Button from '@common/components/Button';
import styles from '@common/components/CopyTextButton.module.scss';
import ScreenReaderMessage from '@common/components/ScreenReaderMessage';
import UrlContainer from '@common/components/UrlContainer';
import useToggle from '@common/hooks/useToggle';
import classNames from 'classnames';
import React, { ReactNode, useEffect } from 'react';

export interface CopyTextButtonProps {
  buttonText?: string | ReactNode;
  className?: string;
  confirmText?: string;
  id: string;
  inline?: boolean;
  label: string;
  labelHidden?: boolean;
  text: string;
}

export default function CopyTextButton({
  buttonText = 'Copy',
  className,
  confirmText = 'Copied',
  id,
  inline = true,
  label,
  labelHidden,
  text,
}: CopyTextButtonProps) {
  const [copied, toggleCopied] = useToggle(false);

  useEffect(() => {
    const resetTimeout = setTimeout(toggleCopied.off, 5000);

    return () => {
      if (copied) {
        clearTimeout(resetTimeout);
      }
    };
  }, [copied, toggleCopied]);

  return (
    <div
      className={classNames(className, styles.container, {
        'dfe-flex dfe-align-items--start': inline,
      })}
    >
      <UrlContainer
        className={classNames(styles.urlContainer, {
          'dfe-flex-grow--1': inline,
          'govuk-!-margin-bottom-2': !inline,
        })}
        id={id}
        inline={inline}
        label={label}
        labelHidden={labelHidden}
        url={text}
      />

      <Button
        className={classNames(styles.button, {
          'govuk-!-margin-right-2': !inline,
        })}
        onClick={async () => {
          await navigator.clipboard.writeText(text);
          toggleCopied.on();
        }}
      >
        {copied ? confirmText : buttonText}
      </Button>

      <ScreenReaderMessage message={copied ? confirmText : ''} />
    </div>
  );
}
