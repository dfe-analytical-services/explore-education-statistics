import CancelAmendmentModal from '@admin/pages/admin-dashboard/components/CancelAmendmentModal';
import DraftReleaseRow from '@admin/pages/admin-dashboard/components/DraftReleaseRow';
import styles from '@admin/pages/admin-dashboard/components/DraftReleasesTable.module.scss';
import {
  DraftStatusGuidanceModal,
  IssuesGuidanceModal,
} from '@admin/pages/publication/components/PublicationGuidance';
import releaseService, {
  DeleteReleasePlan,
  Release,
} from '@admin/services/releaseService';
import ButtonText from '@common/components/ButtonText';
import InfoIcon from '@common/components/InfoIcon';
import useToggle from '@common/hooks/useToggle';
import { Dictionary } from '@common/types';
import orderBy from 'lodash/orderBy';
import React, { useMemo, useState } from 'react';

interface PublicationRowProps {
  isBauUser: boolean;
  publication: string;
  releases: Release[];
  onDelete: (releaseId: string) => void;
}
const PublicationRow = ({
  isBauUser,
  publication,
  releases,
  onDelete,
}: PublicationRowProps) => {
  return (
    <>
      <tr key={publication}>
        <th className="govuk-!-padding-top-6" colSpan={isBauUser ? 3 : 4}>
          {publication}
        </th>
      </tr>
      {releases.map(release => (
        <DraftReleaseRow
          key={release.id}
          isBauUser={isBauUser}
          release={release}
          onDelete={() => onDelete(release.id)}
        />
      ))}
    </>
  );
};

interface DraftReleasesTableProps {
  isBauUser: boolean;
  releases: Release[];
  onChangeRelease: () => void;
}

const DraftReleasesTable = ({
  isBauUser,
  releases,
  onChangeRelease,
}: DraftReleasesTableProps) => {
  const [deleteReleasePlan, setDeleteReleasePlan] = useState<
    DeleteReleasePlan & {
      releaseId: string;
    }
  >();

  const [showDraftStatusGuidance, toggleDraftStatusGuidance] = useToggle(false);
  const [showIssuesGuidance, toggleIssuesGuidance] = useToggle(false);

  const releasesByPublication: Dictionary<Release[]> = useMemo(() => {
    return releases.reduce<Dictionary<Release[]>>((acc, release) => {
      if (acc[release.publicationTitle]) {
        acc[release.publicationTitle].push(release);
      } else {
        acc[release.publicationTitle] = [release];
      }

      return acc;
    }, {});
  }, [releases]);

  return (
    <>
      {releases.length === 0 ? (
        <p>There are currently no draft releases</p>
      ) : (
        <>
          {releasesByPublication &&
            Object.keys(releasesByPublication).length > 0 && (
              <table>
                <thead>
                  <tr>
                    <th
                      className={
                        isBauUser ? 'govuk-!-width-one-half' : undefined
                      }
                    >
                      Publication / Release period
                    </th>
                    <th>
                      Status{' '}
                      <ButtonText onClick={toggleDraftStatusGuidance.on}>
                        <InfoIcon description="Guidance on draft states" />
                      </ButtonText>
                    </th>
                    {/* Don't render the issues for BAU users to prevent performance problems. */}
                    {!isBauUser && (
                      <th className={styles.issuesColumn}>
                        Issues{' '}
                        <ButtonText onClick={toggleIssuesGuidance.on}>
                          <InfoIcon description="Guidance on draft release issues" />
                        </ButtonText>
                      </th>
                    )}
                    <th>Actions</th>
                  </tr>
                  {orderBy(Object.keys(releasesByPublication)).map(
                    publication => (
                      <PublicationRow
                        key={publication}
                        isBauUser={isBauUser}
                        publication={publication}
                        releases={releasesByPublication[publication]}
                        onDelete={async releaseId => {
                          setDeleteReleasePlan({
                            ...(await releaseService.getDeleteReleasePlan(
                              releaseId,
                            )),
                            releaseId,
                          });
                        }}
                      />
                    ),
                  )}
                </thead>
              </table>
            )}
        </>
      )}

      {deleteReleasePlan && (
        <CancelAmendmentModal
          scheduledMethodologies={deleteReleasePlan.scheduledMethodologies}
          onConfirm={async () => {
            await releaseService.deleteRelease(deleteReleasePlan.releaseId);
            setDeleteReleasePlan(undefined);
            onChangeRelease();
          }}
          onCancel={() => setDeleteReleasePlan(undefined)}
        />
      )}

      <DraftStatusGuidanceModal
        open={showDraftStatusGuidance}
        onClose={toggleDraftStatusGuidance.off}
      />
      <IssuesGuidanceModal
        open={showIssuesGuidance}
        onClose={toggleIssuesGuidance.off}
      />
    </>
  );
};

export default DraftReleasesTable;
