import { useReleaseVersionContext } from '@admin/pages/release/contexts/ReleaseVersionContext';
import PreReleaseUserAccessForm from '@admin/pages/release/pre-release/components/PreReleaseUserAccessForm';
import PublicPreReleaseAccessForm from '@admin/pages/release/pre-release/components/PublicPreReleaseAccessForm';
import { preReleaseContentRoute } from '@admin/routes/preReleaseRoutes';
import { ReleaseRouteParams } from '@admin/routes/releaseRoutes';
import releaseVersionService from '@admin/services/releaseVersionService';
import permissionService from '@admin/services/permissionService';
import InsetText from '@common/components/InsetText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import UrlContainer from '@common/components/UrlContainer';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React, { useState } from 'react';
import { generatePath, useLocation } from 'react-router';
import { title } from 'process';
import PageMetaTitle from '@admin/components/PageMetaTitle';

export const releasePreReleaseAccessPageTabs = {
  users: { id: 'preReleaseAccess-users', title: 'Pre-release users' },
  publicAccessList: {
    id: 'preReleaseAccess-publicList',
    title: 'Public access list',
  },
};

const ReleasePreReleaseAccessPage = () => {
  const { releaseVersionId, releaseVersion } = useReleaseVersionContext();
  const { hash } = useLocation();

  const initialTabTitle = hash
    ? Object.values(releasePreReleaseAccessPageTabs).find(
        tab => tab.id === hash.replace('#', ''),
      )?.title
    : releasePreReleaseAccessPageTabs.users.title;
  const [tabTitle, setTabTitle] = useState<string | undefined>(initialTabTitle);

  const {
    value: release,
    isLoading,
    setState: setRelease,
  } = useAsyncHandledRetry(
    () => releaseVersionService.getReleaseVersion(releaseVersionId),
    [releaseVersionId],
  );

  const { value: canUpdateRelease = false } = useAsyncHandledRetry(
    () => permissionService.canUpdateRelease(releaseVersionId),
    [releaseVersionId],
  );

  return (
    <>
      <PageMetaTitle
        title={`${tabTitle} - ${releaseVersion.publicationTitle}`}
      />
      <LoadingSpinner loading={isLoading}>
        {release && (
          <Tabs
            id="preReleaseAccess"
            onToggle={section => {
              setTabTitle(section.title);
            }}
          >
            <TabsSection
              id={releasePreReleaseAccessPageTabs.users.id}
              title={releasePreReleaseAccessPageTabs.users.title}
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
                  Pre-release access via Explore Education Statistics is limited
                  to DfE users by default, access for external users can be
                  requested by emailing{' '}
                  <a href="mailto:explore.statistics@education.gov.uk">
                    explore.statistics@education.gov.uk
                  </a>
                  . If requesting external access, please include the email
                  address of the external user, and contact us at least two
                  weeks in advance as the request requires approval and action
                  from cyber security.
                </p>
              </InsetText>

              {!release.live && (
                <>
                  <p>
                    The <strong>pre-release</strong> will be accessible at:
                  </p>

                  <UrlContainer
                    className="govuk-!-margin-bottom-4"
                    id="prerelease-url"
                    url={`${
                      window.location.origin
                    }${generatePath<ReleaseRouteParams>(
                      preReleaseContentRoute.path,
                      {
                        publicationId: release.publicationId,
                        releaseVersionId: release.id,
                      },
                    )}`}
                  />
                </>
              )}

              <PreReleaseUserAccessForm
                releaseVersionId={release.id}
                isReleaseApproved={release.approvalStatus === 'Approved'}
                isReleaseLive={release.live}
              />
            </TabsSection>
            <TabsSection
              id={releasePreReleaseAccessPageTabs.publicAccessList.id}
              title={releasePreReleaseAccessPageTabs.publicAccessList.title}
            >
              <h2>Public pre-release access list</h2>

              <PublicPreReleaseAccessForm
                canUpdateRelease={canUpdateRelease}
                isReleaseLive={release.live}
                preReleaseAccessList={release.preReleaseAccessList}
                onSubmit={async ({ preReleaseAccessList }) => {
                  const updatedRelease =
                    await releaseVersionService.updateReleaseVersion(
                      release.id,
                      {
                        year: release.year,
                        timePeriodCoverage: release.timePeriodCoverage,
                        type: release.type,
                        preReleaseAccessList,
                        label: release.label,
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
    </>
  );
};

export default ReleasePreReleaseAccessPage;
