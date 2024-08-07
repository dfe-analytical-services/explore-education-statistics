import Button from '@common/components/Button';
import styles from '@common/components/CopyTextButton.module.scss';
import UrlContainer from '@common/components/UrlContainer';
import useToggle from '@common/hooks/useToggle';
import classNames from 'classnames';
import React, { ReactNode } from 'react';

export interface CopyTextButtonProps {
  buttonText?: string | ReactNode;
  className?: string;
  confirmMessage?: string;
  inlineButton?: boolean;
  label?: string;
  text: string;
}

export default function CopyTextButton({
  buttonText = 'Copy',
  className,
  confirmMessage = 'Text copied to the clipboard.',
  inlineButton = true,
  label,
  text,
}: CopyTextButtonProps) {
  const [copied, toggleCopied] = useToggle(false);

  return (
    <div className={className}>
      <div
        className={classNames(styles.container, {
          'dfe-flex dfe-align-items-start': inlineButton,
        })}
      >
        <UrlContainer id="copy-link-url" label={label} url={text} />
        <div
          className={classNames({
            'dfe-flex dfe-align-items--center govuk-!-margin-top-2':
              !inlineButton,
          })}
        >
          <Button
            className={classNames('govuk-!-margin-bottom-0', {
              'govuk-!-margin-right-2': !inlineButton,
            })}
            onClick={async () => {
              await navigator.clipboard.writeText(text);
              toggleCopied.on();
            }}
          >
            {buttonText}
          </Button>
          {!inlineButton && (
            <Message copied={copied} confirmMessage={confirmMessage} />
          )}
        </div>
      </div>
      {inlineButton && (
        <Message
          className={styles.message}
          copied={copied}
          confirmMessage={confirmMessage}
        />
      )}
    </div>
  );
}

function Message({
  className,
  copied,
  confirmMessage,
}: {
  className?: string;
  copied: boolean;
  confirmMessage?: string;
}) {
  return (
    <div aria-live="polite" className={className}>
      {copied && confirmMessage}
    </div>
  );
}
