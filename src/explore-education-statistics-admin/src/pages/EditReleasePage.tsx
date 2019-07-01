import React, { useEffect, useState } from 'react';
import Page from '@admin/components/Page';
import SecondLevelNavigationHeadings, {
  NavigationHeader,
} from '@admin/components/SecondLevelNavigationHeadings';
import { Release } from '@admin/services/publicationService';
import DummyPublicationsData from '@admin/pages/DummyPublicationsData';
import EditReleaseSetupSummary from '@admin/components/EditReleaseSetupSummary';
import { RouteComponentProps } from 'react-router';
import Link from '@admin/components/Link';
import PrototypePage from '@admin/pages/prototypes/components/PrototypePage';
import PreviousNextLinks from '@admin/components/PreviousNextLinks';

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

  const nextSection =
    availableSections.indexOf(selectedSection) < availableSections.length - 1
      ? availableSections[availableSections.indexOf(selectedSection) + 1]
      : null;

  const previousSection =
    availableSections.indexOf(selectedSection) > 0
      ? availableSections[availableSections.indexOf(selectedSection) - 1]
      : null;

  return (
    <Page
      wide
      breadcrumbs={[
        {
          link: '/admin-dashboard',
          name: 'Administrator dashboard',
        },
        { name: 'Edit release', link: '#' },
      ]}
    >
      {release && publicationTitle && (
        <>
          <SecondLevelNavigationHeadings
            navigationHeadingText={publicationTitle}
            navigationHeadingSubtitle="Edit release"
            selectedSection={selectedSection.section}
            availableSections={availableSections}
          />
          {ReleaseSection.ReleaseSetup === selectedSection.section && (
            <EditReleaseSetupSummary
              publicationTitle={publicationTitle}
              release={release}
            />
          )}
          <PreviousNextLinks
            previousSection={previousSection || undefined}
            nextSection={nextSection || undefined}
          />
        </>
      )}
    </Page>
  );
};

export default EditReleasePage;
