import methodologyService, {
  BasicMethodology,
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
  methodologySummary: BasicMethodology;
  onCancel: () => void;
  onSubmit: (values: FormValues) => void;
}

const MethodologyStatusEditPage = ({
  methodologySummary,
  onCancel,
  onSubmit,
}: Props) => {
  const {
    value: unpublishedReleases,
    isLoading,
  } = useAsyncHandledRetry(
    async () =>
      methodologyService.getUnpublishedReleases(methodologySummary.id),
    [methodologySummary.id],
  );

  return (
    <LoadingSpinner loading={isLoading}>
      {unpublishedReleases ? (
        <MethodologyStatusForm
          isPublished={methodologySummary.published}
          methodologySummary={methodologySummary}
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
