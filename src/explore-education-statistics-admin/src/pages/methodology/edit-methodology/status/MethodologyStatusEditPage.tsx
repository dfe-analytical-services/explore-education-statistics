import methodologyService, {
  MethodologyVersion,
} from '@admin/services/methodologyService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import MethodologyStatusForm, {
  MethodologyStatusFormValues,
} from '@admin/pages/methodology/edit-methodology/status/components/MethodologyStatusForm';
import React from 'react';
import { MethodologyStatusPermissions } from '@admin/services/permissionService';

interface Props {
  methodology: MethodologyVersion;
  statusPermissions?: MethodologyStatusPermissions;
  onCancel: () => void;
  onSubmit: (values: MethodologyStatusFormValues) => void;
}

const MethodologyStatusEditPage = ({
  methodology,
  statusPermissions,
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
          statusPermissions={statusPermissions}
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
