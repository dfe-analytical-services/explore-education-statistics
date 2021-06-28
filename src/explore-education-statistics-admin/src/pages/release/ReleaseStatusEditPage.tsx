import ReleaseStatusChecklist from '@admin/pages/release/components/ReleaseStatusChecklist';
import ReleaseStatusForm from '@admin/pages/release/components/ReleaseStatusForm';
import { ReleaseStatusPermissions } from '@admin/services/permissionService';
import releaseService, { Release } from '@admin/services/releaseService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import { formatISO } from 'date-fns';
import React from 'react';

interface Props {
  release: Release;
  statusPermissions: ReleaseStatusPermissions;
  onCancel: () => void;
  onUpdate: (values: Release) => void;
}

const ReleaseStatusEditPage = ({
  release,
  statusPermissions,
  onCancel,
  onUpdate,
}: Props) => {
  const { value: checklist, isLoading } = useAsyncHandledRetry(
    async () => releaseService.getReleaseChecklist(release.id),
    [release.id],
  );

  return (
    <LoadingSpinner loading={isLoading}>
      <h2>Edit release status</h2>

      {checklist && (
        <ReleaseStatusChecklist checklist={checklist} release={release} />
      )}

      <ReleaseStatusForm
        release={release}
        statusPermissions={statusPermissions}
        onCancel={onCancel}
        onSubmit={async values => {
          const nextRelease = await releaseService.createReleaseStatus(
            release.id,
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
