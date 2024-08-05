import Button from '@common/components/Button';
import LinkIcon from '@common/components/LinkIcon';
import Modal from '@common/components/Modal';
import VisuallyHidden from '@common/components/VisuallyHidden';
import CopyTextButton, {
  CopyTextButtonProps,
} from '@common/components/CopyTextButton';
import { OmitStrict } from '@common/types';
import React from 'react';

interface Props extends OmitStrict<CopyTextButtonProps, 'text'> {
  buttonClassName?: string;
  url: string;
}

export default function CopyLinkModal({
  buttonClassName,
  className,
  confirmMessage = 'Link copied to the clipboard.',
  inlineButton = true,
  url,
}: Props) {
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
        className={className}
        confirmMessage={confirmMessage}
        inlineButton={inlineButton}
        text={url}
      />
    </Modal>
  );
}
