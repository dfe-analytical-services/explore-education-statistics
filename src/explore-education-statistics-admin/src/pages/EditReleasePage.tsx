import React, { useEffect, useState } from 'react';
import Page from '@admin/components/Page';
import { Release } from '@admin/services/publicationService';
import DummyPublicationsData from '@admin/pages/DummyPublicationsData';
import EditReleaseSetupSummary from '@admin/components/EditReleaseSetupSummary';
import SectionedPage, {
  NavigationHeader,
} from '@admin/components/SectionedPage';
import { RouteComponentProps } from 'react-router';

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

const breadcrumbs = [
  {
    link: '/admin-dashboard',
    name: 'Administrator dashboard',
  },
  { name: 'Edit release', link: '#' },
];

const EditReleasePage = ({ releaseId, location }: Props) => {
  const [release, setRelease] = useState<Release>();

  const [publicationTitle, setPublicationTitle] = useState<string>();

  useEffect(() => {
    const selectedRelease = DummyPublicationsData.getReleaseById(releaseId);
    const owningPublication = DummyPublicationsData.getOwningPublicationForRelease(
      selectedRelease,
    );
    setRelease(selectedRelease);
    setPublicationTitle(owningPublication ? owningPublication.title : '');
  }, [releaseId]);

  const availableSections = navigationHeadings(releaseId);

  const selectedSection =
    availableSections.find(section =>
      location.pathname.endsWith(section.linkTo),
    ) || availableSections[0];

  return (
    <Page wide breadcrumbs={breadcrumbs}>
      {release && publicationTitle && (
        <>
          <SectionedPage
            navigationHeadingText={publicationTitle}
            navigationHeadingSubtitle="Edit release"
            availableSections={availableSections}
            selectedSection={selectedSection}
          >
            {ReleaseSection.ReleaseSetup === selectedSection.section && (
              <EditReleaseSetupSummary
                publicationTitle={publicationTitle}
                release={release}
              />
            )}
          </SectionedPage>
        </>
      )}
    </Page>
  );
};

export default EditReleasePage;
