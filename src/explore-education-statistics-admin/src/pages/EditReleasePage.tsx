import React, { useEffect, useState } from 'react';
import Page from '@admin/components/Page';
import SecondLevelNavigationHeadings, {
  NavigationHeader,
} from '@admin/components/SecondLevelNavigationHeadings';
import { Release } from '@admin/services/publicationService';
import DummyPublicationsData from '@admin/pages/DummyPublicationsData';
import { RouteComponentProps } from 'react-router';
import PrototypePublicationSummary from './prototypes/components/PrototypePublicationPageSummary';

export enum ReleaseSection {
  ReleaseSetup,
  AddEditData,
  BuildTables,
  ViewEditTables,
  AddEditContent,
  SetPublishStatus,
}

interface Props extends RouteComponentProps {
  releaseId: string;
  initiallySelectedSection?: ReleaseSection;
}

const navigationHeadings: (
  releaseId: string,
) => NavigationHeader<ReleaseSection>[] = releaseId => {
  const urlPrefix = `/edit-release/${releaseId}`;
  return [
    {
      section: ReleaseSection.ReleaseSetup,
      label: 'Release setup',
      linkTo: `${urlPrefix}/setup`,
    },
    {
      section: ReleaseSection.AddEditData,
      label: 'Add / edit data',
      linkTo: `${urlPrefix}/data`,
    },
    {
      section: ReleaseSection.BuildTables,
      label: 'Build tables',
      linkTo: `${urlPrefix}/build-tables`,
    },
    {
      section: ReleaseSection.ViewEditTables,
      label: 'View / edit tables',
      linkTo: `${urlPrefix}/tables`,
    },
    {
      section: ReleaseSection.AddEditContent,
      label: 'Add / edit content',
      linkTo: `${urlPrefix}/content`,
    },
    {
      section: ReleaseSection.SetPublishStatus,
      label: 'Set publish status',
      linkTo: `${urlPrefix}/publish-status`,
    },
  ];
};

const EditReleasePage = ({ releaseId, location }: Props) => {
  const [release, setRelease] = useState<Release>();

  useEffect(() => {
    setRelease(DummyPublicationsData.getReleaseById(releaseId));
  }, [releaseId]);

  const selectedSection = location.pathname.endsWith('/setup')
    ? ReleaseSection.ReleaseSetup
    : ReleaseSection.AddEditData;

  return (
    <Page
      wide
      breadcrumbs={[
        {
          link: '/prototypes/admin-dashboard?status=editNewRelease',
          name: 'Administrator dashboard',
        },
        { name: 'Create new release', link: '#' },
      ]}
    >
      {release && (
        <>
          <SecondLevelNavigationHeadings
            navigationHeadingText={release.releaseName}
            selectedSection={selectedSection}
            availableSections={navigationHeadings(releaseId)}
          />
          {selectedSection === ReleaseSection.ReleaseSetup && (
            <PrototypePublicationSummary />
          )}
          {selectedSection === ReleaseSection.AddEditData && <></>}
        </>
      )}
    </Page>
  );
};

export default EditReleasePage;
