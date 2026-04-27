import LoadingSpinner from '@common/components/LoadingSpinner';
import TableToolInfo from '@common/modules/table-tool/components/TableToolInfo';
import { SelectedPublication } from '@common/modules/table-tool/types/selectedPublication';
import { PublicationMethodologiesList } from '@common/services/publicationService';
import Link from '@frontend/components/Link';
import publicationQueries from '@frontend/queries/publicationQueries';
import { useQuery } from '@tanstack/react-query';
import React, { ReactNode } from 'react';

interface Props {
  publicationMethodologies?: PublicationMethodologiesList;
  selectedPublication: SelectedPublication;
}

export default function TableToolInfoWrapper({
  publicationMethodologies,
  selectedPublication,
}: Props) {
  const { data, isLoading } = useQuery({
    ...publicationQueries.getPublicationMethodologies(selectedPublication.slug),
    enabled: !publicationMethodologies,
    staleTime: Infinity,
  });

  if (!data && !publicationMethodologies) return null;

  const methodologyList =
    publicationMethodologies ?? data ?? ({} as PublicationMethodologiesList);

  const getMethodologyLinks = () => {
    const links: ReactNode[] =
      methodologyList.methodologies.map(methodology => (
        <Link
          key={methodology.methodologyId}
          to={`/methodology/${methodology.slug}`}
        >
          {methodology.title}
        </Link>
      )) ?? [];

    if (methodologyList.externalMethodology) {
      links.push(
        <Link
          key={methodologyList.externalMethodology.url}
          to={methodologyList.externalMethodology.url}
        >
          {methodologyList.externalMethodology.title}
        </Link>,
      );
    }
    return links;
  };

  return (
    <LoadingSpinner loading={!publicationMethodologies && isLoading}>
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
    </LoadingSpinner>
  );
}
