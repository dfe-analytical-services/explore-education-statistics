import Link from '@admin/components/Link';
import PreReleaseUserAccessForm from '@admin/pages/release/components/PreReleaseUserAccessForm';
import PublicPreReleaseAccessForm from '@admin/pages/release/components/PublicPreReleaseAccessForm';
import { useManageReleaseContext } from '@admin/pages/release/contexts/ManageReleaseContext';
import {
  ReleaseRouteParams,
  releaseStatusRoute,
} from '@admin/routes/releaseRoutes';
import { preReleaseRoute } from '@admin/routes/routes';
import releaseService from '@admin/services/releaseService';
import InsetText from '@common/components/InsetText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import UrlContainer from '@common/components/UrlContainer';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { generatePath } from 'react-router';

export const releasePreReleaseAccessPageTabs = {
  users: 'preReleaseAccess-users',
  publicAccessList: 'preReleaseAccess-publicList',
};

const ReleasePreReleaseAccessPage = () => {
  const { releaseId } = useManageReleaseContext();

  const {
    value: release,
    isLoading,
    setState: setRelease,
  } = useAsyncHandledRetry(() => releaseService.getRelease(releaseId), [
    releaseId,
  ]);

  return (
    <LoadingSpinner loading={isLoading}>
      {release && (
        <Tabs id="preReleaseAccess">
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
                Pre-release access via Explore Education Statistics is currently
                limited to DFE users only, if you need to share your release
                with external users you will need to do so outside of the
                system.
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
                    >(preReleaseRoute.path, {
                      publicationId: release.publicationId,
                      releaseId: release.id,
                    })}`}
                  />
                </p>
              </>
            )}

            {release.status === 'Approved' ? (
              <PreReleaseUserAccessForm
                releaseId={release.id}
                isReleaseLive={release.live}
              />
            ) : (
              <WarningMessage>
                Before you can invite users for pre-release access, the release
                status must be approved. This can be done in the{' '}
                <Link
                  to={generatePath<ReleaseRouteParams>(
                    releaseStatusRoute.path,
                    {
                      releaseId,
                      publicationId: release.publicationId,
                    },
                  )}
                >
                  Sign off
                </Link>{' '}
                page.
              </WarningMessage>
            )}
          </TabsSection>
          <TabsSection
            id={releasePreReleaseAccessPageTabs.publicAccessList}
            title="Public access list"
          >
            <h2>Public pre-release access list</h2>

            <PublicPreReleaseAccessForm
              isReleaseLive={release.live}
              preReleaseAccessList={release.preReleaseAccessList}
              onSubmit={async ({ preReleaseAccessList }) => {
                const updatedRelease = await releaseService.updateRelease(
                  release.id,
                  {
                    ...release,
                    typeId: release.type.id,
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
