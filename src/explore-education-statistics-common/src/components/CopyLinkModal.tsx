import Button from '@common/components/Button';
import LinkIcon from '@common/components/LinkIcon';
import Modal from '@common/components/Modal';
import VisuallyHidden from '@common/components/VisuallyHidden';
import CopyTextButton from '@common/components/CopyTextButton';
import React from 'react';
import styles from './CopyLinkModal.module.scss';

interface Props {
  buttonClassName?: string;
  url: string;
}

export default function CopyLinkModal({ buttonClassName, url }: Props) {
  return (
    <Modal
      showClose
      title="Copy link to the clipboard"
      triggerButton={
        <Button className={buttonClassName} variant="secondary">
          <LinkIcon />
          <VisuallyHidden>Copy link to the clipboard</VisuallyHidden>
        </Button>
      }
    >
      <CopyTextButton
        buttonText="Copy link"
        className={styles.copyLink}
        confirmText="Link copied"
        id="copy-link-url"
        label="URL"
        labelHidden={false}
        text={url}
      />
    </Modal>
  );
}
