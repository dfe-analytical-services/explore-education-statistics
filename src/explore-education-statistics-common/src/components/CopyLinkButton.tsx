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
  const [showModal, toggleModal] = useToggle(false);

  return (
    <>
      <Button
        className={className}
        variant="secondary"
        type="button"
        onClick={toggleModal.on}
      >
        <LinkIcon />
        <VisuallyHidden>Copy link to the clipboard</VisuallyHidden>
      </Button>
      <Modal
        open={showModal}
        title="Copy link to the clipboard"
        onExit={() => {
          toggleModal.off();
          toggleCopied.off();
        }}
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
        <Button
          variant="secondary"
          onClick={() => {
            toggleModal.off();
            toggleCopied.off();
          }}
        >
          Close
        </Button>
      </Modal>
    </>
  );
};

export default CopyLinkButton;
