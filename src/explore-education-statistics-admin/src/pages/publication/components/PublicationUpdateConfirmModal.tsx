import ModalConfirm from '@common/components/ModalConfirm';
import React from 'react';
import { useConfig } from '@admin/contexts/ConfigContext';
import UrlContainer from '@common/components/UrlContainer';
import slugFromTitle from '@common/utils/slugFromTitle';

interface Props {
  title: string;
  slug: string;
  newTitle: string;
  onConfirm: () => void;
  onCancel: () => void;
  onExit: () => void;
}

const PublicationUpdateConfirmModal = ({
  title,
  slug,
  newTitle,
  onConfirm,
  onCancel,
  onExit,
}: Props) => {
  const newSlug = slugFromTitle(newTitle);

  const titleHasChanged = title !== newTitle;
  const slugHasChanged = slug !== newSlug;

  const { PublicAppUrl } = useConfig();

  return (
    <ModalConfirm
      title="Confirm publication changes"
      open
      onConfirm={onConfirm}
      onExit={onExit}
      onCancel={onCancel}
    >
      <p>Any changes will appear on the public site immediately.</p>
      {titleHasChanged && (
        <p>
          The publication title will change from <strong>{title}</strong> to{' '}
          <strong>{newTitle}</strong>
        </p>
      )}
      {slugHasChanged && (
        <>
          <p>The URL for this publication will change from</p>
          <UrlContainer
            data-testid="before-url"
            url={`${PublicAppUrl}/find-statistics/${slug}`}
          />{' '}
          to{' '}
          <UrlContainer
            data-testid="after-url"
            url={`${PublicAppUrl}/find-statistics/${newSlug}`}
          />
        </>
      )}

      <p>Are you sure you want to save the changes?</p>
    </ModalConfirm>
  );
};

export default PublicationUpdateConfirmModal;
