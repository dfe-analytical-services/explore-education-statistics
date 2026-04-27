import TableToolInfo from '@common/modules/table-tool/components/TableToolInfo';
import { SelectedPublication } from '@common/modules/table-tool/types/selectedPublication';
import { PublicationMethodologiesList } from '@common/services/publicationService';
import Link from '@frontend/components/Link';
import React, { ReactNode } from 'react';

interface Props {
  publicationMethodologies: PublicationMethodologiesList;
  selectedPublication: SelectedPublication;
}

export default function TableToolInfoWrapper({
  publicationMethodologies,
  selectedPublication,
}: Props) {
  const getMethodologyLinks = () => {
    const links: ReactNode[] =
      publicationMethodologies.methodologies.map(methodology => (
        <Link
          key={methodology.methodologyId}
          to={`/methodology/${methodology.slug}`}
        >
          {methodology.title}
        </Link>
      )) ?? [];

    if (publicationMethodologies.externalMethodology) {
      links.push(
        <Link
          key={publicationMethodologies.externalMethodology.url}
          to={publicationMethodologies.externalMethodology.url}
        >
          {publicationMethodologies.externalMethodology.title}
        </Link>,
      );
    }
    return links;
  };

  return (
    <TableToolInfo
      contactDetails={selectedPublication.contact}
      methodologyLinks={getMethodologyLinks()}
      publishingOrganisations={
        selectedPublication.selectedRelease.publishingOrganisations
      }
      releaseLink={
        <Link
          to={`/find-statistics/${selectedPublication.slug}/${selectedPublication.latestRelease.slug}`}
        >
          {`${selectedPublication.title}, ${selectedPublication.latestRelease.title}`}
        </Link>
      }
      releaseType={selectedPublication.selectedRelease.type}
    />
  );
}
