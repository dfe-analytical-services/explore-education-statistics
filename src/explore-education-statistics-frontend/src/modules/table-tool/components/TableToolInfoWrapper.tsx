import LoadingSpinner from '@common/components/LoadingSpinner';
import TableToolInfo from '@common/modules/table-tool/components/TableToolInfo';
import { SelectedPublication } from '@common/modules/table-tool/types/selectedPublication';
import { ReleaseVersion } from '@common/services/publicationService';
import Link from '@frontend/components/Link';
import publicationQueries from '@frontend/queries/publicationQueries';
import { useQuery } from '@tanstack/react-query';
import React, { ReactNode } from 'react';

interface Props {
  fullPublication?: ReleaseVersion;
  selectedPublication: SelectedPublication;
}

export default function TableToolInfoWrapper({
  fullPublication,
  selectedPublication,
}: Props) {
  const { data, isLoading } = useQuery({
    ...publicationQueries.getLatestPublicationRelease(selectedPublication.slug),
    enabled: !fullPublication,
    staleTime: Infinity,
  });

  if (!data && !fullPublication) return null;

  const { publication } = fullPublication ?? data ?? {};

  const getMethodologyLinks = () => {
    const links: ReactNode[] =
      publication?.methodologies?.map(methodology => (
        <Link key={methodology.id} to={`/methodology/${methodology.slug}`}>
          {methodology.title}
        </Link>
      )) ?? [];

    if (publication?.externalMethodology) {
      links.push(
        <Link
          key={publication.externalMethodology.url}
          to={publication.externalMethodology.url}
        >
          {publication.externalMethodology.title}
        </Link>,
      );
    }
    return links;
  };

  return (
    <LoadingSpinner loading={!fullPublication && isLoading}>
      <TableToolInfo
        contactDetails={publication?.contact}
        methodologyLinks={getMethodologyLinks()}
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
