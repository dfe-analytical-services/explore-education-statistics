import methodologyService, {
  MethodologyVersion,
  MethodologyStatus,
} from '@admin/services/methodologyService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import MethodologyStatusForm from '@admin/pages/methodology/edit-methodology/status/components/MethodologyStatusForm';
import React from 'react';

interface FormValues {
  status: MethodologyStatus;
  latestInternalReleaseNote: string;
  publishingStrategy?: 'WithRelease' | 'Immediately';
  withReleaseId?: string;
}

interface Props {
  methodology: MethodologyVersion;
  onCancel: () => void;
  onSubmit: (values: FormValues) => void;
}

const MethodologyStatusEditPage = ({
  methodology,
  onCancel,
  onSubmit,
}: Props) => {
  const { value: unpublishedReleases, isLoading } = useAsyncHandledRetry(
    async () => methodologyService.getUnpublishedReleases(methodology.id),
    [methodology.id],
  );

  return (
    <LoadingSpinner loading={isLoading}>
      {unpublishedReleases ? (
        <MethodologyStatusForm
          isPublished={methodology.published}
          methodology={methodology}
          unpublishedReleases={unpublishedReleases}
          onCancel={onCancel}
          onSubmit={onSubmit}
        />
      ) : (
        <WarningMessage>Could not load unpublished releases</WarningMessage>
      )}
    </LoadingSpinner>
  );
};

export default MethodologyStatusEditPage;
