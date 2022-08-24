import { useReleaseContext } from '@admin/pages/release/contexts/ReleaseContext';
import PreReleaseUserAccessForm from '@admin/pages/release/pre-release/components/PreReleaseUserAccessForm';
import PublicPreReleaseAccessForm from '@admin/pages/release/pre-release/components/PublicPreReleaseAccessForm';
import { preReleaseContentRoute } from '@admin/routes/preReleaseRoutes';
import { ReleaseRouteParams } from '@admin/routes/releaseRoutes';
import releaseService from '@admin/services/releaseService';
import permissionService from '@admin/services/permissionService';
import InsetText from '@common/components/InsetText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import UrlContainer from '@common/components/UrlContainer';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { generatePath } from 'react-router';

export const releasePreReleaseAccessPageTabs = {
  users: 'preReleaseAccess-users',
  publicAccessList: 'preReleaseAccess-publicList',
};

const ReleasePreReleaseAccessPage = () => {
  const { releaseId } = useReleaseContext();

  const {
    value: release,
    isLoading,
    setState: setRelease,
  } = useAsyncHandledRetry(() => releaseService.getRelease(releaseId), [
    releaseId,
  ]);

  const { value: canUpdateRelease = false } = useAsyncHandledRetry(
    () => permissionService.canUpdateRelease(releaseId),
    [releaseId],
  );

  return (
    <LoadingSpinner loading={isLoading}>
      {release && (
        <Tabs id="preReleaseAccess">
          {!release.amendment && (
            <TabsSection
              id={releasePreReleaseAccessPageTabs.users}
              title="Pre-release users"
            >
              <h2>Manage pre-release user access</h2>
              <InsetText>
                <h3>Before you start</h3>
                <p>
                  Pre-release users will receive an email with a link to preview
                  the publication for pre-release as soon as you add them. The
                  preview will show a holding page until 24 hours before the
                  scheduled publication date.
                </p>
                <p>
                  Pre-release access via Explore Education Statistics is
                  currently limited to DFE users only, if you need to share your
                  release with external users you will need to do so outside of
                  the system.
                </p>
              </InsetText>

              {!release.live && (
                <>
                  <p>
                    The <strong>pre-release</strong> will be accessible at:
                  </p>

                  <p>
                    <UrlContainer
                      data-testid="prerelease-url"
                      url={`${window.location.origin}${generatePath<
                        ReleaseRouteParams
                      >(preReleaseContentRoute.path, {
                        publicationId: release.publicationId,
                        releaseId: release.id,
                      })}`}
                    />
                  </p>
                </>
              )}

              <PreReleaseUserAccessForm
                releaseId={release.id}
                isReleaseApproved={release.approvalStatus === 'Approved'}
                isReleaseLive={release.live}
              />
            </TabsSection>
          )}
          <TabsSection
            id={releasePreReleaseAccessPageTabs.publicAccessList}
            title="Public access list"
          >
            <h2>Public pre-release access list</h2>

            <PublicPreReleaseAccessForm
              canUpdateRelease={canUpdateRelease}
              isReleaseLive={release.live}
              preReleaseAccessList={release.preReleaseAccessList}
              onSubmit={async ({ preReleaseAccessList }) => {
                const updatedRelease = await releaseService.updateRelease(
                  release.id,
                  {
                    year: release.year,
                    timePeriodCoverage: release.timePeriodCoverage,
                    type: release.type,
                    preReleaseAccessList,
                  },
                );

                setRelease({
                  value: updatedRelease,
                });
              }}
            />
          </TabsSection>
        </Tabs>
      )}
    </LoadingSpinner>
  );
};

export default ReleasePreReleaseAccessPage;
