import ModalConfirm from '@common/components/ModalConfirm';
import React from 'react';
import { useConfig } from '@admin/contexts/ConfigContext';
import UrlContainer from '@common/components/UrlContainer';
import slugFromTitle from '@common/utils/slugFromTitle';

interface Props {
  initialPublicationTitle: string;
  initialPublicationSlug: string;
  newPublicationTitle: string;
  onConfirm: () => void;
  onCancel: () => void;
  onExit: () => void;
}

export default function PublicationUpdateConfirmModal({
  initialPublicationTitle,
  initialPublicationSlug,
  newPublicationTitle,
  onConfirm,
  onCancel,
  onExit,
}: Props) {
  const newPublicationSlug = slugFromTitle(newPublicationTitle);

  const titleHasChanged = initialPublicationTitle !== newPublicationTitle;
  const slugHasChanged = initialPublicationSlug !== newPublicationSlug;

  const { publicAppUrl } = useConfig();

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
          The publication title will change from{' '}
          <strong>{initialPublicationTitle}</strong> to{' '}
          <strong>{newPublicationTitle}</strong>
        </p>
      )}
      {slugHasChanged && (
        <>
          <p>The URL for this publication will change from</p>
          <UrlContainer
            id="before-url"
            label="Before URL"
            url={`${publicAppUrl}/find-statistics/${initialPublicationSlug}`}
          />{' '}
          to{' '}
          <UrlContainer
            id="after-url"
            label="After URL"
            url={`${publicAppUrl}/find-statistics/${newPublicationSlug}`}
          />
        </>
      )}

      <p className="govuk-!-margin-top-4">
        Are you sure you want to save the changes?
      </p>
    </ModalConfirm>
  );
}
