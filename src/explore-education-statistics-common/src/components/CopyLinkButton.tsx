import Button from '@common/components/Button';
import LinkIcon from '@common/components/LinkIcon';
import styles from '@common/components/CopyLinkButton.module.scss';
import Modal from '@common/components/Modal';
import UrlContainer from '@common/components/UrlContainer';
import VisuallyHidden from '@common/components/VisuallyHidden';
import useToggle from '@common/hooks/useToggle';
import classNames from 'classnames';
import React from 'react';

interface Props {
  className?: string;
  url: string;
}

const CopyLinkButton = ({ className, url }: Props) => {
  const [copied, toggleCopied] = useToggle(false);

  return (
    <Modal
      showClose
      title="Copy link to the clipboard"
      triggerButton={
        <Button className={className} variant="secondary">
          <LinkIcon />
          <VisuallyHidden>Copy link to the clipboard</VisuallyHidden>
        </Button>
      }
    >
      <div
        className={classNames('dfe-flex dfe-align-items-start', styles.modal)}
      >
        <UrlContainer url={url} />
        <Button
          className="govuk-!-margin-bottom-0"
          onClick={async () => {
            await navigator.clipboard.writeText(url);
            toggleCopied.on();
          }}
        >
          Copy
        </Button>
      </div>
      <div aria-live="polite" className={styles.message}>
        {copied && 'Link copied to the clipboard.'}
      </div>
    </Modal>
  );
};

export default CopyLinkButton;
