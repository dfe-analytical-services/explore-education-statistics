import ReleaseStatusChecklist from '@admin/pages/release/components/ReleaseStatusChecklist';
import ReleaseStatusForm from '@admin/pages/release/components/ReleaseStatusForm';
import { ReleaseStatusPermissions } from '@admin/services/permissionService';
import releaseVersionService, {
  ReleaseVersion,
} from '@admin/services/releaseVersionService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import { formatISO } from 'date-fns';
import React from 'react';

interface Props {
  releaseVersion: ReleaseVersion;
  statusPermissions: ReleaseStatusPermissions;
  onCancel: () => void;
  onUpdate: (values: ReleaseVersion) => void;
}

const ReleaseStatusEditPage = ({
  releaseVersion,
  statusPermissions,
  onCancel,
  onUpdate,
}: Props) => {
  const { value: checklist, isLoading } = useAsyncHandledRetry(
    async () =>
      releaseVersionService.getReleaseVersionChecklist(releaseVersion.id),
    [releaseVersion.id],
  );

  return (
    <LoadingSpinner loading={isLoading}>
      <h2>Edit release status</h2>

      {checklist && (
        <ReleaseStatusChecklist
          checklist={checklist}
          releaseVersion={releaseVersion}
        />
      )}

      <ReleaseStatusForm
        releaseVersion={releaseVersion}
        statusPermissions={statusPermissions}
        onCancel={onCancel}
        onSubmit={async values => {
          const nextRelease =
            await releaseVersionService.createReleaseVersionStatus(
              releaseVersion.id,
              {
                ...values,
                publishScheduled: values.publishScheduled
                  ? formatISO(values.publishScheduled, {
                      representation: 'date',
                    })
                  : undefined,
              },
            );
          onUpdate(nextRelease);
        }}
      />
    </LoadingSpinner>
  );
};

export default ReleaseStatusEditPage;
