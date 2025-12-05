import PageMetaTitle from '@admin/components/PageMetaTitle';
import ReleaseStatusForm from '@admin/pages/release/components/ReleaseStatusForm';
import ReleaseStatusChecklistSummary from '@admin/pages/release/components/ReleaseStatusChecklistSummary';
import { ReleaseStatusPermissions } from '@admin/services/permissionService';
import releaseVersionService, {
  ReleaseVersion,
} from '@admin/services/releaseVersionService';
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
  return (
    <>
      <PageMetaTitle title="Edit release status" />
      <h2>Edit release status</h2>

      <ReleaseStatusChecklistSummary
        publicationId={releaseVersion.publicationId}
        releaseVersionId={releaseVersion.id}
      />

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
    </>
  );
};

export default ReleaseStatusEditPage;
