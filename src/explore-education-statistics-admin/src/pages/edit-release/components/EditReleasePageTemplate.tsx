import React, { ReactNode } from 'react';
import Page from '../../../components/Page';
import NavigableSections, {
  Section,
} from '../../../components/NavigableSections';

export enum ReleaseSection {
  ReleaseSetup,
  AddEditData,
  BuildTables,
  ViewEditTables,
  AddEditContent,
  SetPublishStatus,
}

interface Props {
  releaseId: string;
  children: ReactNode;
  publicationTitle: string;
  selectedSection: ReleaseSection;
}

const navigationHeadings: (
  releaseId: string,
) => Section<ReleaseSection>[] = releaseId => {
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

const EditReleasePageTemplate = ({
  releaseId,
  selectedSection,
  publicationTitle,
  children,
}: Props) => {
  const availableSections = navigationHeadings(releaseId);

  const selectedSectionNav =
    availableSections.find(s => s.section === selectedSection) ||
    availableSections[0];

  return (
    <Page wide breadcrumbs={breadcrumbs}>
      <h1 className="govuk-heading-l">
        {publicationTitle}
        <span className="govuk-caption-l">Edit release</span>
      </h1>

      <NavigableSections
        navigationHeadingText={publicationTitle}
        navigationHeadingSubtitle="Edit release"
        availableSections={availableSections}
        selectedSection={selectedSectionNav}
      >
        {children}
      </NavigableSections>
    </Page>
  );
};

export default EditReleasePageTemplate;
